# Source: https://stackoverflow.com/questions/51167245/trigger-an-appveyor-build-on-particular-commit

$myCommit=(&git log -2 --format=%H)[1]

$token = 'nuywbv31cx4fu7ok0i37'
$revision = (git rev-parse HEAD)
git branch
$branch= (&git branch)[1]

$headers = @{
  "Authorization" = "Bearer $token"
  "Content-type" = "application/json"
}

$body = @{
    accountName="arnonax"
    projectSlug="testessentials-92qwm"
    commitId=$myCommit
    branch=$branch
}
$body = $body | ConvertTo-Json

Write-Output "Triggering build:"
Write-Output "Headers: " $headers
Write-Output "Body: " $body
Invoke-RestMethod -Uri 'https://ci.appveyor.com/api/builds' -Headers $headers  -Body $body -Method POST