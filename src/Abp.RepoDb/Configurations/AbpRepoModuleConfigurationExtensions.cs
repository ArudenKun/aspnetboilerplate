using Abp.Configuration.Startup;

namespace Abp.RepoDb.Configurations;

public static class AbpRepoModuleConfigurationExtensions
{
    public static IAbpRepoModuleConfiguration AbpRepoDb(
        this IModuleConfigurations moduleConfigurations
    ) => moduleConfigurations.AbpConfiguration.Get<IAbpRepoModuleConfiguration>();
}
