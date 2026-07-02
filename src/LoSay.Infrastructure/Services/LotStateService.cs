using AutoMapper;
using LoSay.Application.DTOs;
using LoSay.Application.Repositories;
using LoSay.Application.Services;
using LoSay.Domain.Entities;

namespace LoSay.Infrastructure.Services
{
	public class LotStateService : ILotStateService
	{
		private readonly ILotStateRepository _lotStateRepository;
		private readonly IPLCService _pLCService;
		private readonly IMapper _mapper;
		public LotStateService(ILotStateRepository lotStateRepository, IMapper mapper, IPLCService pLCService)
		{
			_lotStateRepository = lotStateRepository;
			_mapper = mapper;
			_pLCService = pLCService;
		}

		public async Task<LotStateDto> GetCurrentLotNoAsync(int machineId)
		{
			var lotstate = await _lotStateRepository.GetCurrentLotStateAsync(machineId);
			var result = _mapper.Map<LotStateDto>(lotstate);

			return result;
		}

		public async Task<LotStateDto> StartLotAutoAsync(MachineDto machineDto)
		{
			// Kiểm tra vào tạo Lot state mới
			var lotStateDto = await GenerateNewLotNoAsync(machineDto);

			var lotState = _mapper.Map<LotState>(lotStateDto);
			await _lotStateRepository.CreateLotAsync(lotState);
			await _lotStateRepository.SaveChangeAsync();
			var result = _mapper.Map<LotStateDto>(lotState);

			return result;
		}
		public async Task FinishLotAsync(int machineId)
		{
			var lotState = await _lotStateRepository.GetCurrentLotStateAsync(machineId);
			if (lotState != null)
			{
				lotState.EndTime = DateTime.Now;
				lotState.IsRunning = false;

				await _lotStateRepository.UpdateAsync(lotState);
				await _lotStateRepository.SaveChangeAsync();
			}
		}

		public async Task<LotStateDto> GenerateNewLotNoAsync(MachineDto machineDto)
		{
			string datePart = DateTime.Now.ToString("ddMMyy");

			// Đếm số lot đã có trong ngày hôm nay
			var countToday = await _lotStateRepository.CountLotInToday();

			// Lấy sô thứ tự tiếp theo.
			int nextNumber = countToday + 1;

			// Tạo tên Lot mới.
			string newLotNo = $"{machineDto.MachineName}_LotNo_{datePart}_{nextNumber}";

			// Tạo Dto
			var result = new LotStateDto
			{
				MachineId = machineDto.Id,
				LotNo = newLotNo,
				StartTime = DateTime.Now,
				IsRunning = true
			};

			return result;
		}

		public async Task<(List<LotStateDto> Items, int TotalCount)> GetLotStatesPagedAsync(int page, int pageSize, string searchTerm = "", string sortColumn = "StartTime", bool sortAscending = false)
		{
			var (items, totalCount) = await _lotStateRepository.GetAllLotStatesPagedAsync(page, pageSize, searchTerm, sortColumn, sortAscending);

			var result = _mapper.Map<List<LotStateDto>>(items);
			return (result, totalCount);
		}

		public async Task UpdateLotState(LotStateDto lotStateDto)
		{
			var lotState = await _lotStateRepository.GetByIdAsync(lotStateDto.Id);
			string oldLotNo = lotState.LotNo;
			lotState.LotNo = lotStateDto.LotNo;

			await _lotStateRepository.UpdateAsync(lotState);
			await _lotStateRepository.SaveChangeAsync();

			await _pLCService.UpdateLotNo(lotStateDto, oldLotNo, lotStateDto.LotNo);

			var result = _mapper.Map<LotStateDto>(lotState);
		}
	}
}
