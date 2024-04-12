using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AspNetMartenHtmxVsa.Core;

public static class LoggingRegistration
{
  public static IHostBuilder AddLogging(
    this IHostBuilder hostBuilder
  )
  {
    var serilogLogger = Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Debug()
      .WriteTo.Console()
      .CreateLogger();

    using var serilogLoggerFactory = new SerilogLoggerFactory(serilogLogger);
    var dotnetILogger = serilogLoggerFactory
      .CreateLogger<Program>();
    hostBuilder.ConfigureServices(s => s.AddSingleton<ILogger>(dotnetILogger));

    return hostBuilder.UseSerilog(serilogLogger);
  }
}
