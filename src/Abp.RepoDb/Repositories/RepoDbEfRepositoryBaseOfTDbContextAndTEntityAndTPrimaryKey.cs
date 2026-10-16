using System.Data.Common;
using System.Threading.Tasks;
using Abp.Data;
using Abp.Domain.Entities;
using Abp.Domain.Uow;
using Abp.Extensions;

namespace Abp.RepoDb.Repositories;

public class RepoDbEfRepositoryBase<TDbContext, TEntity, TPrimaryKey>
    : RepoDbRepositoryBase<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    private readonly IActiveTransactionProvider _activeTransactionProvider;
    private readonly ICurrentUnitOfWorkProvider _currentUnitOfWorkProvider;

    public RepoDbEfRepositoryBase(
        IActiveTransactionProvider activeTransactionProvider,
        ICurrentUnitOfWorkProvider currentUnitOfWorkProvider
    )
        : base(activeTransactionProvider)
    {
        _activeTransactionProvider = activeTransactionProvider;
        _currentUnitOfWorkProvider = currentUnitOfWorkProvider;
    }

    public ActiveTransactionProviderArgs ActiveTransactionProviderArgs
    {
        get
        {
            return new ActiveTransactionProviderArgs
            {
                ["ContextType"] = typeof(TDbContext),
                ["MultiTenancySide"] = MultiTenancySide,
            };
        }
    }

    public override DbConnection GetConnection()
    {
        return (DbConnection)
            _activeTransactionProvider.GetActiveConnection(ActiveTransactionProviderArgs);
    }

    public override async Task<DbConnection> GetConnectionAsync()
    {
        var connection = await _activeTransactionProvider.GetActiveConnectionAsync(
            ActiveTransactionProviderArgs
        );

        return (DbConnection)connection;
    }

    public override DbTransaction GetActiveTransaction()
    {
        return (DbTransaction)
            _activeTransactionProvider.GetActiveTransaction(ActiveTransactionProviderArgs);
    }

    public override async Task<DbTransaction> GetActiveTransactionAsync()
    {
        var transaction = await _activeTransactionProvider.GetActiveTransactionAsync(
            ActiveTransactionProviderArgs
        );

        return (DbTransaction)transaction;
    }

    public override int? Timeout =>
        _currentUnitOfWorkProvider.Current?.Options.Timeout?.TotalSeconds.To<int>();
}
