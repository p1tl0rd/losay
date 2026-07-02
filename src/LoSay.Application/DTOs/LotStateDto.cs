using AutoMapper;
using LoSay.Application.Mapping;
using LoSay.Domain.Entities;

namespace LoSay.Application.DTOs
{
	public class LotStateDto : IMapFrom<LotState>
	{
		public int Id { get; set; }
		public int MachineId { get; set; }
		public string? LotNo { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public bool IsRunning { get; set; } = false; // Mặc định là false

		public void Mapping(Profile profile)
		{
			profile.CreateMap<LotState, LotStateDto>().ReverseMap();
		}
	}
}
