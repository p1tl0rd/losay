using LoSay.Application.DTOs;
using LoSay.Application.Services;
using LoSay.Infrastructure.Configurations;
using LoSay.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;


namespace LoSay.WorkerService;

public class Worker2 : BackgroundService
{
	private readonly ILogger _logger;
	private readonly IHubContext<PLCHubService> _hubContext;
	private readonly IServiceProvider _serviceProvider;
	private HubConnection? _connection;
	private HubConnection? _dashboardConnection;
	private SignalRConnectURLSettings _signalRConnectURLSettings;
	private SamplingSettings _settings;
	private bool _isConnectedPLC = false;
	public Worker2(ILogger logger, SamplingSettings settings, IServiceProvider serviceProvider,
			IHubContext<PLCHubService> hubContext, SignalRConnectURLSettings signalRConnectURLSettings)
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
		_hubContext = hubContext;
		_signalRConnectURLSettings = signalRConnectURLSettings;
		_settings = settings;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		//Tạo task ngầm retry kết nối SignalR
		_ = RunSignalRReconnectLoop(stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await RunWorkerLoop(stoppingToken); // chạy code chính
			}
			catch (Exception ex)
			{
				_logger.Error($"Worker crashed: {ex.Message}. Restarting in 5s...");
				_isConnectedPLC = false;
				await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
			}
		}
	}
	private async Task RunWorkerLoop(CancellationToken stoppingToken)
	{
		//SignalR
	//	await ConnectSignalR(stoppingToken);

		using var timer = new PeriodicTimer(TimeSpan.FromSeconds(int.Parse(_settings.samp)));

		while (!stoppingToken.IsCancellationRequested)
		{
			//Delay thời gian.
			if (!await timer.WaitForNextTickAsync(stoppingToken)) break;

			using var scope = _serviceProvider.CreateScope();
			var _plcService = scope.ServiceProvider.GetRequiredService<IPLCService>();
			var _machineService = scope.ServiceProvider.GetRequiredService<IMachineService>();
			var _lotStateService = scope.ServiceProvider.GetRequiredService<ILotStateService>();

			if (!_isConnectedPLC)
			{
				_isConnectedPLC = await _plcService.ConnectPLCAsync();
				if (!_isConnectedPLC) throw new Exception("Cannot connect PLC");

			}

			var machines = await _machineService.GetMachines();
			List<PLCDataDto> listDataPLC = new List<PLCDataDto>();
			foreach (var machine in machines)
			{
				try
				{
					var plcData = await _plcService.ReadPLCConsecutiveRawValuesAsync(machine, 5);

					//Lấy trạng thái lotno mới nhất từ database trong mỗi lần lặp
					var currentLotNo = await _lotStateService.GetCurrentLotNoAsync(machineId: machine.Id);
					var lotNo = currentLotNo != null ? currentLotNo : null;

				// Kiểm tra trạng thái Máy đang chạy hay không. plcData.Value4 là trạng thái của máy.
				// convertedValue[3] == 13740 là máy đang dừng; < 13740 là máy đang chạy.
				if (plcData.Value4 < 13740 && lotNo == null)
				{
					var lotStateDto = await _lotStateService.StartLotAutoAsync(machine);
					lotNo = lotStateDto;
				}
				if (plcData.Value4 == 13740 && lotNo != null)
				{
					await _lotStateService.FinishLotAsync(machine.Id);
				}

					plcData.LotNo = lotNo != null ? lotNo.LotNo : null;
					plcData.LotStateDto = lotNo;

					await _plcService.CreatePLCData(plcData);

					listDataPLC.Add(plcData);

					//Send Data to Machine detail 
					if (_connection?.State == HubConnectionState.Connected)
					{
						try
						{
							await _connection.InvokeAsync("SendPlcDataToGroup", machine.Id, plcData);
						}
						catch (Exception ex)
						{
							_logger.Warning($"SendPlcData failed: {ex.Message}");
						}
					}
				}
				catch (Exception ex)
				{
					_logger.Error($"Co loi xay ra: {ex.Message}");
				}
			}
			// Gửi cho dashboard
			if (_dashboardConnection?.State == HubConnectionState.Connected)
			{
				try
				{
					var machineWithLotno = await _machineService.GetMachinesWithLotInfoAsync(listDataPLC);
					await _dashboardConnection.InvokeAsync("SendAllMachineStatus", machineWithLotno);
				}
				catch (Exception ex)
				{
					_logger.Warning($"SendAllMachineStatus failed: {ex.Message}");
				}
			}

			_logger.Information($"Lay Du Lieu Thanh Cong. Delay: {timer.Period} ");

		}
	}
	private async Task ConnectSignalR(CancellationToken stoppingToken)
	{
		_logger.Information("Connecting SignalR...");
		if (_connection != null)
		{
			await _connection.DisposeAsync();
			_connection = null;
		}

		if (_dashboardConnection != null)
		{
			await _dashboardConnection.DisposeAsync();
			_dashboardConnection = null;
		}

		try
		{
			// SigalR Machine Detail
			_connection = new HubConnectionBuilder()
			   .WithUrl(_signalRConnectURLSettings.URL, options =>
			   {
				   options.HttpMessageHandlerFactory = (handler) =>
				   {
					   // Bỏ kiểm tra SSL certificate (chỉ dùng khi dev)
					   if (handler is HttpClientHandler clientHandler)
						   clientHandler.ServerCertificateCustomValidationCallback +=
							   (sender, cert, chain, sslPolicyErrors) => true;
					   return handler;
				   };
			   })
			   .WithAutomaticReconnect()
			   .Build();

			await _connection.StartAsync();
			_logger.Information("Worker connected to MachineDetail Hub!");
		}
		catch (Exception ex)
		{
			_logger.Warning($"Cannot connect to MachineDetail Hub: {ex.Message}");
			_connection = null; // đánh dấu không kết nối được
		}

		try
		{
			// SignalR Dashboard
			_dashboardConnection = new HubConnectionBuilder()
			   .WithUrl(_signalRConnectURLSettings.URLDashboard, options =>
			   {
				   options.HttpMessageHandlerFactory = (handler) =>
				   {
					   // Bỏ kiểm tra SSL certificate (chỉ dùng khi dev)
					   if (handler is HttpClientHandler clientHandler)
						   clientHandler.ServerCertificateCustomValidationCallback +=
							   (sender, cert, chain, sslPolicyErrors) => true;
					   return handler;
				   };
			   })
			   .WithAutomaticReconnect()
			   .Build();

			await _dashboardConnection.StartAsync();
			_logger.Information("Worker connected to Dashboard Hub!");
		}
		catch (Exception ex)
		{
			_logger.Warning($"Cannot connect to Dashboard Hub: {ex.Message}");
			_dashboardConnection = null;
		}
	}
	private async Task RunSignalRReconnectLoop(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			if (_connection == null || _connection.State != HubConnectionState.Connected ||
				_dashboardConnection == null || _dashboardConnection.State != HubConnectionState.Connected)
			{
				_logger.Information("Reconnecting SignalR...");
				await ConnectSignalR(stoppingToken);
			}

			await Task.Delay(10000, stoppingToken);
		}
	}
}
