using AutoMapper;
using LoSay.Application.Mapping;
using LoSay.Domain.Entities;

namespace LoSay.Application.DTOs
{
	public class MachineWithLotInfoDto : MachineDto, IMapFrom<Machine>
	{
		public DateTime? StartTime { get; set; }
		public TimeSpan? Elapsed { get; set; }
		public PLCDataDto? PLCDataDto { get; set; }

		public void Mapping(Profile profile)
		{
			profile.CreateMap<Machine, MachineWithLotInfoDto>().ReverseMap();
		}
	}
}
