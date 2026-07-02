using LoSay.Infrastructure.Configurations;
using Serilog;

namespace LoSay.WorkerService.Configurations
{
	public static class WokerConfigureServices
	{
		public static IServiceCollection AddWokerConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
		{
			var samplingSettings = configuration.GetSection(key: nameof(SamplingSettings)).Get<SamplingSettings>();
			services.AddSingleton(samplingSettings);

			var signalRConnectURL = configuration.GetSection(key: nameof(SignalRConnectURLSettings)).Get<SignalRConnectURLSettings>();
			services.AddSingleton(signalRConnectURL);

			services.AddSingleton(Log.Logger);
			return services;
		}
	}
}
