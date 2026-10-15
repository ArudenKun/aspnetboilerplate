using System;
using Castle.Core.Logging;
using Serilog;
using ILogger = Serilog.ILogger;
using ILoggerCastle = Castle.Core.Logging.ILogger;

namespace Abp.Castle.Logging.Serilog
{
    public class SerilogFactory : AbstractLoggerFactory
    {
        private readonly ILogger _logger;

        public SerilogFactory()
        {
            _logger = Log.Logger;
        }

        public SerilogFactory(ILogger logger)
        {
            _logger = logger;
        }

        public override ILoggerCastle Create(string name)
        {
            return new SerilogLogger(
                _logger.ForContext(global::Serilog.Core.Constants.SourceContextPropertyName, name),
                this
            );
        }

        public override ILoggerCastle Create(string name, LoggerLevel level)
        {
            throw new NotSupportedException(
                "Logger levels cannot be set at runtime. Please see Serilog's LoggerConfiguration.MinimumLevel."
            );
        }
    }
}
