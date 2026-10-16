using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abp.Data;
using Abp.Domain.Entities;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.RepoDb.Filters.Action;
using Abp.RepoDb.Filters.Query;
using RepoDb;

namespace Abp.RepoDb.Repositories;

public class RepoDbRepositoryBase<TEntity, TPrimaryKey>
    : AbpRepoDbRepositoryBase<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    private readonly IActiveTransactionProvider _activeTransactionProvider;

    public RepoDbRepositoryBase(IActiveTransactionProvider activeTransactionProvider)
    {
        _activeTransactionProvider = activeTransactionProvider;

        EntityChangeEventHelper = NullEntityChangeEventHelper.Instance;
        QueryFilterExecuter = NullRepoDbQueryFilterExecuter.Instance;
        ActionFilterExecuter = NullRepoDbActionFilterExecuter.Instance;
    }

    public IEntityChangeEventHelper EntityChangeEventHelper { get; set; }
    public IRepoDbQueryFilterExecuter QueryFilterExecuter { get; set; }
    public IRepoDbActionFilterExecuter ActionFilterExecuter { get; set; }

    public virtual DbConnection GetConnection()
    {
        var connection = _activeTransactionProvider.GetActiveConnection(
            ActiveTransactionProviderArgs.Empty
        );
        return (DbConnection)connection;
    }

    public virtual async Task<DbConnection> GetConnectionAsync()
    {
        var connection = await _activeTransactionProvider.GetActiveConnectionAsync(
            ActiveTransactionProviderArgs.Empty
        );
        return (DbConnection)connection;
    }

    /// <summary>
    ///     Gets the active transaction. If Dapper is active then <see cref="IUnitOfWork" /> should be started before
    ///     and there must be an active transaction.
    /// </summary>
    /// <value>
    ///     The active transaction.
    /// </value>
    public virtual async Task<DbTransaction> GetActiveTransactionAsync()
    {
        var connection = await _activeTransactionProvider.GetActiveTransactionAsync(
            ActiveTransactionProviderArgs.Empty
        );
        return (DbTransaction)connection;
    }

    public virtual DbTransaction GetActiveTransaction()
    {
        var connection = _activeTransactionProvider.GetActiveTransaction(
            ActiveTransactionProviderArgs.Empty
        );
        return (DbTransaction)connection;
    }

    public virtual int? Timeout => null;

    public override TEntity Single(TPrimaryKey id)
    {
        return Single(CreateEqualityExpressionForId(id));
    }

    public override TEntity Single(Expression<Func<TEntity, bool>> predicate)
    {
        var qg = QueryFilterExecuter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
        return GetConnection()
            .Query<TEntity>(qg, transaction: GetActiveTransaction(), commandTimeout: Timeout)
            .Single();
    }

    public override TEntity FirstOrDefault(TPrimaryKey id)
    {
        return FirstOrDefault(CreateEqualityExpressionForId(id));
    }

    public override TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
    {
        var qg = QueryFilterExecuter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
        return GetConnection()
            .Query<TEntity>(qg, transaction: GetActiveTransaction(), commandTimeout: Timeout)
            .FirstOrDefault();
    }

    public override TEntity Get(TPrimaryKey id)
    {
        TEntity item = FirstOrDefault(id);
        if (item == null)
        {
            throw new EntityNotFoundException(typeof(TEntity), id);
        }

        return item;
    }

    public override IEnumerable<TEntity> GetAll()
    {
        var group = QueryFilterExecuter.ExecuteFilter<TEntity, TPrimaryKey>();
        return GetConnection()
            .Query<TEntity>(group, transaction: GetActiveTransaction(), commandTimeout: Timeout);
    }

    // public override IEnumerable<TEntity> Query(string query, object parameters = null)
    // {
    //     return GetConnection()
    //         .Query<TEntity>(query, parameters, GetActiveTransaction(), commandTimeout: Timeout);
    // }
    //
    // public override async Task<IEnumerable<TEntity>> QueryAsync(
    //     string query,
    //     object parameters = null
    // )
    // {
    //     var connection = await GetConnectionAsync();
    //     var activeTransaction = await GetActiveTransactionAsync();
    //     return await connection.QueryAsync<TEntity>(query, parameters, activeTransaction, Timeout);
    // }
    //
    // public override IEnumerable<TAny> Query<TAny>(string query, object parameters = null)
    // {
    //     return GetConnection()
    //         .Query<TAny>(query, parameters, GetActiveTransaction(), commandTimeout: Timeout);
    // }
    //
    // public override async Task<IEnumerable<TAny>> QueryAsync<TAny>(
    //     string query,
    //     object parameters = null
    // )
    // {
    //     var connection = await GetConnectionAsync();
    //     var activeTransaction = await GetActiveTransactionAsync();
    //     return await connection.QueryAsync<TAny>(query, parameters, activeTransaction, Timeout);
    // }
    //
    // public override int Execute(
    //     string query,
    //     object parameters = null,
    //     CommandType? commandType = null
    // )
    // {
    //     return GetConnection()
    //         .Execute(query, parameters, GetActiveTransaction(), Timeout, commandType);
    // }
    //
    // public override async Task<int> ExecuteAsync(
    //     string query,
    //     object parameters = null,
    //     CommandType? commandType = null
    // )
    // {
    //     var connection = await GetConnectionAsync();
    //     var activeTransaction = await GetActiveTransactionAsync();
    //     return await connection.ExecuteAsync(
    //         query,
    //         parameters,
    //         activeTransaction,
    //         Timeout,
    //         commandType
    //     );
    // }

    public override int Count(Expression<Func<TEntity, bool>> predicate)
    {
        var filteredPredicate = QueryFilterExecuter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
        return (int)
            GetConnection()
                .Count<TEntity>(
                    filteredPredicate,
                    transaction: GetActiveTransaction(),
                    commandTimeout: Timeout
                );
    }

    public override IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate)
    {
        var filteredPredicate = QueryFilterExecuter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
        return GetConnection()
            .Query<TEntity>(
                filteredPredicate,
                transaction: GetActiveTransaction(),
                commandTimeout: Timeout
            );
    }

    public override void Insert(TEntity entity)
    {
        InsertAndGetId(entity);
    }

    public override void Update(TEntity entity)
    {
        EntityChangeEventHelper.TriggerEntityUpdatingEvent(entity);
        ActionFilterExecuter.ExecuteModificationAuditFilter<TEntity, TPrimaryKey>(entity);
        GetConnection()
            .Update(entity, transaction: GetActiveTransaction(), commandTimeout: Timeout);
        EntityChangeEventHelper.TriggerEntityUpdatedEventOnUowCompleted(entity);
    }

    public override void Delete(TEntity entity)
    {
        EntityChangeEventHelper.TriggerEntityDeletingEvent(entity);
        if (entity is ISoftDelete)
        {
            ActionFilterExecuter.ExecuteDeletionAuditFilter<TEntity, TPrimaryKey>(entity);
            GetConnection()
                .Update(entity, transaction: GetActiveTransaction(), commandTimeout: Timeout);
        }
        else
        {
            GetConnection()
                .Delete(entity, transaction: GetActiveTransaction(), commandTimeout: Timeout);
        }

        EntityChangeEventHelper.TriggerEntityDeletedEventOnUowCompleted(entity);
    }

    public override void Delete(Expression<Func<TEntity, bool>> predicate)
    {
        IEnumerable<TEntity> items = GetAll(predicate);
        foreach (TEntity entity in items)
        {
            Delete(entity);
        }
    }

    public override TPrimaryKey InsertAndGetId(TEntity entity)
    {
        EntityChangeEventHelper.TriggerEntityCreatingEvent(entity);
        ActionFilterExecuter.ExecuteCreationAuditFilter<TEntity, TPrimaryKey>(entity);
        TPrimaryKey primaryKey = (TPrimaryKey)
            GetConnection()
                .Insert(entity, transaction: GetActiveTransaction(), commandTimeout: Timeout);
        EntityChangeEventHelper.TriggerEntityCreatedEventOnUowCompleted(entity);
        return primaryKey;
    }
}
