namespace Plugin.Maui.AppHealth;

/// <summary>
/// Normalized thermal pressure across Android and iOS.
/// </summary>
public enum ThermalStateKind
{
	Unknown,
	Nominal,
	Fair,
	Serious,
	Critical
}
