using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.MultiTenancy;
using Abp.Reflection.Extensions;
using AutoInterfaceAttributes;
using JetBrains.Annotations;

namespace Abp.RepoDb.Repositories;

public interface IRepoDbRepository<TEntity> : IRepoDbRepository<TEntity, int>
    where TEntity : class, IEntity<int>;

[PublicAPI]
[AutoInterface(Name = "IRepoDbRepository")]
public abstract class AbpRepoDbRepositoryBase<TEntity, TPrimaryKey>
    : IRepoDbRepository<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    // ReSharper disable once StaticMemberInGenericType
    public static MultiTenancySides? MultiTenancySide { get; private set; }

    static AbpRepoDbRepositoryBase()
    {
        var attr =
            typeof(TEntity).GetSingleAttributeOfTypeOrBaseTypesOrNull<MultiTenancySideAttribute>();
        if (attr != null)
        {
            MultiTenancySide = attr.Side;
        }
    }

    public abstract TEntity Single(TPrimaryKey id);

    public abstract IEnumerable<TEntity> GetAll();

    public virtual Task<TEntity> SingleAsync(TPrimaryKey id)
    {
        return Task.FromResult(Single(id));
    }

    public virtual Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return Task.FromResult(GetAll());
    }

    public abstract IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate);

    public virtual Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return Task.FromResult(GetAll(predicate));
    }

    public abstract int Count(Expression<Func<TEntity, bool>> predicate);

    public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return Task.FromResult(Count(predicate));
    }

    public abstract void Insert(TEntity entity);

    public virtual Task InsertAsync(TEntity entity)
    {
        Insert(entity);
        return Task.FromResult(0);
    }

    public abstract void Update(TEntity entity);

    public virtual Task UpdateAsync(TEntity entity)
    {
        Update(entity);
        return Task.FromResult(0);
    }

    public abstract void Delete(TEntity entity);

    public virtual Task DeleteAsync(TEntity entity)
    {
        Delete(entity);
        return Task.FromResult(0);
    }

    public abstract void Delete(Expression<Func<TEntity, bool>> predicate);

    public virtual Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
    {
        Delete(predicate);
        return Task.FromResult(0);
    }

    public abstract TPrimaryKey InsertAndGetId(TEntity entity);

    public virtual Task<TPrimaryKey> InsertAndGetIdAsync(TEntity entity)
    {
        return Task.FromResult(InsertAndGetId(entity));
    }

    public abstract TEntity Single(Expression<Func<TEntity, bool>> predicate);

    public virtual Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return Task.FromResult(Single(predicate));
    }

    public abstract TEntity FirstOrDefault(TPrimaryKey id);

    public virtual Task<TEntity> FirstOrDefaultAsync(TPrimaryKey id)
    {
        return Task.FromResult(FirstOrDefault(id));
    }

    public virtual Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return Task.FromResult(FirstOrDefault(predicate));
    }

    public abstract TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

    public abstract TEntity Get(TPrimaryKey id);

    public virtual Task<TEntity> GetAsync(TPrimaryKey id)
    {
        return Task.FromResult(Get(id));
    }

    protected static Expression<Func<TEntity, bool>> CreateEqualityExpressionForId(TPrimaryKey id)
    {
        ParameterExpression lambdaParam = Expression.Parameter(typeof(TEntity));

        var value = Convert.ChangeType(id, typeof(TPrimaryKey));
        var valueExpression = Expression.Constant(value, typeof(TPrimaryKey));

        BinaryExpression lambdaBody = Expression.Equal(
            Expression.PropertyOrField(lambdaParam, "Id"),
            valueExpression
        );

        return Expression.Lambda<Func<TEntity, bool>>(lambdaBody, lambdaParam);
    }
}
