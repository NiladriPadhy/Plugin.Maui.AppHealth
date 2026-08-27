using System.Diagnostics;

namespace Plugin.Maui.AppHealth;

/// <summary>
/// Writes plugin diagnostics to <see cref="Debug.WriteLine(string?)"/>.
/// </summary>
public sealed class DebugAppHealthLogger : IAppHealthLogger
{
	public void Log(AppHealthLogLevel level, string message, Exception? exception = null)
	{
		var line = exception is null
			? $"[AppHealth] {level}: {message}"
			: $"[AppHealth] {level}: {message}{Environment.NewLine}{exception}";

		Debug.WriteLine(line);
	}
}
