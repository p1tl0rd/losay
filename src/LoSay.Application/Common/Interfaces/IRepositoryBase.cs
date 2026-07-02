using System.Linq.Expressions;
using LoSay.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace LoSay.Application.Common.Interfaces;

public interface IRepositoryQueryBase<T, K> where T : EntityBase<K>
{
	IQueryable<T> FindAll(bool trackChanges = false);
	IQueryable<T> FindAll(bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties);
	IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false);
	IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties);

	Task<T?> GetByIdAsync(K id);
	Task<T?> GetByIdAsync(K id, params Expression<Func<T, object>>[] includeProperties);
}
public interface IRepositoryBase<T, K> : IRepositoryQueryBase<T, K> where T : EntityBase<K>
{
	Task<K> CreateAsync(T entity);
	Task<IList<K>> CreateListAsync(IEnumerable<T> entities);
	Task UpdateAsync(T entity);
	Task UpdateListAsync(IEnumerable<T> entities);
	Task DeleteAsync(T entity);
	Task DeleteListAsync(IEnumerable<T> entities);
	Task<int> SaveChangeAsync();
	Task<IDbContextTransaction> BeginTransactionAsync();
	Task EndTransactionAsync();
	Task RollBackTransactionAsync();
}
