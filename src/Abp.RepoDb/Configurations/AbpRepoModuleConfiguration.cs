using System;
using AutoInterfaceAttributes;
using RepoDb;

namespace Abp.RepoDb.Configurations;

[AutoInterface]
public class AbpRepoModuleConfiguration : IAbpRepoModuleConfiguration
{
    public GlobalConfiguration GlobalConfiguration => GlobalConfiguration.Setup();

    public IAbpRepoModuleConfiguration MapEntity<TEntity>(
        Action<EntityMapFluentDefinition<TEntity>> action
    )
        where TEntity : class
    {
        action(FluentMapper.Entity<TEntity>());
        return this;
    }

    public IAbpRepoModuleConfiguration MapType<TType>(Action<TypeMapFluentDefinition<TType>> action)
    {
        action(FluentMapper.Type<TType>());
        return this;
    }
}
