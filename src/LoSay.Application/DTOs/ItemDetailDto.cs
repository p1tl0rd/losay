using AutoMapper;
using LoSay.Application.Mapping;
using LoSay.Domain.Entities;

namespace LoSay.Application.DTOs
{
	public class ItemDetailDto : IMapFrom<ItemDetail>
	{
		public int? No { get; set; }
		public int Id { get; set; }
		public int? ItemId { get; set; }
		public string? ClassName { get; set; }
		public double Temperature { get; set; }
		public double TimeDrying { get; set; }
		public bool Status { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<ItemDetail, ItemDetailDto>().ReverseMap();
		}
	}
}
