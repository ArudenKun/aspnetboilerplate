// using System.Data;
// using Abp.Domain.Entities;
//
// namespace Abp.RepoDb.Repositories;
//
// /// <summary>
// /// RepoDb repository on top of NHibernate. The connection and transaction are the ones of the session that the
// /// current Unit of Work already holds, so RepoDb and NHibernate calls commit or roll back together.
// /// </summary>
// public class AbpRepoDbNhRepository<TEntity, TPrimaryKey>
//     : AbpRepoDbRepositoryBase<TEntity, TPrimaryKey>
//     where TEntity : class, IEntity<TPrimaryKey>
// {
//     private readonly ISessionProvider _sessionProvider;
//     private readonly IAbpRepoDbConfiguration _configuration;
//
//     public AbpRepoDbNhRepository(
//         ISessionProvider sessionProvider,
//         IAbpRepoDbConfiguration configuration
//     )
//     {
//         _sessionProvider = sessionProvider;
//         _configuration = configuration;
//     }
//
//     /// <summary>The NHibernate session of the current Unit of Work.</summary>
//     public virtual ISession Session => _sessionProvider.Session;
//
//     public override IDbConnection Connection
//     {
//         get
//         {
//             var session = Session;
//
//             if (_configuration.MapEntitiesFromOrmModel)
//             {
//                 RepoDbNhEntityMapper.EnsureMapped<TEntity>(session);
//             }
//
//             var connection = session.Connection;
//             if (connection.State != ConnectionState.Open)
//             {
//                 connection.Open();
//             }
//
//             return connection;
//         }
//     }
//
//     public override IDbTransaction ActiveTransaction
//     {
//         get
//         {
//             var session = Session;
//             var transaction = session.GetCurrentTransaction();
//             if (transaction == null || !transaction.IsActive)
//             {
//                 return null;
//             }
//
//             // NHibernate does not expose its ADO transaction. Enlisting a throw-away command in it is the public
//             // way to get at it: NHibernate sets Command.Transaction to the ADO transaction it began.
//             using (var command = session.Connection.CreateCommand())
//             {
//                 transaction.Enlist(command);
//                 return command.Transaction;
//             }
//         }
//     }
// }
//
// /// <summary>RepoDb repository for entities with an <see cref="int"/> primary key.</summary>
// public class AbpRepoDbNhRepository<TEntity>
//     : AbpRepoDbNhRepository<TEntity, int>,
//         IRepoDbRepository<TEntity>
//     where TEntity : class, IEntity<int>
// {
//     public AbpRepoDbNhRepository(
//         ISessionProvider sessionProvider,
//         IAbpRepoDbConfiguration configuration
//     )
//         : base(sessionProvider, configuration) { }
// }
