param ($env='Test',$drop='No')
$env:ASPNETCORE_ENVIRONMENT=$env
if ($drop -eq 'Yes') {
	dotnet ef database drop -f --project ./Apps/EfMigrationsApp
}
dotnet ef database update --project ./Apps/EfMigrationsApp