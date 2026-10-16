using Abp.RepoDb.Repositories;

namespace Abp.RepoDb;

public static class EfBasedDapperAutoRepositoryTypes
{
    static EfBasedDapperAutoRepositoryTypes()
    {
        Default = new RepoDbAutoRepositoryTypeAttribute(
            typeof(IRepoDbRepository<>),
            typeof(IRepoDbRepository<,>),
            typeof(RepoDbEfRepositoryBase<,>),
            typeof(RepoDbEfRepositoryBase<,,>)
        );
    }

    public static RepoDbAutoRepositoryTypeAttribute Default { get; }
}
