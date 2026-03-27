# GitHub Packages - NuGet Privado

## Configuración

### 1. Crear Personal Access Token (PAT)

1. Ve a GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
   - URL: https://github.com/settings/tokens/new

2. Selecciona los scopes:
   - write:packages - Para publicar packages
   - ead:packages - Para descargar packages
   - epo - Si el repo es privado

3. Genera el token y guárdalo (solo se muestra una vez)

### 2. Configurar nuget.config

Reemplaza GITHUB_TOKEN en 
uget.config con tu token:

`xml
<add key=""ClearTextPassword"" value=""ghp_tu_token_aqui"" />
`

### 3. Publicar en GitHub Packages

`powershell
# Configurar source
dotnet nuget add source --username lleontor705 --password GITHUB_TOKEN --store-password-in-clear-text --name github ""https://nuget.pkg.github.com/lleontor705/index.json""

# Publicar packages
dotnet nuget push artifacts\Khipu.Core.0.1.0-alpha.nupkg --source github
dotnet nuget push artifacts\Khipu.Data.0.1.0-alpha.nupkg --source github
dotnet nuget push artifacts\Khipu.Xml.0.1.0-alpha.nupkg --source github
dotnet nuget push artifacts\Khipu.Ws.0.1.0-alpha.nupkg --source github
`

### 4. Usar el package en otros proyectos

Crea o edita 
uget.config en tu proyecto:

`xml
<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""github"" value=""https://nuget.pkg.github.com/lleontor705/index.json"" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key=""Username"" value=""lleontor705"" />
      <add key=""ClearTextPassword"" value=""GITHUB_TOKEN"" />
    </github>
  </packageSourceCredentials>
</configuration>
`

Instala el package:

`ash
dotnet add package Khipu.Core --version 0.1.0-alpha
`

## Script de publicación automática

`powershell
# publish-github.ps1
 = ""GITHUB_TOKEN""
 = ""lleontor705""

# Configurar source
dotnet nuget add source --username  --password  --store-password-in-clear-text --name github ""https://nuget.pkg.github.com//index.json""

# Publicar todos los packages
Get-ChildItem artifacts\*.nupkg | ForEach-Object {
    Write-Host ""Publicando ""
    dotnet nuget push  --source github
}
`

## Ventajas de GitHub Packages

- ✅ Privado (solo tú y quien tú autorices)
- ✅ Integrado con GitHub
- ✅ Gratis para repos privados
- ✅ Control de versiones
- ✅ CI/CD friendly
