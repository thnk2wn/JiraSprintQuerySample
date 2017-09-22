using Serilog;

namespace JIRASprintQuery.IO
{
    public static class LogFactory
    {
        public static ILogger Create()
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            return logger;
        }
    }
}
