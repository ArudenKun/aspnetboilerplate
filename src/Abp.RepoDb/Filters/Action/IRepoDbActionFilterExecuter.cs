using Abp.Domain.Entities;

namespace Abp.RepoDb.Filters.Action;

public interface IRepoDbActionFilterExecuter
{
    void ExecuteCreationAuditFilter<TEntity, TPrimaryKey>(TEntity entity)
        where TEntity : class, IEntity<TPrimaryKey>;

    void ExecuteModificationAuditFilter<TEntity, TPrimaryKey>(TEntity entity)
        where TEntity : class, IEntity<TPrimaryKey>;

    void ExecuteDeletionAuditFilter<TEntity, TPrimaryKey>(TEntity entity)
        where TEntity : class, IEntity<TPrimaryKey>;
}
