using System;
using System.Linq.Expressions;
using System.Reflection;
using Abp.Domain.Entities;
using Abp.Domain.Uow;
using Abp.RepoDb.Helpers;
using RepoDb;
using RepoDb.Enumerations;

namespace Abp.RepoDb.Filters.Query;

public class SoftDeleteRepoDbQueryFilter : IRepoDbQueryFilter
{
    private readonly ICurrentUnitOfWorkProvider _currentUnitOfWorkProvider;

    public SoftDeleteRepoDbQueryFilter(ICurrentUnitOfWorkProvider currentUnitOfWorkProvider)
    {
        _currentUnitOfWorkProvider = currentUnitOfWorkProvider;
    }

    public bool IsDeleted => false;

    public string FilterName { get; } = AbpDataFilters.SoftDelete;

    public bool IsEnabled => _currentUnitOfWorkProvider.Current.IsFilterEnabled(FilterName);

    public QueryField? ExecuteFilter<TEntity, TPrimaryKey>()
        where TEntity : class, IEntity<TPrimaryKey>
    {
        QueryField? predicate = null;
        if (IsFilterable<TEntity, TPrimaryKey>())
        {
            predicate = new QueryField(
                AbpDataFilters.Parameters.IsDeleted,
                Operation.Equal,
                IsDeleted
            );
        }

        return predicate;
    }

    public Expression<Func<TEntity, bool>>? ExecuteFilter<TEntity, TPrimaryKey>(
        Expression<Func<TEntity, bool>>? predicate
    )
        where TEntity : class, IEntity<TPrimaryKey>
    {
        if (IsFilterable<TEntity, TPrimaryKey>())
        {
            PropertyInfo? propType = typeof(TEntity).GetProperty(nameof(ISoftDelete.IsDeleted));
            if (predicate == null)
            {
                predicate = ExpressionHelper.MakePredicate<TEntity>(
                    nameof(ISoftDelete.IsDeleted),
                    IsDeleted,
                    propType?.PropertyType
                );
            }
            else
            {
                ParameterExpression paramExpr = predicate.Parameters[0];
                MemberExpression memberExpr = Expression.Property(
                    paramExpr,
                    nameof(ISoftDelete.IsDeleted)
                );
                BinaryExpression body = Expression.AndAlso(
                    predicate.Body,
                    Expression.Equal(
                        memberExpr,
                        Expression.Constant(IsDeleted, propType!.PropertyType)
                    )
                );
                predicate = Expression.Lambda<Func<TEntity, bool>>(body, paramExpr);
            }
        }

        return predicate;
    }

    private bool IsFilterable<TEntity, TPrimaryKey>()
        where TEntity : class, IEntity<TPrimaryKey>
    {
        return typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && IsEnabled;
    }
}
