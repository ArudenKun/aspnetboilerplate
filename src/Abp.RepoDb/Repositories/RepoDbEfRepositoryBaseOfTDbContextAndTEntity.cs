using Abp.Data;
using Abp.Domain.Entities;
using Abp.Domain.Uow;

namespace Abp.RepoDb.Repositories;

public class RepoDbEfRepositoryBase<TDbContext, TEntity>
    : RepoDbEfRepositoryBase<TDbContext, TEntity, int>,
        IRepoDbRepository<TEntity>
    where TEntity : class, IEntity<int>
    where TDbContext : class
{
    public RepoDbEfRepositoryBase(
        IActiveTransactionProvider activeTransactionProvider,
        ICurrentUnitOfWorkProvider currentUnitOfWorkProvider
    )
        : base(activeTransactionProvider, currentUnitOfWorkProvider) { }
}
