$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot 'src\KhmerAstrology.WinForms\KhmerAstrology.WinForms.csproj'
$installerScript = Join-Path $PSScriptRoot 'KhmerAstrology.iss'
$publishDir = Join-Path $PSScriptRoot 'src\KhmerAstrology.WinForms\bin\Release\net10.0-windows\win-x64\publish'

dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

$compiler = (Get-Command iscc.exe -ErrorAction SilentlyContinue).Source
if (-not $compiler) {
    $candidates = @(
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')
    ) | Where-Object { Test-Path $_ }
    $compiler = $candidates | Select-Object -First 1
}

if (-not $compiler) {
    throw "Publish succeeded to '$publishDir', but Inno Setup 6 was not found. Install Inno Setup 6 and rerun this script to create installer\KhmerAstrology-Setup.exe."
}

& $compiler $installerScript
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup compilation failed with exit code $LASTEXITCODE"
}

Write-Host "Installer created: $(Join-Path $PSScriptRoot 'installer\KhmerAstrology-Setup.exe')"
