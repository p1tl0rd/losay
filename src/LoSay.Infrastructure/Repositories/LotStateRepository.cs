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
		public async Task UpdateLotAsync(LotState lotState) => await UpdateAsync(lotState);

		public async Task FinishLotAsync(int machineId)
		{
			var running = await FindByCondition(p => p.MachineId == machineId && p.IsRunning).ToListAsync();
			var now = DateTime.Now;
			foreach (var lot in running)
			{
				lot.IsRunning = false;
				lot.EndTime = now;
				await UpdateAsync(lot);
			}
			await SaveChangeAsync();
		}

		public async Task<IEnumerable<LotState>> GetAllLotStatesAsync(int machineId)
		{
			return await FindByCondition(p => p.MachineId == machineId).ToListAsync();
		}

		public async Task<(IEnumerable<LotState> Items, int TotalCount)> GetAllLotStatesPagedAsync(int page, int pageSize, string searchTerm = "", string sortColumn = "StartTime", bool sortAscending = false)
		{
			var lotStates = FindAll();

			var machines = await _machineRepository.GetMachines();


			var query = from ls in lotStates
						join m in machines
						on ls.MachineId equals m.Id
						select new { ls, m.MachineName };

			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				query = query.Where(x => x.ls.LotNo != null && (x.ls.LotNo.Contains(searchTerm) || x.MachineName != null && x.MachineName.Contains(searchTerm)));
			}

			query = sortColumn switch
			{
				"LotNo" => sortAscending ? query.OrderBy(x => x.ls.LotNo) : query.OrderByDescending(x => x.ls.LotNo),
				"MachineName" => sortAscending ? query.OrderBy(x => x.MachineName) : query.OrderByDescending(x => x.MachineName),
				"StartTime" => sortAscending ? query.OrderBy(x => x.ls.StartTime) : query.OrderByDescending(x => x.ls.StartTime),
				"EndTime" => sortAscending ? query.OrderBy(x => x.ls.EndTime) : query.OrderByDescending(x => x.ls.EndTime),
				_ => sortAscending ? query.OrderBy(x => x.ls.StartTime) : query.OrderByDescending(x => x.ls.StartTime)
			};

			var totalCount = await query.CountAsync();
			var items = await query.Skip(page * pageSize)
								   .Take(pageSize)
								   .Select(x => x.ls)
								   .ToListAsync();

			return (items, totalCount);
		}

		public Task<LotState?> GetCurrentLotStateAsync(int machineId) => FindByCondition(p => p.MachineId.Equals(machineId) && p.IsRunning).FirstOrDefaultAsync();

		public async Task<IEnumerable<LotState>> GetLotDataStatesByIdAsync(int id)
		{
			return await FindByCondition(p => p.Id == id).ToListAsync();
		}

		public async Task<DateTime?> GetLotStartTimeAsync(int machineId)
		{
			var lot = await FindByCondition(p => p.MachineId == machineId && p.IsRunning)
				.OrderByDescending(p => p.StartTime)
				.FirstOrDefaultAsync();
			return lot?.StartTime;
		}

		public Task<(IEnumerable<LotState> Items, int TotalCount)> GetPagedLotStatesAsync(int page, int pageSize, string searchTerm = "", string sortColumn = "StartTime", bool sortAscending = false)
		{
			throw new NotImplementedException();
		}

		public async Task StartLotAsync(int machineId, string lotNo)
		{
			var existing = await FindByCondition(p => p.MachineId == machineId && p.IsRunning).ToListAsync();
			foreach (var lot in existing)
			{
				lot.IsRunning = false;
				lot.EndTime = DateTime.Now;
				await UpdateAsync(lot);
			}
			var newLot = new LotState
			{
				MachineId = machineId,
				LotNo = lotNo,
				StartTime = DateTime.Now,
				EndTime = null,
				IsRunning = true
			};
			await CreateAsync(newLot);
			await SaveChangeAsync();
		}

		public async Task<string> StartLotAutoAsync(int machineId)
		{
			var machine = await _machineRepository.GetMachineById(machineId);
			var machineName = machine?.MachineName ?? $"M{machineId}";
			var datePart = DateTime.Now.ToString("ddMMyy");
			var countToday = await CountLotInToday();
			var nextNumber = countToday + 1;
			var newLotNo = $"{machineName}_LotNo_{datePart}_{nextNumber}";

			await StartLotAsync(machineId, newLotNo);
			return newLotNo;
		}

		public async Task<IEnumerable<LotState>> GetLotRunningAsync() => await FindByCondition(p => p.IsRunning).ToListAsync();


	}
}
