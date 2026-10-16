using System;
using System.Linq.Expressions;
using Abp.Dependency;
using Abp.Domain.Entities;
using RepoDb;

namespace Abp.RepoDb.Filters.Query;

public interface IRepoDbQueryFilter : ITransientDependency
{
    string FilterName { get; }

    bool IsEnabled { get; }

    Expression<Func<TEntity, bool>>? ExecuteFilter<TEntity, TPrimaryKey>(
        Expression<Func<TEntity, bool>>? predicate
    )
        where TEntity : class, IEntity<TPrimaryKey>;

    QueryField? ExecuteFilter<TEntity, TPrimaryKey>()
        where TEntity : class, IEntity<TPrimaryKey>;
}
