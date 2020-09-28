param ($env='Test')
$env:ASPNETCORE_ENVIRONMENT=$env
dotnet ef database update --project ./Apps/EfMigrationsApp