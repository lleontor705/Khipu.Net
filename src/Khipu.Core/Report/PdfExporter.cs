namespace Khipu.Core.Report;

using System.Diagnostics;

/// <summary>
/// Exportador de HTML a PDF - Paridad Greenter htmltopdf package.
/// Usa un motor externo (wkhtmltopdf o Chrome headless) para convertir HTML a PDF.
/// </summary>
public static class PdfExporter
{
    /// <summary>
    /// Convierte HTML a PDF usando wkhtmltopdf.
    /// Requiere wkhtmltopdf instalado y accesible en PATH.
    /// </summary>
    public static async Task<byte[]> HtmlToPdfAsync(string html, PdfOptions? options = null)
    {
        var opts = options ?? new PdfOptions();
        var tempHtml = Path.Combine(Path.GetTempPath(), $"khipu_{Guid.NewGuid():N}.html");
        var tempPdf = Path.ChangeExtension(tempHtml, ".pdf");

        try
        {
            await File.WriteAllTextAsync(tempHtml, html).ConfigureAwait(false);

            var args = new List<string>
            {
                "--quiet",
                "--encoding", "utf-8",
                "--page-size", opts.PageSize,
                "--margin-top", opts.MarginTop,
                "--margin-bottom", opts.MarginBottom,
                "--margin-left", opts.MarginLeft,
                "--margin-right", opts.MarginRight
            };

            if (opts.Landscape) args.Add("--orientation Landscape");

            args.Add($"\"{tempHtml}\"");
            args.Add($"\"{tempPdf}\"");

            var executable = opts.WkHtmlToPdfPath ?? FindExecutable();

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = string.Join(" ", args),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
            await process.WaitForExitAsync().ConfigureAwait(false);

            if (process.ExitCode != 0 && !File.Exists(tempPdf))
            {
                throw new InvalidOperationException($"wkhtmltopdf falló (exit {process.ExitCode}): {error}");
            }

            return await File.ReadAllBytesAsync(tempPdf).ConfigureAwait(false);
        }
        finally
        {
            if (File.Exists(tempHtml)) File.Delete(tempHtml);
            if (File.Exists(tempPdf)) File.Delete(tempPdf);
        }
    }

    /// <summary>
    /// Guarda HTML como PDF en un archivo.
    /// </summary>
    public static async Task SavePdfAsync(string html, string outputPath, PdfOptions? options = null)
    {
        var pdfBytes = await HtmlToPdfAsync(html, options).ConfigureAwait(false);
        await File.WriteAllBytesAsync(outputPath, pdfBytes).ConfigureAwait(false);
    }

    private static string FindExecutable()
    {
        // Buscar en PATH
        var paths = new[]
        {
            "wkhtmltopdf",
            @"C:\Program Files\wkhtmltopdf\bin\wkhtmltopdf.exe",
            @"C:\Program Files (x86)\wkhtmltopdf\bin\wkhtmltopdf.exe",
            "/usr/local/bin/wkhtmltopdf",
            "/usr/bin/wkhtmltopdf"
        };

        foreach (var path in paths)
        {
            if (path == "wkhtmltopdf") return path; // Let OS resolve from PATH
            if (File.Exists(path)) return path;
        }

        return "wkhtmltopdf"; // Fallback to PATH
    }
}

/// <summary>
/// Opciones para exportación PDF.
/// </summary>
public class PdfOptions
{
    public string PageSize { get; set; } = "A4";
    public string MarginTop { get; set; } = "10mm";
    public string MarginBottom { get; set; } = "10mm";
    public string MarginLeft { get; set; } = "10mm";
    public string MarginRight { get; set; } = "10mm";
    public bool Landscape { get; set; }
    /// <summary>Ruta completa al ejecutable wkhtmltopdf. Si es null, busca en PATH.</summary>
    public string? WkHtmlToPdfPath { get; set; }
}
