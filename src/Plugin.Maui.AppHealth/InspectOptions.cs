namespace Plugin.Maui.AppHealth;

/// <summary>
/// Per-call overrides for <see cref="IAppHealth.InspectAsync"/>.
/// </summary>
public sealed class InspectOptions
{
	/// <summary>
	/// When set, only these checks run. When omitted, <see cref="AppHealthOptions.Checks"/> is used.
	/// </summary>
	public IReadOnlyList<HealthCheckKind>? Only { get; init; }
}
