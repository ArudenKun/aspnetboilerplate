using Abp.Data;
using Abp.Domain.Entities;

namespace Abp.RepoDb.Repositories;

public class RepoDbRepositoryBase<TEntity>
    : RepoDbRepositoryBase<TEntity, int>,
        IRepoDbRepository<TEntity>
    where TEntity : class, IEntity<int>
{
    public RepoDbRepositoryBase(IActiveTransactionProvider activeTransactionProvider)
        : base(activeTransactionProvider) { }
}
