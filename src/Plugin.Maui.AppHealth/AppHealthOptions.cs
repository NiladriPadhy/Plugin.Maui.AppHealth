namespace Plugin.Maui.AppHealth;

/// <summary>
/// Shared configuration applied when the plugin is registered with <c>UseAppHealth</c>.
/// </summary>
public sealed class AppHealthOptions
{
	double _lowBatteryPercent = 20;
	double _criticalBatteryPercent = 5;
	long _lowStorageMegabytes = 512;
	long _criticalStorageMegabytes = 128;
	double _highMemoryPercent = 80;
	double _criticalMemoryPercent = 92;
	TimeSpan _watchInterval = TimeSpan.FromSeconds(30);
	TimeSpan _watchDebounce = TimeSpan.FromSeconds(1);

	/// <summary>
	/// Gets or sets a value indicating whether plugin logging starts enabled.
	/// </summary>
	public bool EnableLogging { get; set; }

	/// <summary>
	/// Gets or sets a custom logger. When <c>null</c>, the plugin uses Microsoft.Extensions.Logging if available, otherwise a debug logger.
	/// </summary>
	public IAppHealthLogger? Logger { get; set; }

	/// <summary>
	/// Charge percent at or below which a battery warning is raised. Default is 20.
	/// </summary>
	public double LowBatteryPercent
	{
		get => _lowBatteryPercent;
		set => _lowBatteryPercent = ValidatePercent(value, nameof(LowBatteryPercent));
	}

	/// <summary>
	/// Charge percent at or below which a critical battery finding is raised. Default is 5.
	/// </summary>
	public double CriticalBatteryPercent
	{
		get => _criticalBatteryPercent;
		set => _criticalBatteryPercent = ValidatePercent(value, nameof(CriticalBatteryPercent));
	}

	/// <summary>
	/// Free space in megabytes at or below which a storage warning is raised. Default is 512.
	/// </summary>
	public long LowStorageMegabytes
	{
		get => _lowStorageMegabytes;
		set => _lowStorageMegabytes = ValidateNonNegative(value, nameof(LowStorageMegabytes));
	}

	/// <summary>
	/// Free space in megabytes at or below which a critical storage finding is raised. Default is 128.
	/// </summary>
	public long CriticalStorageMegabytes
	{
		get => _criticalStorageMegabytes;
		set => _criticalStorageMegabytes = ValidateNonNegative(value, nameof(CriticalStorageMegabytes));
	}

	/// <summary>
	/// Used-memory percent at or above which a memory warning is raised. Default is 80.
	/// </summary>
	public double HighMemoryPercent
	{
		get => _highMemoryPercent;
		set => _highMemoryPercent = ValidatePercent(value, nameof(HighMemoryPercent));
	}

	/// <summary>
	/// Used-memory percent at or above which a critical memory finding is raised. Default is 92.
	/// </summary>
	public double CriticalMemoryPercent
	{
		get => _criticalMemoryPercent;
		set => _criticalMemoryPercent = ValidatePercent(value, nameof(CriticalMemoryPercent));
	}

	/// <summary>
	/// When set, a warning is raised if the Android OS version is lower.
	/// </summary>
	public Version? MinimumAndroidVersion { get; set; }

	/// <summary>
	/// When set, a warning is raised if the iOS version is lower.
	/// </summary>
	public Version? MinimumIosVersion { get; set; }

	/// <summary>
	/// How often a watch session re-inspects when no platform event fires. Default is 30 seconds.
	/// </summary>
	public TimeSpan WatchInterval
	{
		get => _watchInterval;
		set
		{
			if (value <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), "WatchInterval must be greater than zero.");

			_watchInterval = value;
		}
	}

	/// <summary>
	/// Delay after a platform event before re-inspecting. Default is 1 second.
	/// </summary>
	public TimeSpan WatchDebounce
	{
		get => _watchDebounce;
		set
		{
			if (value < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), "WatchDebounce cannot be negative.");

			_watchDebounce = value;
		}
	}

	/// <summary>
	/// Gets which check groups are enabled. All groups start enabled.
	/// </summary>
	public AppHealthCheckSet Checks { get; } = AppHealthCheckSet.All();

	internal IReadOnlyList<HealthCheckKind> ResolveChecks(IReadOnlyList<HealthCheckKind>? only)
	{
		if (only is { Count: > 0 })
			return only.Distinct().ToArray();

		return Checks.EnabledKinds();
	}

	static double ValidatePercent(double value, string name)
	{
		if (value is < 0 or > 100)
			throw new ArgumentOutOfRangeException(name, "Percent values must be between 0 and 100.");

		return value;
	}

	static long ValidateNonNegative(long value, string name)
	{
		if (value < 0)
			throw new ArgumentOutOfRangeException(name, "Storage thresholds cannot be negative.");

		return value;
	}
}
