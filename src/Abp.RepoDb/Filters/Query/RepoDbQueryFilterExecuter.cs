using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Abp.Dependency;
using Abp.Domain.Entities;
using RepoDb;
using RepoDb.Enumerations;

namespace Abp.RepoDb.Filters.Query;

public class RepoDbQueryFilterExecuter : IRepoDbQueryFilterExecuter, ITransientDependency
{
    private readonly IEnumerable<IRepoDbQueryFilter> _queryFilters;

    public RepoDbQueryFilterExecuter(IIocResolver iocResolver)
    {
        _queryFilters = iocResolver.ResolveAll<IRepoDbQueryFilter>();
    }

    public QueryGroup ExecuteFilter<TEntity, TPrimaryKey>(
        Expression<Func<TEntity, bool>>? predicate
    )
        where TEntity : class, IEntity<TPrimaryKey>
    {
        ICollection<IRepoDbQueryFilter> filters = _queryFilters.ToList();

        foreach (IRepoDbQueryFilter filter in filters)
        {
            predicate = filter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
        }

        if (predicate is null)
        {
            return new QueryGroup(Array.Empty<QueryField>());
        }

        QueryGroup pg = QueryGroup.Parse(predicate);
        return pg;
    }

    public QueryGroup ExecuteFilter<TEntity, TPrimaryKey>()
        where TEntity : class, IEntity<TPrimaryKey>
    {
        ICollection<IRepoDbQueryFilter> filters = _queryFilters.ToList();
        var fields = new List<QueryField>();

        foreach (var filter in filters)
        {
            QueryField? predicate = filter.ExecuteFilter<TEntity, TPrimaryKey>();
            if (predicate is null)
                continue;
            fields.Add(predicate);
        }

        return new QueryGroup(fields, Conjunction.And);
    }
}
