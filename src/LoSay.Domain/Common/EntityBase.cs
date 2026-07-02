using LoSay.Domain.Common.Interfaces;

namespace LoSay.Domain.Common
{
	public abstract class EntityBase<TKey> : IEntityBase<TKey>
	{
		public TKey Id { get; set; }
	}
}
