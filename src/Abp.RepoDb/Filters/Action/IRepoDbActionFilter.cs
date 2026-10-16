using Abp.Dependency;
using Abp.Domain.Entities;

namespace Abp.RepoDb.Filters.Action;

public interface IRepoDbActionFilter : ITransientDependency
{
    void ExecuteFilter<TEntity, TPrimaryKey>(TEntity entity)
        where TEntity : class, IEntity<TPrimaryKey>;
}
