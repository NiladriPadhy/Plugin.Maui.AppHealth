namespace Plugin.Maui.AppHealth;

/// <summary>
/// Point-in-time device and environment measurements used to produce findings.
/// </summary>
public sealed class DeviceEnvironment
{
	public DeviceEnvironment(
		double? batteryPercent,
		BatteryChargeStateKind batteryState,
		bool? energySaverOn,
		long? freeStorageBytes,
		long? totalStorageBytes,
		bool appDataWritable,
		long? availableMemoryBytes,
		long? totalMemoryBytes,
		long? appUsedMemoryBytes,
		bool? isLowMemory,
		MemoryPressureKind memoryPressure,
		ThermalStateKind thermalState,
		bool? hasNetwork,
		bool? hasInternet,
		bool? isConstrained,
		bool? isExpensive,
		bool? isAirplaneMode,
		bool isVirtualDevice,
		string? deviceModel,
		string? deviceManufacturer,
		string? platformName,
		string? osVersion,
		string? idiom,
		string? appVersion,
		string? appBuild,
		bool debuggerAttached)
	{
		BatteryPercent = batteryPercent;
		BatteryState = batteryState;
		EnergySaverOn = energySaverOn;
		FreeStorageBytes = freeStorageBytes;
		TotalStorageBytes = totalStorageBytes;
		AppDataWritable = appDataWritable;
		AvailableMemoryBytes = availableMemoryBytes;
		TotalMemoryBytes = totalMemoryBytes;
		AppUsedMemoryBytes = appUsedMemoryBytes;
		IsLowMemory = isLowMemory;
		MemoryPressure = memoryPressure;
		ThermalState = thermalState;
		HasNetwork = hasNetwork;
		HasInternet = hasInternet;
		IsConstrained = isConstrained;
		IsExpensive = isExpensive;
		IsAirplaneMode = isAirplaneMode;
		IsVirtualDevice = isVirtualDevice;
		DeviceModel = deviceModel;
		DeviceManufacturer = deviceManufacturer;
		PlatformName = platformName;
		OsVersion = osVersion;
		Idiom = idiom;
		AppVersion = appVersion;
		AppBuild = appBuild;
		DebuggerAttached = debuggerAttached;
	}

	public double? BatteryPercent { get; }
	public BatteryChargeStateKind BatteryState { get; }
	public bool? EnergySaverOn { get; }
	public long? FreeStorageBytes { get; }
	public long? TotalStorageBytes { get; }
	public bool AppDataWritable { get; }
	public long? AvailableMemoryBytes { get; }
	public long? TotalMemoryBytes { get; }
	public long? AppUsedMemoryBytes { get; }
	public bool? IsLowMemory { get; }
	public MemoryPressureKind MemoryPressure { get; }
	public ThermalStateKind ThermalState { get; }
	public bool? HasNetwork { get; }
	public bool? HasInternet { get; }
	public bool? IsConstrained { get; }
	public bool? IsExpensive { get; }
	public bool? IsAirplaneMode { get; }
	public bool IsVirtualDevice { get; }
	public string? DeviceModel { get; }
	public string? DeviceManufacturer { get; }
	public string? PlatformName { get; }
	public string? OsVersion { get; }
	public string? Idiom { get; }
	public string? AppVersion { get; }
	public string? AppBuild { get; }
	public bool DebuggerAttached { get; }

	public double? UsedMemoryPercent =>
		TotalMemoryBytes is > 0 && AvailableMemoryBytes is { } available
			? Math.Clamp((TotalMemoryBytes.Value - available) * 100d / TotalMemoryBytes.Value, 0, 100)
			: null;

	public double? UsedStoragePercent =>
		TotalStorageBytes is > 0 && FreeStorageBytes is { } free
			? Math.Clamp((TotalStorageBytes.Value - free) * 100d / TotalStorageBytes.Value, 0, 100)
			: null;
}
