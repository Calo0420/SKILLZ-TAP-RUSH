param(
    [Parameter(Mandatory = $true)]
    [string]$Command,

    [string]$Text = "",

    [float]$Number = 0
)

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$bridgeRoot = Join-Path $projectRoot "CopilotBridge"
$commandPath = Join-Path $bridgeRoot "command.json"
$responsePath = Join-Path $bridgeRoot "response.json"

New-Item -ItemType Directory -Force -Path $bridgeRoot | Out-Null

$payload = [ordered]@{
    id = [guid]::NewGuid().ToString("N")
    command = $Command
    text = $Text
    number = $Number
}

$payload | ConvertTo-Json | Set-Content -Path $commandPath -Encoding UTF8

"Command written: $commandPath"
"Waiting for response..."

$deadline = (Get-Date).AddSeconds(10)
while ((Get-Date) -lt $deadline) {
    if (Test-Path $responsePath) {
        Get-Content $responsePath -Raw
        exit 0
    }
    Start-Sleep -Milliseconds 100
}

Write-Error "No response file found within timeout: $responsePath"
exit 1
