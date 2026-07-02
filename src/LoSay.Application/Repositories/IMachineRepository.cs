using LoSay.Application.Common.Interfaces;
using LoSay.Domain.Entities;

namespace LoSay.Application.Repositories
{
	public interface IMachineRepository : IRepositoryBase<Machine, int>
	{
		Task<IEnumerable<Machine>> GetMachines();
		Task<IEnumerable<Machine>> GetAllMachines();
		Task<Machine> GetMachineById(int id);
		Task<int> CreateMachine(Machine machine);
		Task UpdateMachine(Machine machine);
		Task DeleteMachine(int id);
	}
}
