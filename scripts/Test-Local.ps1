$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $projectRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'Tool restore failed.' }
    dotnet restore Uinsure.slnx --locked-mode
    if ($LASTEXITCODE -ne 0) { throw 'Locked restore failed.' }
    dotnet format Uinsure.slnx --verify-no-changes --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Formatting check failed.' }
    dotnet build Uinsure.slnx --configuration Release --no-restore --disable-build-servers --verbosity minimal -m:1
    if ($LASTEXITCODE -ne 0) { throw 'Release build failed.' }
    dotnet test Uinsure.slnx --configuration Release --no-build --disable-build-servers --logger trx --results-directory TestResults
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
}
finally { Pop-Location }
