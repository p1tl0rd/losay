	dotnet ef migrations add "Add_AuditLogs2" --context "ApplicationDbContext" --project LoSay.Infrastructure --startup-project ..\test\PLCClient --output-dir Persistence\Migrations

	dotnet ef migrations remove --context "ApplicationDbContext" --project LoSay.Infrastructure --startup-project ..\test\PLCClient

	dotnet ef database update --context "ApplicationDbContext" --project LoSay.Infrastructure --startup-project ..\test\PLCClient
