using LoSay.Data.Entities;
using LoSay.Data.Models;

namespace LoSay.Servies.Interfaces
{
	public interface IPLCService
	{
		Task<bool> ConnectPLCAsync();
		Task DisconnectPLCAsync();
		Task<int> ReadPLCRawValueAsync(int address);
		Task<int[]> ReadPLCListRawValueAsync(int[] addresses, int quantity);
		Task<int[]> ReadPLCConsecutiveRawValuesAsync(int startAddress, int quantity);
		Task<ProcessedPlcData> GetProcessedPlcDataAsync(int machineId, string addressConfig);
		PLCReader GetPLCReader(); // Để BackgroundService có thể truy cập PLCReader

		Task StartLotAsync(int machineId, string lotNo);
		Task<string> StartLotAutoAsync(int machineId);
		Task FinishLotAsync(int machineId);
		Task<string?> GetCurrentLotNoAsync(int machineId);
		Task<DateTime?> GetLotStartTimeAsync(int machineId);

		Task<List<PLCData>> GetHistoricalPlcDataAsync(int machineId, TimeSpan timeSpan);
		Task<List<PLCData>> GetPlcDataForLotReportAsync(int machineId, string lotNo);

		//Item

 
	}
}
