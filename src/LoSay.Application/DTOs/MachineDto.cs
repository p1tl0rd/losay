using AutoMapper;
using LoSay.Application.Mapping;
using LoSay.Domain.Entities;


namespace LoSay.Application.DTOs
{
	public class MachineDto : IMapFrom<Machine>
	{
		public int Id { get; set; }
		public string? MachineName { get; set; }
		public string? Address { get; set; }
		public string? LotRun { get; set; }
		public bool? FinishLot { get; set; }
		public bool? Status { get; set; }

		public void Mapping(Profile profile)
		{
			profile.CreateMap<Machine, MachineDto>().ReverseMap();
		}
	}
}
