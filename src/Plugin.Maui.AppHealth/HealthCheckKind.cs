namespace Plugin.Maui.AppHealth;

/// <summary>
/// Groups of environment signals the plugin can inspect.
/// </summary>
public enum HealthCheckKind
{
	Battery,
	PowerMode,
	Storage,
	Memory,
	Thermal,
	Network,
	Device,
	AppRuntime
}
