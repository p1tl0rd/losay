using System;
using LoSay.Data.Contexts;
using LoSay.Data.Entities;
using LoSay.Servies;
using LoSay.Servies.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LoSay.BackgroundServices
{
	public class PLCDataFetcher : BackgroundService
	{
		private readonly ILogger<PLCDataFetcher> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly PeriodicTimer _timer; // Đọc dữ liệu mỗi 5 giây (ví dụ)
		private readonly IHubContext<PlcHub> _hubContext; // Inject IHubContext
		private readonly IConfiguration _configuration; // Inject IConfiguration

		public PLCDataFetcher(ILogger<PLCDataFetcher> logger, IServiceProvider serviceProvider, IHubContext<PlcHub> hubContext, IConfiguration configuration)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_hubContext = hubContext;
			_configuration = configuration;

			// Đọc giá trị sampling từ configuration
			var samplingInterval = _configuration.GetSection("Sampling")["samp"];
			if (int.TryParse(samplingInterval, out int interval))
			{
				_timer = new PeriodicTimer(TimeSpan.FromSeconds(interval));
				_logger.LogInformation($"PLC Data Fetcher sampling interval set to {interval} seconds.");
			}
			else
			{
				_timer = new PeriodicTimer(TimeSpan.FromSeconds(30)); // Giá trị mặc định nếu không đọc được
				_logger.LogWarning("Could not read sampling interval from configuration. Using default 30 seconds.");
			}

		}
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("PLC Data Fetcher is starting.");

			// Tạo kết nối đến PLC
			bool isConnected = false;
			while (!stoppingToken.IsCancellationRequested)
			{
				using var scope = _serviceProvider.CreateScope();
				// Tạo scope services để gọi các services
				var plcService = scope.ServiceProvider.GetRequiredService<IPLCService>();

				isConnected = await plcService.ConnectPLCAsync();
				if (!isConnected)
				{
					_logger.LogError("Delay 10s trước khi ket noi lai.");
					await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
					continue;
				}
				else
				{
					// Khởi tạo dbContext để làm việc với database
					var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

					// Vòng lặp để services chạy ngầm lấy dữ liệu liên tục.
					//	while (!stoppingToken.IsCancellationRequested && await _timer.WaitForNextTickAsync(stoppingToken))
					while (!stoppingToken.IsCancellationRequested)
					{
						try
						{
							if (!await _timer.WaitForNextTickAsync(stoppingToken)) break;

							// Lấy danh sách máy
							var machines = await dbContext.Machines.Where(machine => machine.Status == true).ToListAsync(stoppingToken);
							foreach (var item in machines)
							{
								using (var taskScope = _serviceProvider.CreateScope())
								{
									var taskDbContext = taskScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

									// ✅ Lấy dữ liệu SẠCH từ Service (Đã chia 10.0, đã xử lý LotNo)
									var processedData = await plcService.GetProcessedPlcDataAsync(item.Id, item.Address);

									if (!processedData.IsSuccess)
									{
										_logger.LogWarning($"Không đọc được dữ liệu hợp lệ từ máy {item.MachineName}");
										return; // Bỏ qua lần đọc này nếu lỗi
									}

									try
									{
										// ✅ Push dữ liệu real-time đến các client trong nhóm máy
										await _hubContext.Clients.Group($"Machine_{item.Id}")
											.SendAsync("ReceivePlcValue", item.Id, processedData.LotNo,
												processedData.Values[0], processedData.Values[1], processedData.Values[2], processedData.Values[3], processedData.Values[4]);

										taskDbContext.PLCDatas.Add(new PLCData
										{
											ItemCodeId = 1,
											MachineId = item.Id,
											LotNo = processedData.LotNo,
											Value1 = processedData.Values[0],
											Value2 = processedData.Values[1],
											Value3 = processedData.Values[2],
											Value4 = processedData.Values[3],
											Value5 = processedData.Values[4],
											Timestamp = DateTime.Now
								});
								await taskDbContext.SaveChangesAsync();
								_logger.LogInformation("Data save to SQL successfully");
									}
									catch (Exception ex)
									{
										_logger.LogError("Lỗi trong khối Try-catch bên trong: int[] values = await plcService.ReadPLCConsecutiveRawValuesAsync(addressList.First(), 5)");

										await _hubContext.Clients.Group($"Machine_{item.Id}")
										.SendAsync("ReceivePlcValue", item.Id, await plcService.GetCurrentLotNoAsync(item.Id),
											0, 0, 0, 0, 0);

										taskDbContext.PLCDatas.Add(new PLCData
										{
											ItemCodeId = 1,
											MachineId = item.Id,
											Value1 = 0,
											Value2 = 0,
											Value3 = 0,
											Value4 = 0,
											Value5 = 0,
											Timestamp = DateTime.Now
										});
										await taskDbContext.SaveChangesAsync();
									}
								}
							}

						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Lỗi khi chạy BackgroundService. Đang chờ 10 giây trước khi thử lại.");
							await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
						}

					}
				}
			}
		}
		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogWarning("PLC Data Fetcher is stopping gracefully.");
			await base.StopAsync(cancellationToken);

			// Gọi phương thức Restart lại service
			OnShutdown();

		}
		// Khởi động lại service sau khi dừng
		private void OnShutdown()
		{
			_logger.LogInformation("Restarting PLC Data Fetcher...");

			// Tạo lại service thông qua DI container
			RestartService();
		}
		// Tạo lại service và gọi ExecuteAsync
		private void RestartService()
		{
			var scope = _serviceProvider.CreateScope();
			var newService = scope.ServiceProvider.GetRequiredService<PLCDataFetcher>();

			// Chạy lại service
			Task.Run(() => newService.ExecuteAsync(CancellationToken.None));
		}
	}
}
