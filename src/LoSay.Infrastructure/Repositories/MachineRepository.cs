using LoSay.Application.Common.Interfaces;
using LoSay.Application.Repositories;
using LoSay.Data.Contexts;
using LoSay.Domain.Entities;
using LoSay.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Infrastructure.Repositories
{
	public class MachineRepository : RepositoryBase<Machine, int>, IMachineRepository
	{
		public MachineRepository(ApplicationDbContext context, IUnitOfWork<ApplicationDbContext> unitOfWork) : base(context, unitOfWork)
		{
		}

		public Task<int> CreateMachine(Machine machine) => CreateAsync(machine);

		public async Task DeleteMachine(int id)
		{
			var entity = FindByCondition(p => p.Id.Equals(id)).SingleOrDefault();
			await DeleteAsync(entity);
		}

		public async Task<IEnumerable<Machine>> GetAllMachines() => FindAll();

		public async Task<Machine> GetMachineById(int id) => await FindByCondition(p => p.Id.Equals(id)).FirstOrDefaultAsync();

		public async Task<IEnumerable<Machine>> GetMachines() => FindByCondition(p => p.Status == true);

		public Task UpdateMachine(Machine machine) => UpdateAsync(machine);
	}
}
