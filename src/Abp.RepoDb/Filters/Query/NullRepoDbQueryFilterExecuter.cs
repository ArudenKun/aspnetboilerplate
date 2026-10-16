using System;
using System.Linq.Expressions;
using Abp.Domain.Entities;
using RepoDb;

namespace Abp.RepoDb.Filters.Query;

public class NullRepoDbQueryFilterExecuter : IRepoDbQueryFilterExecuter
{
    public static readonly NullRepoDbQueryFilterExecuter Instance = new();

    public QueryGroup ExecuteFilter<TEntity, TPrimaryKey>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : class, IEntity<TPrimaryKey>
    {
        return new QueryGroup(Array.Empty<QueryField>());
    }

    public QueryGroup ExecuteFilter<TEntity, TPrimaryKey>()
        where TEntity : class, IEntity<TPrimaryKey>
    {
        return new QueryGroup(Array.Empty<QueryField>());
    }
}
