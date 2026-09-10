$ErrorActionPreference = 'Stop'
$payload = @{
    accuracy = 72
    averageTime = 4.8
    mistakes = @('ฤ', 'วรรณยุกต์')
    currentLevel = 4
} | ConvertTo-Json
try {
    Invoke-RestMethod -Uri 'http://127.0.0.1:3000/api/create-practice' -Method Post -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($payload)) | ConvertTo-Json
} catch {
    if ($_.ErrorDetails.Message) { Write-Host $_.ErrorDetails.Message }
    else { Write-Host $_.Exception.Message }
    exit 1
}
