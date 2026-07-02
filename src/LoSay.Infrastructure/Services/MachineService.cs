using AutoMapper;
using LoSay.Application.DTOs;
using LoSay.Application.Repositories;
using LoSay.Application.Services;
using LoSay.Domain.Entities;

namespace LoSay.Infrastructure.Services
{
	public class MachineService : IMachineService
	{
		private readonly IMachineRepository _machineRepository;
		private readonly ILotStateRepository _lotStateRepository;
		private readonly IMapper _mapper;
		public MachineService(IMachineRepository machineRepository, IMapper mapper, ILotStateRepository lotStateRepository)
		{
			_machineRepository = machineRepository;
			_mapper = mapper;
			_lotStateRepository = lotStateRepository;
		}
		public async Task<IEnumerable<MachineDto>> GetMachines()
		{
			var machines = await _machineRepository.GetMachines();

			var result = _mapper.Map<List<MachineDto>>(machines);
			return result;
		}
		public async Task<IEnumerable<MachineDto>> GetAllMachines()
		{
			var machines = await _machineRepository.GetAllMachines();

			var result = _mapper.Map<List<MachineDto>>(machines);
			return result;
		}
		public async Task<MachineDto> GetMachineById(int id)
		{
			var machineEntity = await _machineRepository.GetMachineById(id);

			var result = _mapper.Map<MachineDto>(machineEntity);
			return result;
		}

		public async Task<IEnumerable<MachineWithLotInfoDto>> GetMachinesWithLotInfoAsync(List<PLCDataDto> pLCDataDtos = null)
		{
			var machines = await _machineRepository.GetMachines();
			var lotRunning = await _lotStateRepository.GetLotRunningAsync();
			// Nếu null thì gán list rỗng để không lỗi
			pLCDataDtos ??= new List<PLCDataDto>();

			var result = (from m in machines
						  join l in lotRunning on m.Id equals l.MachineId into lotGroup
						  from l in lotGroup.DefaultIfEmpty()
						  join p in pLCDataDtos on m.Id equals p.MachineId
						  select new MachineWithLotInfoDto
						  {
							  Id = m.Id,
							  MachineName = m.MachineName,
							  Status = m.Status,
							  LotRun = l != null ? l.LotNo : null,
							  StartTime = l != null ? l.StartTime : null,
							  Elapsed = l != null ? DateTime.Now - l.StartTime : null,
							  PLCDataDto = p != null ? p : null,
						  });

			return result;

		}

		public async Task<List<MachineDto>> GetMachinesAsync()
		{
			var machines = await GetMachines();
			var result = _mapper.Map<List<MachineDto>>(machines);
			return result;
		}

		public async Task<int> CreateMachineAsync(MachineDto machine)
		{
			var machineMapped = _mapper.Map<Machine>(machine);
			var id = await _machineRepository.CreateAsync(machineMapped);
			await _machineRepository.SaveChangeAsync();
			return id;
		}

		public async Task UpdateMachineAsync(MachineDto machine)
		{
			var machineEntity = await _machineRepository.GetMachineById(machine.Id);
			var machineUpdate = _mapper.Map(source: machine, destination: machineEntity);
			await _machineRepository.UpdateMachine(machineUpdate);
			await _machineRepository.SaveChangeAsync();
		}

		public async Task DeleteMachineAsync(int machineId)
		{
			await _machineRepository.DeleteMachine(machineId);
			await _machineRepository.SaveChangeAsync();
		}


	}
}
