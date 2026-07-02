using AutoMapper;
using LoSay.Application.DTOs;
using LoSay.Application.Repositories;
using LoSay.Application.Services;
using LoSay.Domain.Entities;
using Serilog;

namespace LoSay.Infrastructure.Services
{
	public class ItemService : IItemService
	{
		private readonly ILogger _logger;
		private readonly IMapper _mapper;
		private readonly IItemRepository _itemRepository;
		private readonly IItemDetailRepository _itemDetailRepository;
		public ItemService(IItemDetailRepository itemDetailRepository, IItemRepository itemRepository, ILogger logger, IMapper mapper)
		{
			_itemDetailRepository = itemDetailRepository ?? throw new ArgumentNullException(nameof(itemDetailRepository));
			_itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
			_logger = logger ?? throw new ArgumentNullException(nameof(itemRepository));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		public async Task<int> CreateItemAsync(ItemDto item)
		{
			var itemEntity = _mapper.Map<Item>(item);
			await _itemRepository.CreateAsync(itemEntity);
			await _itemRepository.SaveChangeAsync();
			return itemEntity.Id;
		}

		public async Task<List<ItemDto>> GetItemsAsync()
		{
			_logger.Information("BEGIN GetItemsAsync");
			var items = await _itemRepository.GetItemsAsync();
			var result = _mapper.Map<List<ItemDto>>(items).Select((x, i) =>
			{
				x.No = i + 1;
				return x;
			}).ToList(); ;

			return result;
		}

	
		public async Task UpdateItemAsync(ItemDto item)
		{
			var itemEntity = await _itemRepository.GetByIdAsync(item.Id);
			if (itemEntity == null) return;

			var updateItem = _mapper.Map(source: item, destination: itemEntity);
			await _itemRepository.UpdateItemAsync(updateItem);
			await _itemRepository.SaveChangeAsync();
		}

		public Task DeleteItemAsync(int id)
		{
			throw new NotImplementedException();
		}

		public async Task CreateItemDetailAsync(ItemDetailDto itemDetailDto)
		{
			var itemEntity = _mapper.Map<ItemDetail>(itemDetailDto);
			await _itemDetailRepository.CreateItemDetailAsync(itemEntity);
			await _itemDetailRepository.SaveChangeAsync();
		}
		public async Task<List<ItemDetailDto>> GetItemDetailsByItemIdAsync(int itemId)
		{
			var itemDetails = await _itemDetailRepository.GetItemDetailsByItemIdAsync(itemId);
			var itemDetailDto = _mapper.Map<List<ItemDetailDto>>(itemDetails).Select((x, i) =>
			{
				x.No = i + 1;
				return x;
			}).ToList();

			return itemDetailDto;
		}
		public async Task UpdateItemDetailAsync(ItemDetailDto itemDetail)
		{
			var itemEntity = await _itemDetailRepository.GetItemDetailByItemIdAsync(itemDetail.Id);
			if (itemEntity == null) return;

			var updateItem = _mapper.Map(source: itemDetail, destination: itemEntity);
			await _itemDetailRepository.UpdateItemDetailAsync(updateItem);
			await _itemDetailRepository.SaveChangeAsync();
			
		}
		public Task DeleteItemDetailAsync(int id)
		{
			throw new NotImplementedException();
		}
	}
}
