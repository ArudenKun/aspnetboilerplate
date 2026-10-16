using Abp.Dependency;
using Abp.Domain.Entities;

namespace Abp.RepoDb.Filters.Action;

public class RepoDbActionFilterExecuter : IRepoDbActionFilterExecuter, ITransientDependency
{
    private readonly IIocResolver _iocResolver;

    public RepoDbActionFilterExecuter(IIocResolver iocResolver)
    {
        _iocResolver = iocResolver;
    }

    public void ExecuteCreationAuditFilter<TEntity, TPrimaryKey>(TEntity entity)
        where TEntity : class, IEntity<TPrimaryKey>
    {
        _iocResolver
            .Resolve<CreationAuditRepoDbActionFilter>()
            .ExecuteFilter<TEntity, TPrimaryKey>(entity);
    }

    public void ExecuteModificationAuditFilter<TEntity, TPrimaryKey>(TEntity entity)
        where TEntity : class, IEntity<TPrimaryKey>
    {
        _iocResolver
            .Resolve<ModificationAuditRepoDbActionFilter>()
            .ExecuteFilter<TEntity, TPrimaryKey>(entity);
    }

    public void ExecuteDeletionAuditFilter<TEntity, TPrimaryKey>(TEntity entity)
        where TEntity : class, IEntity<TPrimaryKey>
    {
        _iocResolver
            .Resolve<DeletionAuditRepoDbActionFilter>()
            .ExecuteFilter<TEntity, TPrimaryKey>(entity);
    }
}
