using LoSay.Data.Contexts;
using LoSay.Data.Entities;
using LoSay.Data.Model;
using LoSay.Servies.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Servies.Implementations
{
	public class MachineService : IMachineService
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly ILogger<MachineService> _logger;

		public MachineService(ApplicationDbContext dbContext, ILogger<MachineService> logger)
		{
			_dbContext = dbContext;
			_logger = logger;
		}

		public async Task<List<Machine>> GetMachinesAsync()
		{
			return await _dbContext.Machines.ToListAsync();
		}

		public async Task<List<MachineWithLotInfo>> GetMachinesWithLotInfoAsync()
		{
			try
			{
				return await _dbContext.Machines.Where(m => m.Status == true).Select(m => new MachineWithLotInfo
				{
					Id = m.Id,
					MachineName = m.MachineName,
					Status = m.Status,
					LotRun = m.LotRun,
					StartTime = _dbContext.LotStates.FirstOrDefault(ls => ls.MachineId == m.Id && ls.IsRunning).StartTime
				}).ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting machines with lot info.");
				throw;
			}
		}

		public async Task AddMachineAsync(Machine machine)
		{
			_dbContext.Machines.Add(machine);
			await _dbContext.SaveChangesAsync();
		}

		public async Task UpdateMachineAsync(Machine machine)
		{
			var exist = await _dbContext.Machines.FindAsync(machine.Id);
			if (exist != null)
			{
				exist.MachineName = machine.MachineName;
				exist.Address = machine.Address;
				exist.LotRun = machine.LotRun;
				exist.FinishLot = machine.FinishLot;
				exist.Status = machine.Status;
				await _dbContext.SaveChangesAsync();
			}
		}

		public async Task DeleteMachineAsync(int machineId)
		{
			var exist = await _dbContext.Machines.FindAsync(machineId);
			if (exist != null)
			{
				_dbContext.Machines.Remove(exist);

				var lotRuning = await _dbContext.LotStates.Where(p=>p.IsRunning == true && p.MachineId == machineId).ToListAsync();
				_dbContext.LotStates.RemoveRange(lotRuning);

				await _dbContext.SaveChangesAsync();
			}
		}

	}
}
