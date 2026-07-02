using Common.Logging;
using LoSay.Application;
using LoSay.Application.Repositories;
using LoSay.Application.Services;
using LoSay.Infrastructure;
using LoSay.Infrastructure.Repositories;
using LoSay.Infrastructure.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor.Services;
using OxyPlot.Series;
using PLCClient.Components;
using PLCClient.Providers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(Serilogger.Configure);
Log.Information(messageTemplate: "Starting Services...");

// Add MudBlazor services
builder.Services.AddMudServices(config =>
{
	config.SnackbarConfiguration.HideTransitionDuration = 100;
	config.SnackbarConfiguration.ShowTransitionDuration = 100;
	config.SnackbarConfiguration.VisibleStateDuration = 2000;
});

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

// Add services to the container.
builder.Services.AddConfigurationSettings(builder.Configuration);//1
builder.Services.AddApplicationServices();//2
builder.Services.AddInfrastructureServices(configuration: builder.Configuration);//3

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

//Chart
builder.Services.AddOxyPlotBlazor();

// auth
builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<TokenAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
	sp.GetRequiredService<TokenAuthenticationStateProvider>());



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles(); 
app.UseAntiforgery();
app.MapHub<PLCHubService>("/plchub");
app.MapHub<PLCHubService>("/dashboardhub");
app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();
