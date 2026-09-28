param([switch]$Serve)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path -Parent $PSScriptRoot)
try {
    dotnet restore Uinsure.slnx --locked-mode
    if ($LASTEXITCODE -ne 0) { throw 'Locked restore failed.' }
    dotnet build Uinsure.slnx --configuration Release --no-restore --disable-build-servers --verbosity minimal -m:1
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    $runnerArguments = @()
    if ($Serve) { $runnerArguments += '--serve' }
    dotnet tests/Uinsure.FullStack/bin/Release/net10.0/Uinsure.FullStack.dll @runnerArguments
    if ($LASTEXITCODE -ne 0) { throw 'Full-stack verification failed.' }
}
finally { Pop-Location }
