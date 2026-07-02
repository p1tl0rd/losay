namespace LoSay.Domain.Common.Interfaces
{
	public interface ISoftDeleteTracking
	{
		bool IsDeleted { get; set; }
		DateTime? DeletedDate { get; set; }
		string? DeletedBy { get; set; }
	}
}
