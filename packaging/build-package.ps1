[CmdletBinding()]
param(
    [Parameter()][string] $Version = '0.1.0.0',
    [Parameter()][string] $Configuration = 'Release',
    [Parameter()][string] $Repository = 'CCDASTRO/TonightsBest-NINA'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$output = Join-Path $root 'artifacts'
$stage = Join-Path $output 'package'
$pluginOutput = Join-Path $root "src\TonightsBest.NINA.Plugin\bin\$Configuration\net8.0-windows"
$archiveName = "TonightsBest.NINA.Plugin.$Version.zip"
$archive = Join-Path $output $archiveName

dotnet build (Join-Path $root 'TonightsBest.NINA.sln') --configuration $Configuration
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
dotnet test (Join-Path $root 'TonightsBest.NINA.sln') --configuration $Configuration --no-build
if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }

if (Test-Path -LiteralPath $stage) { Remove-Item -LiteralPath $stage -Recurse -Force }
New-Item -ItemType Directory -Path $stage -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $pluginOutput 'TonightsBest.NINA.Plugin.dll') -Destination $stage
Copy-Item -LiteralPath (Join-Path $pluginOutput 'TonightsBest.Core.dll') -Destination $stage
if (Test-Path -LiteralPath $archive) { Remove-Item -LiteralPath $archive -Force }
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $archive -CompressionLevel Optimal

$checksum = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
$parts = $Version.Split('.')
if ($parts.Count -ne 4) { throw 'Version must contain four numeric parts, for example 0.1.0.0.' }
$manifest = [ordered]@{
    Name = "Tonight's Best"
    Identifier = '2b3210d8-342c-473d-9d26-c417a52ef803'
    Version = [ordered]@{ Major=$parts[0]; Minor=$parts[1]; Patch=$parts[2]; Build=$parts[3] }
    Author = 'Chuck Faranda / CCDASTRO'
    Homepage = "https://github.com/$Repository"
    Repository = "https://github.com/$Repository"
    License = 'MIT'
    LicenseURL = 'https://opensource.org/license/mit'
    ChangelogURL = "https://github.com/$Repository/blob/main/CHANGELOG.md"
    Tags = @('planning','sky atlas','framing','targets','moon')
    MinimumApplicationVersion = [ordered]@{ Major='3'; Minor='2'; Patch='0'; Build='9001' }
    Descriptions = [ordered]@{
        ShortDescription = "Ranks tonight's best deep-sky imaging targets for the active N.I.N.A. profile and equipment."
        LongDescription = 'Shows a dockable Top 15 list with object type, score, frame coverage, visibility, altitude, Moon separation, magnitude, and Framing Assistant handoff.'
    }
    Installer = [ordered]@{
        URL = "https://github.com/$Repository/releases/download/v$Version/$archiveName"
        Type = 'ARCHIVE'
        Checksum = $checksum
        ChecksumType = 'SHA256'
    }
}
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $output 'manifest.json') -Encoding utf8
Remove-Item -LiteralPath $stage -Recurse -Force
Write-Host "Created $archive"
Write-Host "Created $(Join-Path $output 'manifest.json')"
