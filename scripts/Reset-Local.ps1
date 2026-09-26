[CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'High')]
param()
. (Join-Path $PSScriptRoot 'Local-Environment.ps1')
$resolvedRoot = (Resolve-Path -LiteralPath $script:ProjectRoot).Path
if ((Split-Path -Leaf $resolvedRoot) -ne 'uinsure-policy-assessment') { throw "Unexpected project root: $resolvedRoot" }
Initialize-LocalEnvironment
if ($PSCmdlet.ShouldProcess('uinsure-policy-assessment containers and SQL volume', 'Remove')) {
    Invoke-ProjectCompose down --volumes --remove-orphans
}
