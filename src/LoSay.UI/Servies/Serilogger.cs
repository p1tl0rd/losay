using Serilog;

namespace LoSay.Servies
{
	public static class Serilogger
	{
		public static Action<HostBuilderContext, LoggerConfiguration> Configure = (context, configuration) =>
		{
			var applicationName = context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-");
			var environmentName = context.HostingEnvironment.EnvironmentName ?? "Development";

			configuration
				.WriteTo.Debug()
				.WriteTo.Console(outputTemplate:
					"[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
				.Enrich.FromLogContext()
				.Enrich.WithMachineName()
				.Enrich.WithProperty("Environment", environmentName)
				.Enrich.WithProperty("Application", applicationName)
				.ReadFrom.Configuration(context.Configuration)
				.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // Ghi log vào file hàng ngày
				.WriteTo.Seq("http://localhost:5341") // Ghi log ra Seq n?u c?n
				;
			//	.CreateLogger();
		};
	}
}
