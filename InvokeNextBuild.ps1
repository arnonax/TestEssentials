﻿# Source: https://stackoverflow.com/questions/51167245/trigger-an-appveyor-build-on-particular-commit
$token = 'nuywbv31cx4fu7ok0i37'
$revision = (git rev-parse HEAD)
$allBranches= &git branch
foreach($branch in $allBranches) {
    if ($branch.StartsWith("* ")) {
        $currentBranch = $branch.Substring(2)
    }
 }

$headers = @{
  "Authorization" = "Bearer $token"
  "Content-type" = "application/json"
}

$body = @{
    accountName="arnonax"
    projectSlug="testessentials-92qwm"
    commitId=$revision
    branch=$currentBranch
}
$body = $body | ConvertTo-Json

Write-Output "Triggering build:"
Write-Output "Headers: " $headers
Write-Output "Body: " $body
Invoke-RestMethod -Uri 'https://ci.appveyor.com/api/builds' -Headers $headers  -Body $body -Method POST