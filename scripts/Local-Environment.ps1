Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:ProjectRoot = Split-Path -Parent $PSScriptRoot
$script:LocalDirectory = Join-Path $script:ProjectRoot '.local'
$script:SqlEnvironmentFile = Join-Path $script:LocalDirectory 'sql.env'
$script:DatabaseName = 'UinsureAssessment'

function Initialize-LocalEnvironment {
    if (-not (Test-Path -LiteralPath $script:LocalDirectory)) {
        New-Item -ItemType Directory -Path $script:LocalDirectory | Out-Null
    }
    if (-not (Test-Path -LiteralPath $script:SqlEnvironmentFile)) {
        $random = [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(24))
        Set-Content -LiteralPath $script:SqlEnvironmentFile -Value "MSSQL_SA_PASSWORD=U!${random}a1" -NoNewline
    }
}

function Get-LocalSqlPassword {
    Initialize-LocalEnvironment
    $line = Get-Content -LiteralPath $script:SqlEnvironmentFile -Raw
    if ($line -notmatch '^MSSQL_SA_PASSWORD=(.+)$') { throw 'Invalid ignored SQL environment file.' }
    return $Matches[1]
}

function Set-LocalConnectionString {
    $password = Get-LocalSqlPassword
    $env:ConnectionStrings__Uinsure = "Server=127.0.0.1,14333;Database=$script:DatabaseName;User Id=sa;Password=$password;Encrypt=True;TrustServerCertificate=True"
}

function Assert-DockerEngine {
    docker info --format '{{.ServerVersion}}' *> $null
    if ($LASTEXITCODE -ne 0) { throw 'Docker CLI is installed, but the Linux engine is unavailable.' }
}

function Invoke-ProjectCompose {
    param([Parameter(Mandatory, ValueFromRemainingArguments)][string[]] $Arguments)
    docker compose --project-directory $script:ProjectRoot --env-file $script:SqlEnvironmentFile @Arguments
    if ($LASTEXITCODE -ne 0) { throw "docker compose failed with exit code $LASTEXITCODE." }
}
