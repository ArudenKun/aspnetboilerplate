using Abp.Modules;

namespace Abp.Castle.Logging.Serilog
{
    /// <summary>
    /// ABP Castle Log4Net module.
    /// </summary>
    [DependsOn(typeof(AbpKernelModule))]
    public class AbpCastleSerilogModule : AbpModule { }
}
