using AutoMapper;
using LoSay.Application.DTOs;
using LoSay.Application.Repositories;
using LoSay.Application.Services;
using LoSay.Data.Contexts;
using LoSay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LoSay.Infrastructure.Services
{
	public class PLCService : IPLCService, IAsyncDisposable, IDisposable
	{
		private readonly PLCReader _plcReader;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IPLCDataRepository _pLCDataRepository;
		private readonly ILotStateRepository _lotStateRepository;
		private readonly ILogger _logger;
		private readonly IMapper _mapper;
		private readonly Dictionary<int, string?> _currentLotNos = new();
		private readonly Dictionary<int, bool> _isLotRunning = new();

		public PLCService(IConfiguration configuration, PLCReader plcReader, ILogger logger, IServiceScopeFactory scopeFactory
			, IPLCDataRepository pLCDataRepository, ILotStateRepository lotStateRepository, IMapper mapper)
		{
			_logger = logger;
			_scopeFactory = scopeFactory;
			_plcReader = plcReader;
			_pLCDataRepository = pLCDataRepository;
			_lotStateRepository = lotStateRepository;
			_mapper = mapper;
		}
		public async Task<bool> ConnectPLCAsync()
		{
			var isConntected = await _plcReader.Connect();
			if (isConntected)
			{
				_logger.Information($" Ket noi PLC thanh cong.");
			}
			else
			{
				_logger.Error($" Loi ket noi PLC");
			}
			return isConntected;
		}

		public async Task<int> CreatePLCData(PLCDataDto pLCDataDto)
		{
			var plcDataEntity = _mapper.Map<PLCData>(pLCDataDto);
			await _pLCDataRepository.CreatePLCDataAsync(plcDataEntity);
			await _pLCDataRepository.SaveChangeAsync();
			return plcDataEntity.Id;
		}

		public async Task DisconnectPLCAsync()
		{
			await _plcReader.Disconnect();
		}

		public void Dispose()
		{
			//	DisposeAsync().GetAwaiter().GetResult();
		}

		public async ValueTask DisposeAsync()
		{
			await DisconnectPLCAsync();
		}

		public async Task<IEnumerable<PLCDataDto>> GetPLCDataByMachineId(int id, TimeSpan timeSpan)
		{
			var PLCDataDtos = await _pLCDataRepository.GetPLCDataByMachineIdAync(id, timeSpan);
			var result = _mapper.Map<List<PLCDataDto>>(PLCDataDtos);

			return result;
		}

		public async Task<PLCDataDto> ReadPLCConsecutiveRawValuesAsync(MachineDto machineDto, int quantity)
		{
			int[] addressList = machineDto.Address!.Split(',').Select(int.Parse).ToArray();

			var dataPLC = await _plcReader.ReadConsecutiveRegistersAsync(addressList.First(), quantity);
			double[] convertedValue = dataPLC.Select(v => (double)v / 10).ToArray();

			if (convertedValue.Length < 5)
			{
				_logger.Warning($"PLC returned only {convertedValue.Length} registers for machine {machineDto.MachineName}, expected 5");
			}

			_logger.Information($"Data from Machine {machineDto.MachineName} " +
				$"[{string.Join(", ", convertedValue.Take(5))}]");

			var result = new PLCDataDto
			{
				MachineId = machineDto.Id,
				Value1 = convertedValue.Length > 0 ? convertedValue[0] : 0,
				Value2 = convertedValue.Length > 1 ? convertedValue[1] : 0,
				Value3 = convertedValue.Length > 2 ? convertedValue[2] : 0,
				Value4 = convertedValue.Length > 3 ? convertedValue[3] : 0,
				Value5 = convertedValue.Length > 4 ? convertedValue[4] : 0,
				Timestamp = DateTime.Now,
			};


			return result;
		}

		public async Task<int[]> ReadPLCListRawValueAsync(int[] addresses, int quantity)
		{
			return await _plcReader.ReadListRegisterAsync(addresses, quantity);
		}

		public async Task<int> ReadPLCRawValueAsync(int address)
		{
			return await _plcReader.ReadRegisterAsync(address);
		}

		public async Task<IEnumerable<PLCDataDto>> GetPlcDataForLotReportAsync(int machineId, string lotNo)
		{
			var plcDataEntity = await _pLCDataRepository.GetPLCDataByLotNoAndMachineIdAsync(machineId, lotNo);

			var result = _mapper.Map<List<PLCDataDto>>(plcDataEntity);

			return result;
		}

		public async Task<IEnumerable<PLCDataDto>> GetHistoricalPlcDataAsync(int machineId, TimeSpan timeSpan)
		{
			using var scope = _scopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var endTime = DateTime.Now;
			var startTime = endTime - timeSpan;
			var data = await dbContext.PLCDatas
				.Where(p => p.MachineId == machineId && p.Timestamp >= startTime && p.Timestamp <= endTime)
				.OrderBy(p => p.Timestamp)
				.ToListAsync();
			return _mapper.Map<List<PLCDataDto>>(data);
		}

		public async Task<IEnumerable<PLCDataDto>> GetPlcDataForLotReportAsync(int machineId, string lotNo, TimeSpan timeSpan)
		{
			using var scope = _scopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var endTime = DateTime.Now;
			var startTime = endTime - timeSpan;
			var data = await dbContext.PLCDatas
				.Where(p => p.MachineId == machineId && p.LotNo == lotNo && p.Timestamp >= startTime && p.Timestamp <= endTime)
				.OrderBy(p => p.Timestamp)
				.ToListAsync();
			return _mapper.Map<List<PLCDataDto>>(data);
		}

		public async Task<int> UpdateLotNo(LotStateDto lotStateDto, string oldLotNo, string newLotNo)
		{
			var plcData = await _pLCDataRepository.GetPLCDataByLotNoAndMachineIdAsync(lotStateDto.MachineId, oldLotNo);
			if (plcData != null)
			{
				foreach (var item in plcData)
				{
					item.LotNo = newLotNo;
				}
				await _pLCDataRepository.UpdateLotNoAsync(plcData.ToList());
				var result = await _pLCDataRepository.SaveChangeAsync();
				return result;
			}
			else
			{
				return 0;
			}
		}

		// Lot management
		public async Task StartLotAsync(int machineId, string lotNo)
		{
			await _lotStateRepository.StartLotAsync(machineId, lotNo);
			_currentLotNos[machineId] = lotNo;
			_isLotRunning[machineId] = true;
		}

		public async Task<string> StartLotAutoAsync(int machineId)
		{
			var lotNo = await _lotStateRepository.StartLotAutoAsync(machineId);
			_currentLotNos[machineId] = lotNo;
			_isLotRunning[machineId] = true;
			return lotNo;
		}

		public async Task<string> GenerateNewLotNoAsync(int machineId)
		{
			// Implementation: generate a lot number without persisting
			using var scope = _scopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var datePart = DateTime.Now.ToString("ddMMyy");
			var countToday = await dbContext.LotStates
				.CountAsync(l => l.StartTime.HasValue && l.StartTime.Value.Date == DateTime.Today);
			var machine = await dbContext.Machines.Where(m => m.Id == machineId).FirstOrDefaultAsync();
			var machineName = machine?.MachineName ?? $"M{machineId}";
			return $"{machineName}_LotNo_{datePart}_{countToday + 1}";
		}

		public async Task FinishLotAsync(int machineId)
		{
			await _lotStateRepository.FinishLotAsync(machineId);
			_currentLotNos[machineId] = null;
			_isLotRunning[machineId] = false;
		}

		public async Task<string?> GetCurrentLotNoAsync(int machineId)
		{
			if (_currentLotNos.TryGetValue(machineId, out var cached) && _isLotRunning.TryGetValue(machineId, out var running) && running)
			{
				return cached;
			}
			var lotState = await _lotStateRepository.GetCurrentLotStateAsync(machineId);
			return lotState?.LotNo;
		}

		public async Task<DateTime?> GetLotStartTimeAsync(int machineId)
		{
			return await _lotStateRepository.GetLotStartTimeAsync(machineId);
		}

		public async Task<ProcessedPlcData> GetProcessedPlcDataAsync(int machineId, string addressConfig)
		{
			try
			{
				int[] addressList = addressConfig.Split(',').Select(int.Parse).ToArray();
				int startAddress = addressList.First();

				int[] values = await _plcReader.ReadConsecutiveRegistersAsync(startAddress, 5);

				if (values == null || values.Length < 5)
				{
					_logger.Warning($"PLC returned invalid data for machine {machineId}");
					return new ProcessedPlcData { IsSuccess = false };
				}

				double[] convertedValues = values.Select(v => (double)v / 10.0).ToArray();

				string? currentLotNo = await GetCurrentLotNoAsync(machineId);
				bool isLotRunning = !string.IsNullOrEmpty(currentLotNo);
				string? finalLotNo = isLotRunning ? currentLotNo : null;

				// value[3] == 13740 means machine stopped, < 13740 means running
				if (values[3] < 13740 && finalLotNo == null)
				{
					finalLotNo = await StartLotAutoAsync(machineId);
				}
				else if (values[3] == 13740 && finalLotNo != null)
				{
					await FinishLotAsync(machineId);
				}

				return new ProcessedPlcData
				{
					IsSuccess = true,
					LotNo = finalLotNo,
					Values = convertedValues
				};
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"Error processing PLC data for machine {machineId}");
				return new ProcessedPlcData { IsSuccess = false };
			}
		}
	}
}
