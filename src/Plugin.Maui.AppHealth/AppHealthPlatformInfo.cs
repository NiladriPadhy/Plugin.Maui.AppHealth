namespace Plugin.Maui.AppHealth;

/// <summary>
/// Describes which health signals this target can collect natively.
/// </summary>
public sealed class AppHealthPlatformInfo
{
	public static AppHealthPlatformInfo Android { get; } = new(
		"Android",
		supportsThermal: true,
		supportsAirplaneMode: true,
		supportsLowMemoryFlag: true);

	public static AppHealthPlatformInfo iOS { get; } = new(
		"iOS",
		supportsThermal: true,
		supportsAirplaneMode: false,
		supportsLowMemoryFlag: false);

	public static AppHealthPlatformInfo Net { get; } = new(
		"net",
		supportsThermal: false,
		supportsAirplaneMode: false,
		supportsLowMemoryFlag: false);

	public AppHealthPlatformInfo(string name, bool supportsThermal, bool supportsAirplaneMode, bool supportsLowMemoryFlag)
	{
		Name = name;
		SupportsThermal = supportsThermal;
		SupportsAirplaneMode = supportsAirplaneMode;
		SupportsLowMemoryFlag = supportsLowMemoryFlag;
	}

	public string Name { get; }

	public bool SupportsThermal { get; }

	public bool SupportsAirplaneMode { get; }

	public bool SupportsLowMemoryFlag { get; }
}
