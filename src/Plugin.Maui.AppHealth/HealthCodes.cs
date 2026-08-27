namespace Plugin.Maui.AppHealth;

/// <summary>
/// Stable finding codes that apps can switch on.
/// </summary>
public static class HealthCodes
{
	public const string BatteryLow = "battery.low";
	public const string BatteryCritical = "battery.critical";
	public const string BatteryChargingLow = "battery.charging_low";
	public const string BatteryUnknown = "battery.unknown";

	public const string PowerEnergySaver = "power.energy_saver";

	public const string StorageLow = "storage.low";
	public const string StorageCritical = "storage.critical";
	public const string StorageNotWritable = "storage.not_writable";

	public const string MemoryHigh = "memory.high";
	public const string MemoryCritical = "memory.critical";
	public const string MemoryPressure = "memory.pressure";

	public const string ThermalFair = "thermal.fair";
	public const string ThermalSerious = "thermal.serious";
	public const string ThermalCritical = "thermal.critical";

	public const string NetworkOffline = "network.offline";
	public const string NetworkNoInternet = "network.no_internet";
	public const string NetworkConstrained = "network.constrained";
	public const string NetworkExpensive = "network.expensive";
	public const string NetworkAirplane = "network.airplane";

	public const string DeviceVirtual = "device.virtual";
	public const string DeviceOsOutdated = "device.os_outdated";

	public const string AppDebugger = "app.debugger";
	public const string CheckFailed = "check.failed";
}
