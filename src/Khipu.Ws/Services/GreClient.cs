namespace Khipu.Ws.Services;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Khipu.Ws.Constants;
using Khipu.Ws.Helpers;
using Khipu.Ws.Interfaces;
using Khipu.Ws.Models;

/// <summary>
/// Cliente REST para la API GRE (Guía de Remisión Electrónica) de SUNAT.
/// Paridad con Greenter Api.php + GreSender.php + ApiFactory.php
/// Soporta envío de CPE vía REST con OAuth2, como alternativa al SOAP.
/// </summary>
public class GreClient : ISunatClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _username; // RUC + MODDATOS user
    private readonly string _password;
    private readonly string _authEndpoint;
    private readonly string _cpeEndpoint;

    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public GreClient(
        string clientId,
        string clientSecret,
        string ruc,
        string solUser,
        string solPassword,
        string? authEndpoint = null,
        string? cpeEndpoint = null,
        HttpClient? httpClient = null)
    {
        _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
        _username = $"{ruc}{solUser}";
        _password = solPassword ?? throw new ArgumentNullException(nameof(solPassword));
        _authEndpoint = authEndpoint ?? SunatEndpoints.GreAuth;
        _cpeEndpoint = cpeEndpoint ?? SunatEndpoints.GreCpe;
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
    }

    /// <summary>
    /// Envía un comprobante (factura, nota, etc.) a SUNAT vía GRE REST API.
    /// Paridad: GreSender.send()
    /// </summary>
    public async Task<SunatResponse> SendBillAsync(SunatSendRequest request, CancellationToken ct = default)
    {
        try
        {
            await EnsureTokenAsync(ct).ConfigureAwait(false);

            var hashZip = Convert.ToBase64String(SHA256.HashData(request.ZipContent));
            var arcGreZip = Convert.ToBase64String(request.ZipContent);
            var fileName = $"{request.FileNameWithoutExtension}.zip";

            var body = new
            {
                nomArchivo = fileName,
                arcGreZip = arcGreZip,
                hashZip = hashZip
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_cpeEndpoint}/contribuyente/gem/comprobantes/{fileName}")
            {
                Content = JsonContent.Create(body)
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            using var response = await _httpClient.SendAsync(httpRequest, ct).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var greResponse = JsonSerializer.Deserialize<GreSendResponse>(json);
                return new SunatResponse
                {
                    Success = true,
                    Ticket = greResponse?.NumTicket,
                    StatusCode = "0"
                };
            }

            return CreateErrorResponse(response.StatusCode, json);
        }
        catch (Exception ex)
        {
            return new SunatResponse
            {
                Success = false,
                ErrorCode = "GRE_ERROR",
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Envía un resumen o comunicación de baja vía GRE REST API.
    /// Misma lógica que SendBillAsync (GRE usa el mismo endpoint).
    /// </summary>
    public Task<SunatResponse> SendSummaryAsync(SunatSendRequest request, CancellationToken ct = default)
        => SendBillAsync(request, ct);

    /// <summary>
    /// Consulta el estado de un envío por ticket.
    /// Paridad: GreSender.status()
    /// </summary>
    public async Task<TicketResponse> GetStatusAsync(string ticket, CancellationToken ct = default)
    {
        try
        {
            await EnsureTokenAsync(ct).ConfigureAwait(false);

            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_cpeEndpoint}/contribuyente/gem/comprobantes/envios/{ticket}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            using var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return new TicketResponse
                {
                    Success = false,
                    Ticket = ticket,
                    ErrorMessage = json,
                    StatusCode = ((int)response.StatusCode).ToString()
                };
            }

            var statusResponse = JsonSerializer.Deserialize<GreStatusResponse>(json);
            if (statusResponse == null)
            {
                return new TicketResponse
                {
                    Success = false,
                    Ticket = ticket,
                    ErrorMessage = "Respuesta vacía de SUNAT"
                };
            }

            // Greenter: codRespuesta 98 = pendiente, 0/99 = completado
            var result = new TicketResponse
            {
                Success = true,
                Ticket = ticket,
                StatusCode = statusResponse.CodRespuesta
            };

            // Si CDR fue generado, decodificar ZIP
            if (statusResponse.IndCdrGenerado == "1" && !string.IsNullOrEmpty(statusResponse.ArcCdr))
            {
                result.CdrZip = Convert.FromBase64String(statusResponse.ArcCdr);
            }

            return result;
        }
        catch (Exception ex)
        {
            return new TicketResponse
            {
                Success = false,
                Ticket = ticket,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Consulta CDR directamente. En GRE esto se hace via getStatus con ticket.
    /// </summary>
    public Task<CdrResponse> GetCdrAsync(CdrQuery query, CancellationToken ct = default)
    {
        // GRE API no tiene un endpoint directo de consulta de CDR como SOAP.
        // El CDR se obtiene del resultado del getStatus.
        return Task.FromResult(new CdrResponse
        {
            Success = false,
            ErrorCode = "NOT_SUPPORTED",
            ErrorMessage = "GRE API no soporta consulta directa de CDR. Use GetStatusAsync con el ticket."
        });
    }

    /// <summary>
    /// Obtiene o renueva el token OAuth2.
    /// Paridad: ApiFactory.create() + getToken()
    /// </summary>
    private async Task EnsureTokenAsync(CancellationToken ct)
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
            return;

        await _tokenLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // Double-check después de adquirir el lock
            if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
                return;

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("scope", "https://api-cpe.sunat.gob.pe"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("username", _username),
                new KeyValuePair<string, string>("password", _password)
            });

            using var response = await _httpClient.PostAsync(
                $"{_authEndpoint}/clientessol/{_clientId}/oauth2/token",
                formData, ct).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"GRE OAuth2 authentication failed ({(int)response.StatusCode}): {json}");
            }

            var tokenResponse = JsonSerializer.Deserialize<GreTokenResponse>(json);
            if (tokenResponse?.AccessToken == null)
            {
                throw new InvalidOperationException("Token de acceso vacío en respuesta de SUNAT");
            }

            _accessToken = tokenResponse.AccessToken;
            // Renovar 1 minuto antes de expirar (Greenter usa 10 min buffer)
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private static SunatResponse CreateErrorResponse(System.Net.HttpStatusCode statusCode, string body)
    {
        var code = (int)statusCode;
        var message = body;

        try
        {
            var error = JsonSerializer.Deserialize<GreErrorResponse>(body);
            if (error != null)
            {
                message = error.Message ?? error.Detail ?? body;
            }
        }
        catch { /* fallback to raw body */ }

        return new SunatResponse
        {
            Success = false,
            ErrorCode = code.ToString(),
            ErrorMessage = message
        };
    }

    public void Dispose()
    {
        _tokenLock.Dispose();
        GC.SuppressFinalize(this);
    }
}

// ===== Modelos JSON internos para GRE API =====

internal class GreTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

internal class GreSendResponse
{
    [JsonPropertyName("numTicket")]
    public string? NumTicket { get; set; }

    [JsonPropertyName("fecRecepcion")]
    public string? FecRecepcion { get; set; }
}

internal class GreStatusResponse
{
    [JsonPropertyName("codRespuesta")]
    public string? CodRespuesta { get; set; }

    [JsonPropertyName("indCdrGenerado")]
    public string? IndCdrGenerado { get; set; }

    [JsonPropertyName("arcCdr")]
    public string? ArcCdr { get; set; }

    [JsonPropertyName("error")]
    public GreErrorResponse? Error { get; set; }
}

internal class GreErrorResponse
{
    [JsonPropertyName("numError")]
    public string? NumError { get; set; }

    [JsonPropertyName("desError")]
    public string? DesError { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}
