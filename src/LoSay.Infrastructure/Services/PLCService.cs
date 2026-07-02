using AutoMapper;
using LoSay.Application.DTOs;
using LoSay.Application.Repositories;
using LoSay.Application.Services;
using LoSay.Domain.Entities;
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
		private readonly ILogger _logger;
		private readonly IMapper _mapper;

		public PLCService(IConfiguration configuration, PLCReader plcReader, ILogger logger, IServiceScopeFactory scopeFactory
			, IPLCDataRepository pLCDataRepository, IMapper mapper)
		{
			_logger = logger;
			_scopeFactory = scopeFactory;
			_plcReader = plcReader;
			_pLCDataRepository = pLCDataRepository;
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
			//	DisposeAsync().GetAwaiter().GetResult(); // Gọi DisposeAsync() đồng bộ
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
			int[] addressList = machineDto.Address.Split(',').Select(int.Parse).ToArray();

			var dataPLC = await _plcReader.ReadConsecutiveRegistersAsync(addressList.First(), quantity);
			double[] convertedValue = dataPLC.Select(v => (double)v / 10).ToArray();

			_logger.Information($"Data from Machine {machineDto.MachineName} " +
				$"[{string.Join(", ", convertedValue[0])}]" +
				$"[{string.Join(", ", convertedValue[1])}]" +
				$"[{string.Join(", ", convertedValue[2])}]" +
				$"[{string.Join(", ", convertedValue[3])}]");

			var result = new PLCDataDto
			{
				MachineId = machineDto.Id,
				Value1 = convertedValue[0],
				Value2 = convertedValue[1],
				Value3 = convertedValue[2],
				Value4 = convertedValue[3],
				Value5 = convertedValue[4],
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

		// Read Data to Create Chart
		public async Task<IEnumerable<PLCDataDto>> GetPlcDataForLotReportAsync(int machineId, string lotNo)
		{
			var plcDataEntity = await _pLCDataRepository.GetPLCDataByLotNoAndMachineIdAsync(machineId, lotNo);

			var result = _mapper.Map<List<PLCDataDto>>(plcDataEntity);

			return result;
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
	}
}
