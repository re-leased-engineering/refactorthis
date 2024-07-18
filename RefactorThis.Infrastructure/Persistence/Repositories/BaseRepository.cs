using System.Linq.Expressions;
using RefactorThis.Domain.Repositories.Interfaces;

namespace RefactorThis.Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<TEntity, TEntityId> : IEntityRepository<TEntity, TEntityId> where TEntity : class, IEntity<TEntityId>
{
    private readonly List<TEntity> _dataSet = new ();

    public async Task<TEntity> FindByAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _dataSet.AsQueryable()
            .Where(predicate)
            .FirstOrDefault();
    }

    public async Task<TEntityId> Add(TEntity newEntity)
    {
        if (!_dataSet.Contains(newEntity))
        {
            _dataSet.Add(newEntity);
        }

        return newEntity.Id;
    }

    public async Task Save(TEntity entity)
    {
        var record = _dataSet.FirstOrDefault(i => i.Id.Equals(entity.Id));
        // update record here
    }
}