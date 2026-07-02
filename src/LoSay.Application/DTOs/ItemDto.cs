using AutoMapper;
using LoSay.Application.Mapping;
using LoSay.Domain.Entities;

namespace LoSay.Application.DTOs
{
	public class ItemDto : IMapFrom<Item>
	{
		public int Id { get; set; }
		public int No { get; set; }
		public string ItemCode { get; set; }
		public string ItemName { get; set; }
		public double Max { get; set; }
		public double Min { get; set; }
		public int TimeSamping { get; set; }
		public bool Status { get; set; }

		public void Mapping(Profile profile)
		{
			profile.CreateMap<Item, ItemDto>().ReverseMap();
		}
	}
}
