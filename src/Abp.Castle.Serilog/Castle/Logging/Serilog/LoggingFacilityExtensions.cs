using Castle.Facilities.Logging;

namespace Abp.Castle.Logging.Serilog
{
    public static class LoggingFacilityExtensions
    {
        public static LoggingFacility UseAbpSerilog(this LoggingFacility loggingFacility)
        {
            return loggingFacility.LogUsing<SerilogFactory>();
        }
    }
}
