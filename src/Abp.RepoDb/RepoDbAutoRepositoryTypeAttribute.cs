using System;
using Abp.Domain.Repositories;
using JetBrains.Annotations;

namespace Abp.RepoDb;

public class RepoDbAutoRepositoryTypeAttribute : AutoRepositoryTypesAttribute
{
    public RepoDbAutoRepositoryTypeAttribute(
        Type repositoryInterface,
        Type repositoryInterfaceWithPrimaryKey,
        Type repositoryImplementation,
        Type repositoryImplementationWithPrimaryKey
    )
        : base(
            repositoryInterface,
            repositoryInterfaceWithPrimaryKey,
            repositoryImplementation,
            repositoryImplementationWithPrimaryKey
        ) { }
}
