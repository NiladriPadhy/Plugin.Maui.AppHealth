namespace Plugin.Maui.AppHealth;

sealed record EnvironmentMetrics
{
	public static EnvironmentMetrics Empty { get; } = new();

	public double? BatteryPercent { get; init; }
	public BatteryChargeStateKind BatteryState { get; init; } = BatteryChargeStateKind.Unknown;
	public bool? EnergySaverOn { get; init; }

	public long? FreeStorageBytes { get; init; }
	public long? TotalStorageBytes { get; init; }
	public bool AppDataWritable { get; init; } = true;

	public long? AvailableMemoryBytes { get; init; }
	public long? TotalMemoryBytes { get; init; }
	public long? AppUsedMemoryBytes { get; init; }
	public bool? IsLowMemory { get; init; }
	public MemoryPressureKind MemoryPressure { get; init; } = MemoryPressureKind.Unknown;

	public ThermalStateKind ThermalState { get; init; } = ThermalStateKind.Unknown;

	public bool? HasNetwork { get; init; }
	public bool? HasInternet { get; init; }
	public bool? IsConstrained { get; init; }
	public bool? IsExpensive { get; init; }
	public bool? IsAirplaneMode { get; init; }

	public bool IsVirtualDevice { get; init; }
	public string? DeviceModel { get; init; }
	public string? DeviceManufacturer { get; init; }
	public string? PlatformName { get; init; }
	public string? OsVersion { get; init; }
	public string? Idiom { get; init; }

	public string? AppVersion { get; init; }
	public string? AppBuild { get; init; }
	public bool DebuggerAttached { get; init; }

	public IReadOnlyList<string> CollectionErrors { get; init; } = [];

	public DeviceEnvironment ToEnvironment() => new(
		BatteryPercent,
		BatteryState,
		EnergySaverOn,
		FreeStorageBytes,
		TotalStorageBytes,
		AppDataWritable,
		AvailableMemoryBytes,
		TotalMemoryBytes,
		AppUsedMemoryBytes,
		IsLowMemory,
		MemoryPressure,
		ThermalState,
		HasNetwork,
		HasInternet,
		IsConstrained,
		IsExpensive,
		IsAirplaneMode,
		IsVirtualDevice,
		DeviceModel,
		DeviceManufacturer,
		PlatformName,
		OsVersion,
		Idiom,
		AppVersion,
		AppBuild,
		DebuggerAttached);
}
