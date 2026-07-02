using System.ComponentModel.DataAnnotations.Schema;
using LoSay.Domain.Common.Interfaces;

namespace LoSay.Domain.Common
{
	public abstract class EntityAuditBase<T> : EntityBase<T>, IAuditable, IUserTracking
	{
		public DateTimeOffset CreatedDate { get; set; }
		public DateTimeOffset? LastModifiedDate { get; set; }
		[Column(TypeName = "nvarchar(50)")]
		public string? CreatedBy { get; set; }
		[Column(TypeName = "nvarchar(50)")]
		public string? LastModifiedBy { get; set; }
	}
}
