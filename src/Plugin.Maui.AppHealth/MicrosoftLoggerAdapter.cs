using Microsoft.Extensions.Logging;

namespace Plugin.Maui.AppHealth;

sealed class MicrosoftLoggerAdapter(ILogger logger) : IAppHealthLogger
{
	public void Log(AppHealthLogLevel level, string message, Exception? exception = null)
	{
		logger.Log(ToLogLevel(level), exception, "{Message}", message);
	}

	static LogLevel ToLogLevel(AppHealthLogLevel level) => level switch
	{
		AppHealthLogLevel.Trace => LogLevel.Trace,
		AppHealthLogLevel.Debug => LogLevel.Debug,
		AppHealthLogLevel.Information => LogLevel.Information,
		AppHealthLogLevel.Warning => LogLevel.Warning,
		AppHealthLogLevel.Error => LogLevel.Error,
		_ => LogLevel.Information
	};
}
