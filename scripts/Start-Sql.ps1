. (Join-Path $PSScriptRoot 'Local-Environment.ps1')
Initialize-LocalEnvironment
Assert-DockerEngine
Invoke-ProjectCompose up -d --wait
Write-Output 'SQL Server is healthy on 127.0.0.1:14333.'
