// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.Linq;
// using System.Linq.Expressions;
// using System.Threading;
// using System.Threading.Tasks;
// using Abp.Domain.Entities;
// using Abp.Domain.Entities.Auditing;
// using Abp.Domain.Uow;
// using Abp.Runtime.Session;
// using Abp.Timing;
// using RepoDb;
// using RepoDb.Enumerations;
//
// namespace Abp.RepoDb.Repositories;
//
// /// <summary>
// /// ORM independent implementation of <see cref="IRepoDbRepository{TEntity,TPrimaryKey}"/>.
// /// A derived class only has to say where the connection and the transaction come from
// /// (see <see cref="RepoDbEfRepository{TDbContext,TEntity,TPrimaryKey}"/> for EF Core).
// /// </summary>
// public abstract class AbpRepoDbRepositoryBase<TEntity, TPrimaryKey>
//     : IRepoDbRepository<TEntity, TPrimaryKey>
//     where TEntity : class, IEntity<TPrimaryKey>
// {
//     private static readonly bool IsSoftDeleteEntity = typeof(ISoftDelete).IsAssignableFrom(
//         typeof(TEntity)
//     );
//
//     private static readonly bool IsMustHaveTenantEntity = typeof(IMustHaveTenant).IsAssignableFrom(
//         typeof(TEntity)
//     );
//
//     private static readonly bool IsMayHaveTenantEntity = typeof(IMayHaveTenant).IsAssignableFrom(
//         typeof(TEntity)
//     );
//
//     private static readonly string[] SoftDeleteFieldNames = BuildSoftDeleteFieldNames();
//
//     /// <summary>The session, used for the creator/modifier/deleter user id and the tenant id.</summary>
//     public IAbpSession AbpSession { get; set; } = NullAbpSession.Instance;
//
//     /// <summary>Used to read the active data filters and the current tenant.</summary>
//     public ICurrentUnitOfWorkProvider CurrentUnitOfWorkProvider { get; set; }
//
//     public abstract IDbConnection Connection { get; }
//
//     public abstract IDbTransaction ActiveTransaction { get; }
//
//     // ------------------------------------------------------------------ typed reads
//
//     public virtual TEntity Get(TPrimaryKey id)
//     {
//         var entity = FirstOrDefault(id);
//         if (entity == null)
//         {
//             throw new EntityNotFoundException(typeof(TEntity), id);
//         }
//
//         return entity;
//     }
//
//     public virtual async Task<TEntity> GetAsync(
//         TPrimaryKey id,
//         CancellationToken cancellationToken = default
//     )
//     {
//         var entity = await FirstOrDefaultAsync(id, cancellationToken);
//         if (entity == null)
//         {
//             throw new EntityNotFoundException(typeof(TEntity), id);
//         }
//
//         return entity;
//     }
//
//     public virtual TEntity FirstOrDefault(TPrimaryKey id)
//     {
//         return FirstOrDefault(CreateEqualityExpressionForId(id));
//     }
//
//     public virtual Task<TEntity> FirstOrDefaultAsync(
//         TPrimaryKey id,
//         CancellationToken cancellationToken = default
//     )
//     {
//         return FirstOrDefaultAsync(CreateEqualityExpressionForId(id), cancellationToken);
//     }
//
//     public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
//     {
//         var where = ApplyFilters(predicate);
//         return Connection.Query(where, top: 1, transaction: ActiveTransaction).FirstOrDefault();
//     }
//
//     public virtual async Task<TEntity> FirstOrDefaultAsync(
//         Expression<Func<TEntity, bool>> predicate,
//         CancellationToken cancellationToken = default
//     )
//     {
//         var where = ApplyFilters(predicate);
//         var result = await Connection.QueryAsync(
//             where,
//             top: 1,
//             transaction: ActiveTransaction,
//             cancellationToken: cancellationToken
//         );
//         return result.FirstOrDefault();
//     }
//
//     public virtual TEntity Single(Expression<Func<TEntity, bool>> predicate)
//     {
//         var where = ApplyFilters(predicate);
//         return Connection.Query(where, top: 2, transaction: ActiveTransaction).Single();
//     }
//
//     public virtual async Task<TEntity> SingleAsync(
//         Expression<Func<TEntity, bool>> predicate,
//         CancellationToken cancellationToken = default
//     )
//     {
//         var where = ApplyFilters(predicate);
//         var result = await Connection.QueryAsync(
//             where,
//             top: 2,
//             transaction: ActiveTransaction,
//             cancellationToken: cancellationToken
//         );
//         return result.Single();
//     }
//
//     public virtual IEnumerable<TEntity> GetAll()
//     {
//         return GetAll(null);
//     }
//
//     public virtual Task<IEnumerable<TEntity>> GetAllAsync(
//         CancellationToken cancellationToken = default
//     )
//     {
//         return GetAllAsync(null, cancellationToken);
//     }
//
//     public virtual IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>>? predicate)
//     {
//         var where = ApplyFilters(predicate);
//         return where == null
//             ? Connection.QueryAll<TEntity>(transaction: ActiveTransaction)
//             : Connection.Query(where, transaction: ActiveTransaction);
//     }
//
//     public virtual Task<IEnumerable<TEntity>> GetAllAsync(
//         Expression<Func<TEntity, bool>> predicate,
//         CancellationToken cancellationToken = default
//     )
//     {
//         var where = ApplyFilters(predicate);
//         return where == null
//             ? Connection.QueryAllAsync<TEntity>(
//                 transaction: ActiveTransaction,
//                 cancellationToken: cancellationToken
//             )
//             : Connection.QueryAsync(
//                 where,
//                 transaction: ActiveTransaction,
//                 cancellationToken: cancellationToken
//             );
//     }
//
//     public virtual IEnumerable<TEntity> GetAllPaged(
//         Expression<Func<TEntity, bool>> predicate,
//         int pageIndex,
//         int pageSize,
//         string sortingProperty = null,
//         bool ascending = true
//     )
//     {
//         var where = ApplyFilters(predicate);
//         var orderBy = CreateOrderBy(sortingProperty, ascending);
//         return Connection.BatchQuery(
//             pageIndex,
//             pageSize,
//             orderBy,
//             where,
//             transaction: ActiveTransaction
//         );
//     }
//
//     public virtual Task<IEnumerable<TEntity>> GetAllPagedAsync(
//         Expression<Func<TEntity, bool>> predicate,
//         int pageIndex,
//         int pageSize,
//         string sortingProperty = null,
//         bool ascending = true,
//         CancellationToken cancellationToken = default
//     )
//     {
//         var where = ApplyFilters(predicate);
//         var orderBy = CreateOrderBy(sortingProperty, ascending);
//         return Connection.BatchQueryAsync(
//             pageIndex,
//             pageSize,
//             orderBy,
//             where,
//             transaction: ActiveTransaction,
//             cancellationToken: cancellationToken
//         );
//     }
//
//     public virtual int Count()
//     {
//         return (int)LongCount();
//     }
//
//     public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
//     {
//         return (int)await LongCountAsync(cancellationToken);
//     }
//
//     public virtual int Count(Expression<Func<TEntity, bool>> predicate)
//     {
//         return (int)LongCount(predicate);
//     }
//
//     public virtual async Task<int> CountAsync(
//         Expression<Func<TEntity, bool>> predicate,
//         CancellationToken cancellationToken = default
//     )
//     {
//         return (int)await LongCountAsync(predicate, cancellationToken);
//     }
//
//     public virtual long LongCount()
//     {
//         return LongCount(null);
//     }
//
//     public virtual Task<long> LongCountAsync(CancellationToken cancellationToken = default)
//     {
//         return LongCountAsync(null, cancellationToken);
//     }
//
//     public virtual long LongCount(Expression<Func<TEntity, bool>> predicate)
//     {
//         var where = ApplyFilters(predicate);
//         return where == null
//             ? Connection.CountAll<TEntity>(transaction: ActiveTransaction)
//             : Connection.Count(where, transaction: ActiveTransaction);
//     }
//
//     public virtual Task<long> LongCountAsync(
//         Expression<Func<TEntity, bool>> predicate,
//         CancellationToken cancellationToken = default
//     )
//     {
//         var where = ApplyFilters(predicate);
//         return where == null
//             ? Connection.CountAllAsync<TEntity>(
//                 transaction: ActiveTransaction,
//                 cancellationToken: cancellationToken
//             )
//             : Connection.CountAsync(
//                 where,
//                 transaction: ActiveTransaction,
//                 cancellationToken: cancellationToken
//             );
//     }
//
//     public virtual bool Exists(Expression<Func<TEntity, bool>> predicate)
//     {
//         var where = ApplyFilters(predicate);
//         return Connection.Exists(where, transaction: ActiveTransaction);
//     }
//
//     public virtual Task<bool> ExistsAsync(
//         Expression<Func<TEntity, bool>> predicate,
//         CancellationToken cancellationToken = default
//     )
//     {
//         var where = ApplyFilters(predicate);
//         return Connection.ExistsAsync(
//             where,
//             transaction: ActiveTransaction,
//             cancellationToken: cancellationToken
//         );
//     }
//
//     // ------------------------------------------------------------------ writes
//
//     public virtual TEntity Insert(TEntity entity)
//     {
//         InsertAndGetId(entity);
//         return entity;
//     }
//
//     public virtual async Task<TEntity> InsertAsync(
//         TEntity entity,
//         CancellationToken cancellationToken = default
//     )
//     {
//         await InsertAndGetIdAsync(entity, cancellationToken);
//         return entity;
//     }
//
//     public virtual TPrimaryKey InsertAndGetId(TEntity entity)
//     {
//         PrepareForInsert(entity);
//         var id = Connection.Insert<TEntity, TPrimaryKey>(entity, transaction: ActiveTransaction);
//         return AssignGeneratedId(entity, id);
//     }
//
//     public virtual async Task<TPrimaryKey> InsertAndGetIdAsync(
//         TEntity entity,
//         CancellationToken cancellationToken = default
//     )
//     {
//         PrepareForInsert(entity);
//         var id = await Connection.InsertAsync<TEntity, TPrimaryKey>(
//             entity,
//             transaction: ActiveTransaction,
//             cancellationToken: cancellationToken
//         );
//         return AssignGeneratedId(entity, id);
//     }
//
//     public virtual TEntity Update(TEntity entity)
//     {
//         PrepareForUpdate(entity);
//         Connection.Update(entity, transaction: ActiveTransaction);
//         return entity;
//     }
//
//     public virtual async Task<TEntity> UpdateAsync(
//         TEntity entity,
//         CancellationToken cancellationToken = default
//     )
//     {
//         PrepareForUpdate(entity);
//         await Connection.UpdateAsync(
//             entity,
//             transaction: ActiveTransaction,
//             cancellationToken: cancellationToken
//         );
//         return entity;
//     }
//
//     public virtual void Delete(TEntity entity)
//     {
//         if (entity is ISoftDelete)
//         {
//             PrepareForSoftDelete(entity);
//             Connection.Update(
//                 entity,
//                 fields: Field.From(SoftDeleteFieldNames),
//                 transaction: ActiveTransaction
//             );
//             return;
//         }
//
//         Connection.Delete<TEntity>(entity, transaction: ActiveTransaction);
//     }
//
//     public virtual async Task DeleteAsync(
//         TEntity entity,
//         CancellationToken cancellationToken = default
//     )
//     {
//         if (entity is ISoftDelete)
//         {
//             PrepareForSoftDelete(entity);
//             await Connection.UpdateAsync(
//                 entity,
//                 fields: Field.From(SoftDeleteFieldNames),
//                 transaction: ActiveTransaction,
//                 cancellationToken: cancellationToken
//             );
//             return;
//         }
//
//         await Connection.DeleteAsync<TEntity>(
//             entity,
//             transaction: ActiveTransaction,
//             cancellationToken: cancellationToken
//         );
//     }
//
//     public virtual void Delete(TPrimaryKey id)
//     {
//         var entity = FirstOrDefault(id);
//         if (entity != null)
//         {
//             Delete(entity);
//         }
//     }
//
//     public virtual async Task DeleteAsync(
//         TPrimaryKey id,
//         CancellationToken cancellationToken = default
//     )
//     {
//         var entity = await FirstOrDefaultAsync(id, cancellationToken);
//         if (entity != null)
//         {
//             await DeleteAsync(entity, cancellationToken);
//         }
//     }
//
//     public virtual void Delete(Expression<Func<TEntity, bool>> predicate)
//     {
//         if (IsSoftDeleteEntity)
//         {
//             // Soft delete needs the audit fields set per row, so load the matches and delete one by one.
//             foreach (var entity in GetAll(predicate).ToList())
//             {
//                 Delete(entity);
//             }
//
//             return;
//         }
//
//         var where = ApplyFilters(predicate);
//         Connection.Delete(where, transaction: ActiveTransaction);
//     }
//
//     public virtual async Task DeleteAsync(
//         Expression<Func<TEntity, bool>> predicate,
//         CancellationToken cancellationToken = default
//     )
//     {
//         if (IsSoftDeleteEntity)
//         {
//             foreach (var entity in (await GetAllAsync(predicate, cancellationToken)).ToList())
//             {
//                 await DeleteAsync(entity, cancellationToken);
//             }
//
//             return;
//         }
//
//         var where = ApplyFilters(predicate);
//         await Connection.DeleteAsync(
//             where,
//             transaction: ActiveTransaction,
//             cancellationToken: cancellationToken
//         );
//     }
//
//     // ------------------------------------------------------------------ raw SQL
//
//     public virtual IEnumerable<dynamic> Query(
//         string sql,
//         object param = null,
//         CommandType? commandType = null,
//         int? commandTimeout = null
//     )
//     {
//         return Connection.ExecuteQuery(
//             sql,
//             param,
//             commandType,
//             commandTimeout: commandTimeout,
//             transaction: ActiveTransaction
//         );
//     }
//
//     public virtual Task<IEnumerable<dynamic>> QueryAsync(
//         string sql,
//         object param = null,
//         CommandType? commandType = null,
//         int? commandTimeout = null,
//         CancellationToken cancellationToken = default
//     )
//     {
//         return Connection.ExecuteQueryAsync(
//             sql,
//             param,
//             commandType,
//             commandTimeout: commandTimeout,
//             transaction: ActiveTransaction,
//             cancellationToken: cancellationToken
//         );
//     }
//
//     public virtual IEnumerable<TAny> Query<TAny>(
//         string sql,
//         object param = null,
//         CommandType? commandType = null,
//         int? commandTimeout = null
//     )
//     {
//         return Connection.ExecuteQuery<TAny>(
//             sql,
//             param,
//             commandType,
//             commandTimeout: commandTimeout,
//             transaction: ActiveTransaction
//         );
//     }
//
//     public virtual Task<IEnumerable<TAny>> QueryAsync<TAny>(
//         string sql,
//         object param = null,
//         CommandType? commandType = null,
//         int? commandTimeout = null,
//         CancellationToken cancellationToken = default
//     )
//     {
//         return Connection.ExecuteQueryAsync<TAny>(
//             sql,
//             param,
//             commandType,
//             commandTimeout: commandTimeout,
//             transaction: ActiveTransaction,
//             cancellationToken: cancellationToken
//         );
//     }
//
//     public virtual int Execute(
//         string sql,
//         object param = null,
//         CommandType? commandType = null,
//         int? commandTimeout = null
//     )
//     {
//         return Connection.ExecuteNonQuery(
//             sql,
//             param,
//             commandType,
//             commandTimeout: commandTimeout,
//             transaction: ActiveTransaction
//         );
//     }
//
//     public virtual Task<int> ExecuteAsync(
//         string sql,
//         object param = null,
//         CommandType? commandType = null,
//         int? commandTimeout = null,
//         CancellationToken cancellationToken = default
//     )
//     {
//         return Connection.ExecuteNonQueryAsync(
//             sql,
//             param,
//             commandType,
//             commandTimeout: commandTimeout,
//             transaction: ActiveTransaction,
//             cancellationToken: cancellationToken
//         );
//     }
//
//     public virtual TResult ExecuteScalar<TResult>(
//         string sql,
//         object param = null,
//         CommandType? commandType = null,
//         int? commandTimeout = null
//     )
//     {
//         return Connection.ExecuteScalar<TResult>(
//             sql,
//             param,
//             commandType,
//             commandTimeout: commandTimeout,
//             transaction: ActiveTransaction
//         );
//     }
//
//     public virtual Task<TResult> ExecuteScalarAsync<TResult>(
//         string sql,
//         object param = null,
//         CommandType? commandType = null,
//         int? commandTimeout = null,
//         CancellationToken cancellationToken = default
//     )
//     {
//         return Connection.ExecuteScalarAsync<TResult>(
//             sql,
//             param,
//             commandType,
//             commandTimeout: commandTimeout,
//             transaction: ActiveTransaction,
//             cancellationToken: cancellationToken
//         );
//     }
//
//     // ------------------------------------------------------------------ filters
//
//     /// <summary>
//     /// Adds the active ISoftDelete / IMustHaveTenant / IMayHaveTenant data filters to <paramref name="predicate"/>.
//     /// Returns null when there is neither a predicate nor a filter, meaning "all rows".
//     /// </summary>
//     protected virtual Expression<Func<TEntity, bool>>? ApplyFilters(
//         Expression<Func<TEntity, bool>>? predicate
//     )
//     {
//         var result = predicate;
//
//         if (IsSoftDeleteEntity && IsFilterEnabled(AbpDataFilters.SoftDelete))
//         {
//             result = RepoDbPredicateBuilder.And(
//                 result,
//                 RepoDbPredicateBuilder.PropertyEquals<TEntity>(nameof(ISoftDelete.IsDeleted), false)
//             );
//         }
//
//         if (IsMustHaveTenantEntity && IsFilterEnabled(AbpDataFilters.MustHaveTenant))
//         {
//             // TenantId is a plain int here. A host call (no tenant) must see nothing, and no tenant has id 0.
//             result = RepoDbPredicateBuilder.And(
//                 result,
//                 RepoDbPredicateBuilder.PropertyEquals<TEntity>(
//                     nameof(IMustHaveTenant.TenantId),
//                     GetCurrentTenantId() ?? 0
//                 )
//             );
//         }
//
//         if (IsMayHaveTenantEntity && IsFilterEnabled(AbpDataFilters.MayHaveTenant))
//         {
//             // TenantId is int?, so a null current tenant (host) matches the host's rows through IS NULL.
//             result = RepoDbPredicateBuilder.And(
//                 result,
//                 RepoDbPredicateBuilder.PropertyEquals<TEntity>(
//                     nameof(IMayHaveTenant.TenantId),
//                     GetCurrentTenantId()
//                 )
//             );
//         }
//
//         return result;
//     }
//
//     /// <summary>Same rule as AbpDbContext: a filter is on only inside a Unit of Work that has it enabled.</summary>
//     protected virtual bool IsFilterEnabled(string filterName)
//     {
//         return CurrentUnitOfWorkProvider?.Current?.IsFilterEnabled(filterName) == true;
//     }
//
//     protected virtual int? GetCurrentTenantId()
//     {
//         var unitOfWork = CurrentUnitOfWorkProvider?.Current;
//         return unitOfWork != null ? unitOfWork.GetTenantId() : AbpSession.TenantId;
//     }
//
//     // ------------------------------------------------------------------ auditing
//
//     protected virtual void PrepareForInsert(TEntity entity)
//     {
//         if (entity is IHasCreationTime hasCreationTime && hasCreationTime.CreationTime == default)
//         {
//             hasCreationTime.CreationTime = Clock.Now;
//         }
//
//         if (entity is ICreationAudited creationAudited && !creationAudited.CreatorUserId.HasValue)
//         {
//             creationAudited.CreatorUserId = AbpSession.UserId;
//         }
//
//         var tenantId = GetCurrentTenantId();
//         if (entity is IMayHaveTenant mayHaveTenant)
//         {
//             if (!mayHaveTenant.TenantId.HasValue)
//             {
//                 mayHaveTenant.TenantId = tenantId;
//             }
//         }
//         else if (
//             entity is IMustHaveTenant mustHaveTenant
//             && mustHaveTenant.TenantId == default
//             && tenantId.HasValue
//         )
//         {
//             mustHaveTenant.TenantId = tenantId.Value;
//         }
//     }
//
//     protected virtual void PrepareForUpdate(TEntity entity)
//     {
//         if (entity is IHasModificationTime hasModificationTime)
//         {
//             hasModificationTime.LastModificationTime = Clock.Now;
//         }
//
//         if (entity is IModificationAudited modificationAudited)
//         {
//             modificationAudited.LastModifierUserId = AbpSession.UserId;
//         }
//     }
//
//     protected virtual void PrepareForSoftDelete(TEntity entity)
//     {
//         ((ISoftDelete)entity).IsDeleted = true;
//
//         if (entity is IHasDeletionTime hasDeletionTime)
//         {
//             hasDeletionTime.DeletionTime = Clock.Now;
//         }
//
//         if (entity is IDeletionAudited deletionAudited)
//         {
//             deletionAudited.DeleterUserId = AbpSession.UserId;
//         }
//     }
//
//     // ------------------------------------------------------------------ helpers
//
//     protected virtual Expression<Func<TEntity, bool>> CreateEqualityExpressionForId(TPrimaryKey id)
//     {
//         var parameter = Expression.Parameter(typeof(TEntity), "e");
//         var body = Expression.Equal(
//             Expression.Property(parameter, nameof(IEntity<TPrimaryKey>.Id)),
//             Expression.Constant(id, typeof(TPrimaryKey))
//         );
//         return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
//     }
//
//     private static IEnumerable<OrderField> CreateOrderBy(string sortingProperty, bool ascending)
//     {
//         var field = string.IsNullOrWhiteSpace(sortingProperty)
//             ? nameof(IEntity<TPrimaryKey>.Id)
//             : sortingProperty;
//         return new[] { new OrderField(field, ascending ? Order.Ascending : Order.Descending) };
//     }
//
//     private static TPrimaryKey AssignGeneratedId(TEntity entity, TPrimaryKey id)
//     {
//         if (entity.IsTransient())
//         {
//             entity.Id = id;
//         }
//
//         return entity.Id;
//     }
//
//     private static string[] BuildSoftDeleteFieldNames()
//     {
//         var names = new List<string> { nameof(ISoftDelete.IsDeleted) };
//
//         if (typeof(IHasDeletionTime).IsAssignableFrom(typeof(TEntity)))
//         {
//             names.Add(nameof(IHasDeletionTime.DeletionTime));
//         }
//
//         if (typeof(IDeletionAudited).IsAssignableFrom(typeof(TEntity)))
//         {
//             names.Add(nameof(IDeletionAudited.DeleterUserId));
//         }
//
//         return names.ToArray();
//     }
// }
