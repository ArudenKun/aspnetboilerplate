using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Abp.Domain.Entities;
using Abp.Domain.Uow;
using Abp.RepoDb.Helpers;
using RepoDb;
using RepoDb.Enumerations;

namespace Abp.RepoDb.Filters.Query;

public class MayHaveTenantRepoDbQueryFilter : IRepoDbQueryFilter
{
    private readonly ICurrentUnitOfWorkProvider _currentUnitOfWorkProvider;

    public MayHaveTenantRepoDbQueryFilter(ICurrentUnitOfWorkProvider currentUnitOfWorkProvider)
    {
        _currentUnitOfWorkProvider = currentUnitOfWorkProvider;
    }

    private int? TenantId
    {
        get
        {
            DataFilterConfiguration filter =
                _currentUnitOfWorkProvider.Current.Filters.FirstOrDefault(x =>
                    x.FilterName == FilterName
                );
            if (filter.FilterParameters.ContainsKey(AbpDataFilters.Parameters.TenantId))
            {
                return (int?)filter.FilterParameters[AbpDataFilters.Parameters.TenantId];
            }

            return null;
        }
    }

    public string FilterName { get; } = AbpDataFilters.MayHaveTenant;

    public bool IsEnabled => _currentUnitOfWorkProvider.Current.IsFilterEnabled(FilterName);

    public QueryField? ExecuteFilter<TEntity, TPrimaryKey>()
        where TEntity : class, IEntity<TPrimaryKey>
    {
        QueryField? predicate = null;
        if (typeof(IMayHaveTenant).IsAssignableFrom(typeof(TEntity)) && IsEnabled)
        {
            predicate = new QueryField(
                AbpDataFilters.Parameters.TenantId,
                Operation.Equal,
#pragma warning disable CS8604 // Possible null reference argument.
                TenantId
#pragma warning restore CS8604 // Possible null reference argument.
            );
        }
        return predicate;
    }

    public Expression<Func<TEntity, bool>>? ExecuteFilter<TEntity, TPrimaryKey>(
        Expression<Func<TEntity, bool>>? predicate
    )
        where TEntity : class, IEntity<TPrimaryKey>
    {
        if (typeof(IMayHaveTenant).IsAssignableFrom(typeof(TEntity)) && IsEnabled)
        {
            PropertyInfo? propType = typeof(TEntity).GetProperty(nameof(IMayHaveTenant.TenantId));
            if (predicate == null)
            {
                predicate = ExpressionHelper.MakePredicate<TEntity>(
                    nameof(IMayHaveTenant.TenantId),
                    TenantId,
                    propType?.PropertyType
                );
            }
            else
            {
                ParameterExpression paramExpr = predicate.Parameters[0];
                MemberExpression memberExpr = Expression.Property(
                    paramExpr,
                    nameof(IMayHaveTenant.TenantId)
                );
                BinaryExpression body = Expression.AndAlso(
                    predicate.Body,
                    Expression.Equal(
                        memberExpr,
                        Expression.Constant(TenantId, propType!.PropertyType)
                    )
                );
                predicate = Expression.Lambda<Func<TEntity, bool>>(body, paramExpr);
            }
        }
        return predicate;
    }
}
