using System;
using System.Linq.Expressions;
using Abp.Domain.Entities;
using RepoDb;

namespace Abp.RepoDb.Filters.Query;

public interface IRepoDbQueryFilterExecuter
{
    QueryGroup ExecuteFilter<TEntity, TPrimaryKey>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : class, IEntity<TPrimaryKey>;

    QueryGroup ExecuteFilter<TEntity, TPrimaryKey>()
        where TEntity : class, IEntity<TPrimaryKey>;
}
