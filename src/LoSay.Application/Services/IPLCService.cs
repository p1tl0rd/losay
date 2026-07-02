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

	}
}
