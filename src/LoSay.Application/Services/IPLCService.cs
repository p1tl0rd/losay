using LoSay.Application.DTOs;

namespace LoSay.Application.Services
{
	public interface IPLCService
	{
		//Read Data From PLC
		Task<bool> ConnectPLCAsync();
		Task DisconnectPLCAsync();
		Task<int> ReadPLCRawValueAsync(int address);
		Task<int[]> ReadPLCListRawValueAsync(int[] addresses, int quantity);
		Task<PLCDataDto> ReadPLCConsecutiveRawValuesAsync(MachineDto machineDto, int quantity);

		//CRUD
		Task<int> CreatePLCData(PLCDataDto pLCDataDto);
		Task<IEnumerable<PLCDataDto>> GetPLCDataByMachineId(int id, TimeSpan timeSpan);
		Task<int> UpdateLotNo(LotStateDto lotStateDto,string oldLotNo, string newLotNo);

		//Read Data For create Chart
		Task<IEnumerable<PLCDataDto>> GetPlcDataForLotReportAsync(int machineId, string lotNo);
		Task<IEnumerable<PLCDataDto>> GetHistoricalPlcDataAsync(int machineId, TimeSpan timeSpan);
		Task<IEnumerable<PLCDataDto>> GetPlcDataForLotReportAsync(int machineId, string lotNo, TimeSpan timeSpan);

		// Lot management
		Task StartLotAsync(int machineId, string lotNo);
		Task<string> StartLotAutoAsync(int machineId);
		Task FinishLotAsync(int machineId);
		Task<string?> GetCurrentLotNoAsync(int machineId);
		Task<DateTime?> GetLotStartTimeAsync(int machineId);
		Task<string> GenerateNewLotNoAsync(int machineId);

		// High-level processing (used by BackgroundService fetcher)
		Task<ProcessedPlcData> GetProcessedPlcDataAsync(int machineId, string addressConfig);
	}

	public record ProcessedPlcData
	{
		public bool IsSuccess { get; init; }
		public string? LotNo { get; init; }
		public double[]? Values { get; init; }
	}
}
