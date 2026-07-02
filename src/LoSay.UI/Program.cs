using Common.Logging;
using LoSay.Application;
using LoSay.Components;
using LoSay.Infrastructure;
using LoSay.Infrastructure.Persistence;
using LoSay.Infrastructure.Services;
using Microsoft.AspNetCore.ResponseCompression;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Log.Information("Starting LoSay UI host...");

builder.Services.AddMudServices();
builder.Services.AddOxyPlotBlazor();

builder.Services.AddConfigurationSettings(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddTransient<DbInitializer>();

builder.Services.AddResponseCompression(opts =>
{
	opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
		["application/octet-stream"]);
});

builder.Services.Configure<HostOptions>(options =>
{
	options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddServerSideBlazor()
	.AddCircuitOptions(options => { options.DetailedErrors = true; });

builder.Host.UseSerilog(Serilogger.Configure);

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

using (var scope = app.Services.CreateScope())
{
	try
	{
		var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
		await dbInitializer.Seed();
	}
	catch (Exception ex)
	{
		Console.WriteLine($"DB seed error: {ex}");
	}
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapHub<PLCHubService>("/plchub");
app.MapHub<PLCHubService>("/dashboardhub");

app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();
