using EasyModbus;
using LoSay.Data.Contexts;
using LoSay.Data.Entities;
using LoSay.Data.Repositories.Interfaces;
using LoSay.Servies.Interfaces;
using LoSay.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Servies.Implementations
{
	public class PLCService : IPLCService, IAsyncDisposable, IDisposable
	{
		private readonly PLCReader _plcReader;
		private readonly ILotStateRepository _lotStateRepository;
		private Dictionary<int, string?> _currentLotNos = new Dictionary<int, string?>();
		private Dictionary<int, bool> _isLotRunning = new Dictionary<int, bool>();
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<PLCService> _logger;
		public PLCService(IConfiguration configuration, ILotStateRepository lotStateRepository, ILogger<PLCService> logger, IServiceScopeFactory scopeFactory)
		{
			var plcIp = configuration.GetSection("PLCSettings")["IPAddress"];
			var plcPort = int.Parse(configuration.GetSection("PLCSettings")["Port"] ?? "502");
			_plcReader = new PLCReader(plcIp, plcPort);
			_lotStateRepository = lotStateRepository;
			_logger = logger;
			_scopeFactory = scopeFactory;
		}
		public async Task<bool> ConnectPLCAsync()
		{

			var isConntected = await _plcReader.Connect();
			if (isConntected)
			{
				_logger.LogInformation($" Ket noi PLC thanh cong.");
			}
			else
			{
				_logger.LogError($" Loi ket noi PLC");
			}
			return isConntected;
		}

		public async Task DisconnectPLCAsync()
		{
			await _plcReader.Disconnect();
		}

		public async ValueTask DisposeAsync()
		{
			await DisconnectPLCAsync();
			// Dispose managed resources if any
		}
		public async Task<int[]> ReadPLCConsecutiveRawValuesAsync(int startAddress, int quantity)
		{
			return await _plcReader.ReadConsecutiveRegistersAsync(startAddress, quantity);
		}

		public async Task<int[]> ReadPLCListRawValueAsync(int[] addresses, int quantity)
		{
			return await _plcReader.ReadListRegisterAsync(addresses, quantity);
		}

		public async Task<int> ReadPLCRawValueAsync(int address)
		{
			return await _plcReader.ReadRegisterAsync(address);
		}

		public async Task<ProcessedPlcData> GetProcessedPlcDataAsync(int machineId, string addressConfig)
		{
			try
			{
				// 1. Lọc và lấy địa chỉ đầu tiên từ chuỗi (VD: "0,1,2,3,4")
				int[] addressList = addressConfig.Split(',').Select(int.Parse).ToArray();
				int startAddress = addressList.First();

				// 2. Kéo Data Gốc từ máy PLC
				int[] values = await ReadPLCConsecutiveRawValuesAsync(startAddress, 5);

				if (values == null || values.Length < 5)
				{
					_logger.LogWarning($"PLC trả về mảng dữ liệu không hợp lệ cho máy {machineId}");
					return new ProcessedPlcData { IsSuccess = false };
				}

				// 3. Xử lý Toán học (Chia 10.0 cho các giá trị thực tế)
				double[] convertedValues = values.Select(v => (double)v / 10.0).ToArray();

				// 4. Xử lý Logic (Start Lot, Stop Lot)
				// Lấy trạng thái lot hiện tại
				string? currentLotNo = await GetCurrentLotNoAsync(machineId);
				bool isLotRunning = !string.IsNullOrEmpty(currentLotNo);
				string? finalLotNo = isLotRunning ? currentLotNo : null;

				// Kiểm tra trạng thái máy dựa vào thanh ghi số 3 (value[3])
				// value[3] == 13740 là máy dừng, nhỏ hơn 13740 là máy chạy
				if (values[3] < 13740 && finalLotNo == null)
				{
					finalLotNo = await StartLotAutoAsync(machineId);
				}
				else if (values[3] == 13740 && finalLotNo != null)
				{
					await FinishLotAsync(machineId);
					// Khi tắt máy, vẫn trả về LotNo của mẻ vừa chạy xong để lưu frame cuối cùng
				}

				// 5. Trả Dữ liệu Sạch về cho Fetcher
				return new ProcessedPlcData
				{
					IsSuccess = true,
					LotNo = finalLotNo,
					Values = convertedValues
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Lỗi nghiêm trọng khi xử lý dữ liệu PLC cho máy {machineId}");
				return new ProcessedPlcData { IsSuccess = false };
			}
		}

		// Xử lý Lot

		public async Task StartLotAsync(int machineId, string lotNo)
		{
			await _lotStateRepository.StartLotAsync(machineId, lotNo);
			_currentLotNos[machineId] = lotNo;
			_isLotRunning[machineId] = true;
		}
		/// <summary>
		/// Tự động tạo Lotno khi bấm Run trên máy
		/// </summary>
		/// <param name="machineId"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public async Task<string> StartLotAutoAsync(int machineId)
		{
			// Bắt đầu lô mới
			var _lotNo = await GenerateNewLotNoAsync(machineId);

			await _lotStateRepository.StartLotAsync(machineId, _lotNo);
			_currentLotNos[machineId] = _lotNo;
			_isLotRunning[machineId] = true;

			return _lotNo;

		}
		public async Task<string> GenerateNewLotNoAsync(int Id)
		{
			string datePart = DateTime.Now.ToString("ddMMyy");

			// Đếm số lot đã có trong ngày hôm nay
			using var scope = _scopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			var countToday = await dbContext.LotStates
				.CountAsync(l => l.StartTime.HasValue
								&& l.StartTime.Value.Date == DateTime.Today);
			var machineName = await dbContext.Machines.Where(m => m.Id == Id).SingleOrDefaultAsync();
			var _machineName = machineName.MachineName;


			int nextNumber = countToday + 1;

			string newLotNo = $"{_machineName}_LotNo_{datePart}_{nextNumber}";

			return newLotNo;
		}

		public async Task FinishLotAsync(int machineId)
		{
			await _lotStateRepository.FinishLotAsync(machineId);
			_currentLotNos[machineId] = null;
			_isLotRunning[machineId] = false;
		}

		public async Task<string?> GetCurrentLotNoAsync(int machineId)
		{
			var lotState = await _lotStateRepository.GetCurrentLotStateAsync(machineId);
			return lotState?.LotNo;
		}

		public PLCReader GetPLCReader()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			DisposeAsync().GetAwaiter().GetResult(); // Gọi DisposeAsync() đồng bộ
		}

		public async Task<DateTime?> GetLotStartTimeAsync(int machineId)
		{
			using (var scope = _scopeFactory.CreateScope())
			{
				var lotStateRepository = scope.ServiceProvider.GetRequiredService<ILotStateRepository>();
				try
				{
					return await lotStateRepository.GetLotStartTimeAsync(machineId);
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error getting lot start time for machine {machineId}: {ex.Message}");
					throw;
				}
			}
		}
		public async Task<List<PLCData>> GetHistoricalPlcDataAsync(int machineId, TimeSpan timeSpan)
		{
			using var scope = _scopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var endTime = DateTime.Now;
			var startTime = endTime - timeSpan;
			return await dbContext.PLCDatas
				.Where(p => p.MachineId == machineId && p.Timestamp >= startTime && p.Timestamp <= endTime)
				.OrderBy(p => p.Timestamp)
				.ToListAsync();
		}
		public async Task<List<PLCData>> GetPlcDataForLotReportAsync(int machineId, string lotNo)
		{
			using var scope = _scopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			return await dbContext.PLCDatas
				.Where(p => p.MachineId == machineId && p.LotNo == lotNo)
				.OrderBy(p => p.Timestamp)
				.ToListAsync();
		}

		public async Task<List<PLCData>> GetPlcDataForLotReportAsync(int machineId, string lotNo, TimeSpan timeSpan)
		{
			using var scope = _scopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var endTime = DateTime.Now;
			var startTime = endTime - timeSpan;
			return await dbContext.PLCDatas
				.Where(p => p.MachineId == machineId && p.LotNo == lotNo && p.Timestamp >= startTime && p.Timestamp <= endTime)
				.OrderBy(p => p.Timestamp)
				.ToListAsync();
		}


	}
}
