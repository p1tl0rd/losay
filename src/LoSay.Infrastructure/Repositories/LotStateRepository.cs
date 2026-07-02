using LoSay.Application.Common.Interfaces;
using LoSay.Application.Repositories;
using LoSay.Data.Contexts;
using LoSay.Domain.Entities;
using LoSay.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Infrastructure.Repositories
{
	public class LotStateRepository : RepositoryBase<LotState, int>, ILotStateRepository
	{
		private readonly IMachineRepository _machineRepository;
		public LotStateRepository(ApplicationDbContext context, IUnitOfWork<ApplicationDbContext> unitOfWork, IMachineRepository machineRepository) : base(context, unitOfWork)
		{
			_machineRepository = machineRepository;
		}

		public async Task<int> CountLotInToday()
		{
			var lotsInToday = FindByCondition(p => p.StartTime.HasValue && p.StartTime.Value.Date == DateTime.Today);
			return lotsInToday.Count();
		}

		public async Task<int> CreateLotAsync(LotState lotState)
		{
			return await CreateAsync(lotState);
		}
		public async Task UpdateLotAsync(LotState lotState) => UpdateAsync(lotState);

		public Task FinishLotAsync(int machineId)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<LotState>> GetAllLotStatesAsync(int machineId)
		{
			throw new NotImplementedException();
		}

		public async Task<(IEnumerable<LotState> Items, int TotalCount)> GetAllLotStatesPagedAsync(int page, int pageSize, string searchTerm = "", string sortColumn = "StartTime", bool sortAscending = false)
		{
			var lotStates = FindAll(); // Kiểu IQueryable

			var machines = await _machineRepository.GetMachines(); 

			
			var query = from ls in lotStates
						join m in machines
						on ls.MachineId equals m.Id
						select new { ls, m.MachineName };

			// 4. Filter
			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				query = query.Where(x => x.ls.LotNo.Contains(searchTerm) || x.MachineName.Contains(searchTerm));
			}

			// 5. Sorting
			query = sortColumn switch
			{
				"LotNo" => sortAscending ? query.OrderBy(x => x.ls.LotNo) : query.OrderByDescending(x => x.ls.LotNo),
				"MachineName" => sortAscending ? query.OrderBy(x => x.MachineName) : query.OrderByDescending(x => x.MachineName),
				"StartTime" => sortAscending ? query.OrderBy(x => x.ls.StartTime) : query.OrderByDescending(x => x.ls.StartTime),
				"EndTime" => sortAscending ? query.OrderBy(x => x.ls.EndTime) : query.OrderByDescending(x => x.ls.EndTime),
				_ => query.OrderByDescending(x => x.ls.StartTime)
			};

			// 6. Paging + lấy tổng số
			var totalCount = await query.CountAsync();
			var items = await query.Skip(page * pageSize)
								   .Take(pageSize)
								   .Select(x => x.ls) // chỉ trả LotState
								   .ToListAsync();

			return (items, totalCount);
		}

		public Task<LotState?> GetCurrentLotStateAsync(int machineId) => FindByCondition(p => p.MachineId.Equals(machineId) && p.IsRunning).FirstOrDefaultAsync();

		public Task<IEnumerable<LotState>> GetLotDataStatesByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<DateTime?> GetLotStartTimeAsync(int machineId)
		{
			throw new NotImplementedException();
		}

		public Task<(IEnumerable<LotState> Items, int TotalCount)> GetPagedLotStatesAsync(int page, int pageSize, string searchTerm = "", string sortColumn = "StartTime", bool sortAscending = false)
		{
			throw new NotImplementedException();
		}

		public Task StartLotAsync(int machineId, string lotNo)
		{
			throw new NotImplementedException();
		}

		public Task<string> StartLotAutoAsync(int machineId)
		{
			throw new NotImplementedException();
		}

		public async Task<IEnumerable<LotState>> GetLotRunningAsync() => FindByCondition(p => p.IsRunning);


	}
}
