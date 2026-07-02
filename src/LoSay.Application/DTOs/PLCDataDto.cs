using AutoMapper;
using LoSay.Application.Mapping;
using LoSay.Domain.Entities;

namespace LoSay.Application.DTOs
{
	public class PLCDataDto : IMapFrom<PLCData>
	{
		public int MachineId { get; set; }
		public int ItemCodeId { get; set; }
		public string? LotNo { get; set; }
		public LotStateDto? LotStateDto { get; set; }
		public double? Value1 { get; set; }
		public double? Value2 { get; set; }
		public double? Value3 { get; set; }
		public double? Value4 { get; set; }
		public double? Value5 { get; set; }
		public DateTime? Timestamp { get; set; }
		public bool Status { get; set; }
		public string? Message { get; set; }

		public void Mapping(Profile profile)
		{
			profile.CreateMap<PLCData, PLCDataDto>().ReverseMap();
		}
	}
}
