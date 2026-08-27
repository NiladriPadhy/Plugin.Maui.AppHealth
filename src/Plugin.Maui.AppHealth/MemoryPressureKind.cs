namespace Plugin.Maui.AppHealth;

/// <summary>
/// Normalized system memory pressure.
/// </summary>
public enum MemoryPressureKind
{
	Unknown,
	Normal,
	Warning,
	Critical
}
