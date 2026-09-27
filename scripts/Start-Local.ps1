. (Join-Path $PSScriptRoot 'Local-Environment.ps1')
Initialize-LocalEnvironment
Assert-DockerEngine
Invoke-ProjectCompose up -d --wait
Set-LocalConnectionString
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore failed.' }
dotnet ef database update --project src/Uinsure.Api/Uinsure.Api.csproj --startup-project src/Uinsure.Api/Uinsure.Api.csproj
if ($LASTEXITCODE -ne 0) { throw 'Database migration failed.' }
dotnet run --project src/Uinsure.Api/Uinsure.Api.csproj --no-launch-profile --urls http://127.0.0.1:5080
