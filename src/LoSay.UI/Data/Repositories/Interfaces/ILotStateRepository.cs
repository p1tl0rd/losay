using LoSay.Data.Entities;

namespace LoSay.Data.Repositories.Interfaces
{
	public interface ILotStateRepository
	{
		Task<LotState?> GetCurrentLotStateAsync(int machineId);
		Task<List<LotState>> GetAllLotStatesAsync(int machineId);  
		Task StartLotAsync(int machineId, string lotNo);
		Task FinishLotAsync(int machineId);
		Task<DateTime?> GetLotStartTimeAsync(int machineId);
		Task<(List<LotState> Items, int TotalCount)> GetPagedLotStatesAsync(int page, int pageSize, string searchTerm = "", string sortColumn = "StartTime", bool sortAscending = false);
		Task<List<LotState>> GetAllLotStatesAsync(); 
		Task<List<LotState>> GetLotDataStatesByIdAsync(int id); 
	}
}
