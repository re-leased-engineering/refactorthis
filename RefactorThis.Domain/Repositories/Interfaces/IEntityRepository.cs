using System.Linq.Expressions;

namespace RefactorThis.Domain.Repositories.Interfaces;

public interface IEntityRepository<TEntity, TEntityId>
{
    Task<TEntity> FindByAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntityId> Add(TEntity newEntity);
    Task Save(TEntity entity);
}