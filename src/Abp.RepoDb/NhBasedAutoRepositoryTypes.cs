using Abp.RepoDb.Repositories;

namespace Abp.RepoDb;

public class NhBasedDapperAutoRepositoryTypes
{
    static NhBasedDapperAutoRepositoryTypes()
    {
        Default = new RepoDbAutoRepositoryTypeAttribute(
            typeof(IRepoDbRepository<>),
            typeof(IRepoDbRepository<,>),
            typeof(RepoDbRepositoryBase<>),
            typeof(RepoDbRepositoryBase<,>)
        );
    }

    public static RepoDbAutoRepositoryTypeAttribute Default { get; }
}
