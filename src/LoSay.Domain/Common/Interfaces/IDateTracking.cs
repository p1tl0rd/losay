namespace LoSay.Domain.Common.Interfaces
{
	public interface IDateTracking
	{
		DateTimeOffset CreatedDate { get; set; }
		DateTimeOffset? LastModifiedDate { get; set; }
	}
}
