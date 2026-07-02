using System;
using LoSay.Data.Contexts;
using LoSay.Data.Entities;
using LoSay.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Data.Repositories.Implementations
{
	public class EfLotStateRepository : ILotStateRepository
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly ILogger<EfLotStateRepository> _logger;

		public EfLotStateRepository(ApplicationDbContext dbContext, ILogger<EfLotStateRepository> logger)
		{
			_dbContext = dbContext;
			_logger = logger;
		}

		public async Task<LotState?> GetCurrentLotStateAsync(int machineId)
		{
			return await _dbContext.LotStates
				//	.AsNoTracking() // Tang hi?u su?t n?u không c?n theo dõi entity
				.FirstOrDefaultAsync(ls => ls.MachineId == machineId && ls.IsRunning);
		}

		public async Task<List<LotState>> GetAllLotStatesAsync(int machineId)
		{
			return await _dbContext.LotStates
				.AsNoTracking()
				.Where(ls => ls.MachineId == machineId)
				.ToListAsync();
		}

		public async Task StartLotAsync(int machineId, string lotNo)
		{
			// K?t thúc lô dang ch?y (n?u có)
			var existingLotState = await GetCurrentLotStateAsync(machineId);
			if (existingLotState != null)
			{
				existingLotState.EndTime = DateTime.Now;
				existingLotState.IsRunning = false;
			}

			// B?t d?u lô m?i
			var newLotState = new LotState
			{
				MachineId = machineId,
				LotNo = lotNo,
				StartTime = DateTime.Now,
				IsRunning = true
			};

			var a = EntityState.Detached;
			_dbContext.LotStates.Add(newLotState);
			await _dbContext.SaveChangesAsync();
		}

		public async Task FinishLotAsync(int machineId)
		{
			var lotState = await GetCurrentLotStateAsync(machineId);
			if (lotState != null)
			{
				lotState.EndTime = DateTime.Now;
				lotState.IsRunning = false;
				// Detach entity tru?c khi luu thay d?i
				//	_dbContext.Entry(lotState).State = EntityState.Detached;
				//	_dbContext.LotStates.Update(lotState);
				_dbContext.Entry(lotState).State = EntityState.Modified;
				await _dbContext.SaveChangesAsync();
			}
		}

		public async Task<DateTime?> GetLotStartTimeAsync(int machineId)
		{
			try
			{
				var lotState = await _dbContext.LotStates
					.AsNoTracking()
					.FirstOrDefaultAsync(ls => ls.MachineId == machineId && ls.IsRunning);
				return lotState?.StartTime;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error getting lot start time for machine {machineId}: {ex.Message}");
				throw;
			}
		}

		public async Task<(List<LotState> Items, int TotalCount)> GetPagedLotStatesAsync(int page, int pageSize, string searchTerm = "", string sortColumn = "StartTime", bool sortAscending = false)
		{
			var query = _dbContext.LotStates.AsQueryable();

			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				query = query.Where(ls => ls.LotNo.Contains(searchTerm) ||
										   _dbContext.Machines.Any(m => m.Id == ls.MachineId && m.MachineName.Contains(searchTerm))); // Search in LotNo and MachineName
			}

			// Sorting
			query = sortColumn switch
			{
				"LotNo" => sortAscending ? query.OrderBy(ls => ls.LotNo) : query.OrderByDescending(ls => ls.LotNo),
				"MachineName" => sortAscending ? query.OrderBy(ls => _dbContext.Machines.FirstOrDefault(m => m.Id == ls.MachineId).MachineName) : query.OrderByDescending(ls => _dbContext.Machines.FirstOrDefault(m => m.Id == ls.MachineId).MachineName),
				"StartTime" => sortAscending ? query.OrderBy(ls => ls.StartTime) : query.OrderByDescending(ls => ls.StartTime),
				"EndTime" => sortAscending ? query.OrderBy(ls => ls.EndTime) : query.OrderByDescending(ls => ls.EndTime),
				_ => query.OrderByDescending(ls => ls.StartTime) // Default sorting
			};

			var totalCount = await query.CountAsync();

			var items = await query
				.Skip(page * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, totalCount);
		}

		public async Task<List<LotState>> GetAllLotStatesAsync()
		{
			return await _dbContext.LotStates.OrderByDescending(ls => ls.StartTime).ToListAsync();
		}

		public async Task<List<LotState>> GetLotDataStatesByIdAsync(int id)
		{
			return await _dbContext.LotStates.Where(lot => lot.Id == id).ToListAsync();
		}


	}
}
