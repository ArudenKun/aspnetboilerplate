using Abp.Modules;
using Abp.Reflection.Extensions;

namespace Abp.RepoDb;

[DependsOn(typeof(AbpKernelModule))]
public class AbpRepoDbModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactionScopeAvailable = false;
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(AbpRepoDbModule).GetAssembly());
    }
}
