namespace Plugin.Maui.AppHealth;

/// <summary>
/// Normalized battery charge state.
/// </summary>
public enum BatteryChargeStateKind
{
	Unknown,
	Charging,
	Discharging,
	Full,
	NotCharging,
	NotPresent
}
