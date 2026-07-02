using LoSay.Application.DTOs;

namespace LoSay.Application.Services
{
	public interface ILotStateService
	{
		Task<LotStateDto> GetCurrentLotNoAsync(int machineId);
		Task<LotStateDto> StartLotAutoAsync(MachineDto machineDto);
		Task FinishLotAsync(int machineId);
		Task<(List<LotStateDto> Items, int TotalCount)> GetLotStatesPagedAsync(int page, int pageSize, string searchTerm = "", string sortColumn = "StartTime", bool sortAscending = false);
		Task UpdateLotState(LotStateDto lotStateDto);
	}
}
