. (Join-Path $PSScriptRoot 'Local-Environment.ps1')
Initialize-LocalEnvironment
Invoke-ProjectCompose down
Write-Output 'Local containers stopped; SQL data was retained.'
