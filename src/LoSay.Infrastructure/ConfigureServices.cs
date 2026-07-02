using EasyModbus;
using LoSay.Application.Common.Interfaces;
using LoSay.Application.Repositories;
using LoSay.Application.Services;
using LoSay.Data.Contexts;
using LoSay.Domain.Entities;
using LoSay.Infrastructure.Common;
using LoSay.Infrastructure.Configurations;
using LoSay.Infrastructure.Persistence;
using LoSay.Infrastructure.Repositories;
using LoSay.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace LoSay.Infrastructure
{
	public static class ConfigureServices
	{
		public static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
		{

			var plcSettings = configuration.GetSection(key: nameof(PLCSettings)).Get<PLCSettings>();
			services.AddSingleton(plcSettings);
			services.AddSingleton(p =>
			{
				var settings = p.GetRequiredService<PLCSettings>();
				return new ModbusClient
				{
					IPAddress = settings.IPAddress,
					Port = settings.Port,
					ConnectionTimeout = 20000
				};
			});
			return services;
		}
		public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
		{
			//Add Database
			services.AddDatabase(configuration);
			// Add Identity
			services.ConfigureIdentity(configuration);

			//Add Repository
			services.AddRepositories();

			//Add Service
			services.AddDomainServices();

			services.AddScoped(serviceType: typeof(IUnitOfWork<>), implementationType: typeof(UnitOfWork<>));

			//Add SignalR
			services.AddSignalR();

			return services;
		}

		public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
				builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
			});
			services.AddDbContext<UserDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("UserConnection")));

			services.AddScoped(serviceType: typeof(IUnitOfWork<>), implementationType: typeof(UnitOfWork<>));
			return services;
		}
		public static IServiceCollection AddRepositories(this IServiceCollection services)
		{
			services.AddScoped<IMachineRepository, MachineRepository>();
			services.AddScoped<ILotStateRepository, LotStateRepository>();
			services.AddScoped<IPLCDataRepository, PLCDataRepository>();
			services.AddScoped<IItemRepository, ItemRepository>();
			services.AddScoped<IItemDetailRepository, ItemDetailRepository>();
			
			return services;
		}
		public static IServiceCollection AddDomainServices(this IServiceCollection services)
		{
			services.AddScoped<IPLCService, PLCService>();
			services.AddScoped<IMachineService, MachineService>();
			services.AddScoped<ILotStateService, LotStateService>();
			services.AddScoped<IItemService, ItemService>();
			
			services.AddSingleton<PLCReader>();
			services.AddScoped<IAuditLogService, AuditLogService>();

			return services;
		}
		public static void ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddIdentity<User, IdentityRole>(options =>
			{
			}).AddEntityFrameworkStores<UserDbContext>()
			.AddDefaultTokenProviders();
			services.Configure<IdentityOptions>(options =>
			{
				options.Password.RequireDigit = false;
				options.Password.RequiredLength = 5;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireUppercase = false;
			});

			services.AddAuthentication(
				options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				}).AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(
							configuration.GetSection("TokenSettings:Token").Value)),
						ValidateIssuer = false,
						ValidateAudience = false
					};
				});
		}
	}
}
