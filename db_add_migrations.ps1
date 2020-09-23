param ([Parameter(Mandatory)]$name)
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet ef migrations add $name --project ./Apps/EfMigrationsApp