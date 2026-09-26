. (Join-Path $PSScriptRoot 'Local-Environment.ps1')
Set-LocalConnectionString
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore failed.' }
dotnet ef database update --project src/Uinsure.Api/Uinsure.Api.csproj --startup-project src/Uinsure.Api/Uinsure.Api.csproj
if ($LASTEXITCODE -ne 0) { throw 'Database migration failed.' }
