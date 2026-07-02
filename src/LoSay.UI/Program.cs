using LoSay.BackgroundServices;
using LoSay.Components;
using LoSay.Data.Contexts;
using LoSay.Data.Repositories.Implementations;
using LoSay.Data.Repositories.Interfaces;
using LoSay.Servies;
using LoSay.Servies.Implementations;
using LoSay.Servies.Interfaces;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Log.Information(messageTemplate: "Starting Services...");

// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),p=>p.CommandTimeout(300)));
 
builder.Services.AddTransient<DbInitializer>();

builder.Services.AddSignalR();
// ?? B?t n�n d? li?u cho SignalR
builder.Services.AddResponseCompression(opts =>
{
	opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
		["application/octet-stream"]);
});


// Đọc cấu hình từ appsettings.json
var plcConfig = builder.Configuration.GetSection("PLCSettings");
// Lấy IP và Port từ appsettings.json
string plcIp = plcConfig.GetValue<string>("IPAddress") ?? "127.0.0.1";
int plcPort = plcConfig.GetValue<int>("Port", 502);

// Đăng ký PLCReader với IP từ appsettings.json
builder.Services.AddSingleton<PLCReader>(sp => new PLCReader(plcIp, plcPort));
builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<IPLCService, PLCService>();
builder.Services.AddScoped<ILotStateRepository, EfLotStateRepository>();
builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IPrinter, PrintDocument>();
// �ang k� Background Services
builder.Services.AddSingleton<PLCDataFetcher>();
builder.Services.AddHostedService<PLCDataFetcher>();
// C?u h�nh Host d? BackgroundService l?i kh�ng d?ng host
builder.Services.Configure<HostOptions>(options =>
{
	options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

//Chart
builder.Services.AddOxyPlotBlazor();

builder.Services.AddServerSideBlazor()
	.AddCircuitOptions(options => { options.DetailedErrors = true; });
// T�ch h?p Serilog v�o ?ng d?ng Blazor Server
builder.Host.UseSerilog(Serilogger.Configure);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
//Seed Data
using (var scope = app.Services.CreateScope())
{
	try
	{
		var dbInitializer = scope.ServiceProvider.GetService<DbInitializer>();
		dbInitializer.Seed().Wait();
	}
	catch (Exception ex)
	{
		Console.WriteLine(ex.ToString());
	}

}
app.UseStaticFiles(); // Cho ph�p ph?c v? file t? wwwroot
app.UseAntiforgery();
// C?u h�nh endpoint cho SignalR
app.MapHub<PlcHub>("/plchub");

app.MapHub<MachineHub>("/machinehub");

app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();
