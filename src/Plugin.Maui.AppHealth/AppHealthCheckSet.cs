namespace Plugin.Maui.AppHealth;

/// <summary>
/// Enables or disables groups of health checks.
/// </summary>
public sealed class AppHealthCheckSet
{
	public bool Battery { get; set; } = true;
	public bool PowerMode { get; set; } = true;
	public bool Storage { get; set; } = true;
	public bool Memory { get; set; } = true;
	public bool Thermal { get; set; } = true;
	public bool Network { get; set; } = true;
	public bool Device { get; set; } = true;
	public bool AppRuntime { get; set; } = true;

	public static AppHealthCheckSet All() => new();

	public static AppHealthCheckSet None() => new()
	{
		Battery = false,
		PowerMode = false,
		Storage = false,
		Memory = false,
		Thermal = false,
		Network = false,
		Device = false,
		AppRuntime = false
	};

	public bool IsEnabled(HealthCheckKind kind) => kind switch
	{
		HealthCheckKind.Battery => Battery,
		HealthCheckKind.PowerMode => PowerMode,
		HealthCheckKind.Storage => Storage,
		HealthCheckKind.Memory => Memory,
		HealthCheckKind.Thermal => Thermal,
		HealthCheckKind.Network => Network,
		HealthCheckKind.Device => Device,
		HealthCheckKind.AppRuntime => AppRuntime,
		_ => false
	};

	public IReadOnlyList<HealthCheckKind> EnabledKinds()
	{
		var kinds = new List<HealthCheckKind>(8);
		foreach (HealthCheckKind kind in Enum.GetValues<HealthCheckKind>())
		{
			if (IsEnabled(kind))
				kinds.Add(kind);
		}

		return kinds;
	}
}
