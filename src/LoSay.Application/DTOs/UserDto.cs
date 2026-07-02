using AutoMapper;
using LoSay.Application.Mapping;
using LoSay.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LoSay.Application.DTOs
{
	public class UserDto : IdentityUser, IMapFrom<User>
	{
		public int DivisionId { get; set; }
		public int SectionId { get; set; }
		public int ParentId { get; set; }
		public string? FullName { get; set; }
		public string? Code { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<LotState, LotStateDto>().ReverseMap();
		}
	}
}
