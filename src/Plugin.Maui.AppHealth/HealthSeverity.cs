namespace Plugin.Maui.AppHealth;

/// <summary>
/// How serious a single finding is.
/// </summary>
public enum HealthSeverity
{
	/// <summary>
	/// Diagnostic context that does not change <see cref="HealthStatus"/>.
	/// </summary>
	Info,

	/// <summary>
	/// The app can keep running, but conditions are likely to degrade experience.
	/// </summary>
	Warning,

	/// <summary>
	/// A condition that can block or seriously impair the app.
	/// </summary>
	Critical
}
