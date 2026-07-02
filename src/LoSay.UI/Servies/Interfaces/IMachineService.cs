using LoSay.Data.Entities;
using LoSay.Data.Model;

namespace LoSay.Servies.Interfaces
{
	public interface IMachineService
	{
		Task<List<MachineWithLotInfo>> GetMachinesWithLotInfoAsync();
		Task<List<Machine>> GetMachinesAsync();
		Task AddMachineAsync(Machine machine);
		Task UpdateMachineAsync(Machine machine);
		Task DeleteMachineAsync(int machineId);

	}
}
