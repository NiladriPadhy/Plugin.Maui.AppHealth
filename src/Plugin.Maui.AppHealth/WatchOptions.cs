namespace Plugin.Maui.AppHealth;

/// <summary>
/// Options for <see cref="IAppHealth.StartWatching"/>.
/// </summary>
public sealed class WatchOptions
{
	/// <summary>
	/// How often to re-inspect when no platform event fires. When omitted, <see cref="AppHealthOptions.WatchInterval"/> is used.
	/// </summary>
	public TimeSpan? Interval { get; init; }

	/// <summary>
	/// When set, only these checks run during the watch session.
	/// </summary>
	public IReadOnlyList<HealthCheckKind>? Only { get; init; }
}
