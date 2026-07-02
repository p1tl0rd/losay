using LoSay.Application.Common.Interfaces;
using LoSay.Domain.Entities;

namespace LoSay.Application.Repositories
{
	public interface ILotStateRepository : IRepositoryBase<LotState, int>
	{
		//Get
		Task<LotState?> GetCurrentLotStateAsync(int machineId);
		Task<int> CountLotInToday();
		Task<IEnumerable<LotState>> GetAllLotStatesAsync(int machineId);
		Task<IEnumerable<LotState>> GetLotRunningAsync();

		//Create
		Task<int> CreateLotAsync(LotState lotState);

		//Update
		Task UpdateLotAsync(LotState lotState);


		//
		Task<string> StartLotAutoAsync(int machineId);
		Task StartLotAsync(int machineId, string lotNo);
		Task FinishLotAsync(int machineId);
		Task<DateTime?> GetLotStartTimeAsync(int machineId);
		Task<(IEnumerable<LotState> Items, int TotalCount)> GetAllLotStatesPagedAsync(int page, int pageSize, string searchTerm = "", string sortColumn = "StartTime", bool sortAscending = false);
		Task<IEnumerable<LotState>> GetLotDataStatesByIdAsync(int id);
	}
}
