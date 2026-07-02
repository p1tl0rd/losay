using AutoMapper;

namespace LoSay.Application.Mapping
{
	public interface IMapFrom<T>
	{
		void Mapping(Profile profile) =>
		profile.CreateMap(sourceType: typeof(T), destinationType: GetType());
	}
}
