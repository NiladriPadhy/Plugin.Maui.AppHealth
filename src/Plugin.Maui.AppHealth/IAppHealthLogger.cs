namespace Plugin.Maui.AppHealth;

/// <summary>
/// Receives diagnostic messages from the AppHealth plugin.
/// </summary>
public interface IAppHealthLogger
{
	void Log(AppHealthLogLevel level, string message, Exception? exception = null);
}
