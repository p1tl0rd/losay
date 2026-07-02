using LoSay.Application.DTOs;

namespace LoSay.Application.Services
{
	public interface IMachineService
	{
		Task<IEnumerable<MachineDto>> GetMachines();
		Task<IEnumerable<MachineDto>> GetAllMachines();
		Task<MachineDto> GetMachineById(int id);
		Task<IEnumerable<MachineWithLotInfoDto>> GetMachinesWithLotInfoAsync(List<PLCDataDto> pLCDataDtos = null);
		Task<List<MachineDto>> GetMachinesAsync();
		Task<int> CreateMachineAsync(MachineDto machine);
		Task UpdateMachineAsync(MachineDto machine);
		Task DeleteMachineAsync(int machineId);
	}
}
