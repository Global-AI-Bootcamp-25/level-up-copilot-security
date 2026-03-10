$repoRoot = Join-Path $PSScriptRoot '..'
$clientAppPath = Join-Path $repoRoot 'ClientApp'
$reportDir = Join-Path $repoRoot 'docs\code-review'
$jsonReport = Join-Path $reportDir 'latest-clientapp-npm-audit.json'
$mdReport = Join-Path $reportDir 'latest-clientapp-npm-audit.md'

New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
Set-Location $clientAppPath

$auditOutput = npm audit --json 2>&1
$exitCode = $LASTEXITCODE

$auditText = $auditOutput -join [Environment]::NewLine
Set-Content -Path $jsonReport -Value $auditText -Encoding utf8

$timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
$summaryLines = @(
    '# ClientApp npm audit'
    ''
    "- Generated: $timestamp"
    "- Exit code: $exitCode"
    "- JSON report: docs/code-review/latest-clientapp-npm-audit.json"
    ''
    '## Raw output'
    '```json'
    $auditText
    '```'
)

Set-Content -Path $mdReport -Value $summaryLines -Encoding utf8

Write-Host "Audit reports written:"
Write-Host " - $mdReport"
Write-Host " - $jsonReport"

exit 0