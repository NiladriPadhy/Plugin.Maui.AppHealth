namespace Plugin.Maui.AppHealth;

sealed class EnvironmentMetricsBuilder
{
	readonly List<string> _errors = [];

	public double? BatteryPercent { get; set; }
	public BatteryChargeStateKind BatteryState { get; set; } = BatteryChargeStateKind.Unknown;
	public bool? EnergySaverOn { get; set; }

	public long? FreeStorageBytes { get; set; }
	public long? TotalStorageBytes { get; set; }
	public bool AppDataWritable { get; set; } = true;

	public long? AvailableMemoryBytes { get; set; }
	public long? TotalMemoryBytes { get; set; }
	public long? AppUsedMemoryBytes { get; set; }
	public bool? IsLowMemory { get; set; }
	public MemoryPressureKind MemoryPressure { get; set; } = MemoryPressureKind.Unknown;

	public ThermalStateKind ThermalState { get; set; } = ThermalStateKind.Unknown;

	public bool? HasNetwork { get; set; }
	public bool? HasInternet { get; set; }
	public bool? IsConstrained { get; set; }
	public bool? IsExpensive { get; set; }
	public bool? IsAirplaneMode { get; set; }

	public bool IsVirtualDevice { get; set; }
	public string? DeviceModel { get; set; }
	public string? DeviceManufacturer { get; set; }
	public string? PlatformName { get; set; }
	public string? OsVersion { get; set; }
	public string? Idiom { get; set; }

	public string? AppVersion { get; set; }
	public string? AppBuild { get; set; }
	public bool DebuggerAttached { get; set; }

	public void AddError(string message)
	{
		if (!string.IsNullOrWhiteSpace(message))
			_errors.Add(message);
	}

	public EnvironmentMetrics Build() => new()
	{
		BatteryPercent = BatteryPercent,
		BatteryState = BatteryState,
		EnergySaverOn = EnergySaverOn,
		FreeStorageBytes = FreeStorageBytes,
		TotalStorageBytes = TotalStorageBytes,
		AppDataWritable = AppDataWritable,
		AvailableMemoryBytes = AvailableMemoryBytes,
		TotalMemoryBytes = TotalMemoryBytes,
		AppUsedMemoryBytes = AppUsedMemoryBytes,
		IsLowMemory = IsLowMemory,
		MemoryPressure = MemoryPressure,
		ThermalState = ThermalState,
		HasNetwork = HasNetwork,
		HasInternet = HasInternet,
		IsConstrained = IsConstrained,
		IsExpensive = IsExpensive,
		IsAirplaneMode = IsAirplaneMode,
		IsVirtualDevice = IsVirtualDevice,
		DeviceModel = DeviceModel,
		DeviceManufacturer = DeviceManufacturer,
		PlatformName = PlatformName,
		OsVersion = OsVersion,
		Idiom = Idiom,
		AppVersion = AppVersion,
		AppBuild = AppBuild,
		DebuggerAttached = DebuggerAttached,
		CollectionErrors = _errors.ToArray()
	};
}
