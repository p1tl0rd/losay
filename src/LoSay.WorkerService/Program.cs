using LoSay.Application;
using LoSay.Infrastructure;
using LoSay.WorkerService;
using LoSay.WorkerService.Configurations;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Serilog;


Log.Logger = new LoggerConfiguration()
	  .ReadFrom.Configuration(new ConfigurationBuilder()
		.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
		.Build())
	.WriteTo.Debug()
	.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
	.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
	.Enrich.FromLogContext()
	.WriteTo.Seq("http://localhost:5341")
	.CreateLogger();

try
{
	Log.Information(messageTemplate: "Starting Background Service...");

	var builder = Host.CreateApplicationBuilder(args);

	// Kết nối SeriLog vào Log mặc định.
	builder.Logging.ClearProviders();
	builder.Logging.AddSerilog();

	// Add services to the container.
	builder.Services.AddConfigurationSettings(builder.Configuration);//1
	builder.Services.AddWokerConfigurationSettings(builder.Configuration);//1
	builder.Services.AddApplicationServices();//2
	builder.Services.AddInfrastructureServices(configuration: builder.Configuration);//3
	
	builder.Services.AddHostedService<Worker2>();
	var host = builder.Build();


	host.Run();
}
catch (Exception ex)
{
	string type = ex.GetType().Name;
	if (type.Equals("StopTheHostException", StringComparison.Ordinal)) throw;

	Log.Fatal(ex, "Background service terminated unexpectedly!");
}
finally
{
	Log.Information(messageTemplate: "Shut down Service");
	Log.CloseAndFlush();
}



