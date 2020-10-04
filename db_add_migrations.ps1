param ([Parameter(Mandatory)]$name)
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:DOTNET_ENVIRONMENT="Development"
dotnet ef migrations add $name --project ./Apps/AppDbApp

$currentDir = (Get-Item .).FullName
Set-Location Apps/AppDbApp
dotnet publish  /p:PublishProfile=Local
Set-Location $currentDir