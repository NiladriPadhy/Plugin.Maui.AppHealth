namespace Plugin.Maui.AppHealth.Tests;

sealed class FakeClock : IClock
{
	public DateTimeOffset UtcNow { get; set; } = new(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);

	public void Advance(TimeSpan value) => UtcNow += value;
}

sealed class FakeEnvironmentProbe : IEnvironmentProbe
{
	public EnvironmentMetrics Metrics { get; set; } = EnvironmentMetrics.Empty;

	public bool IsSupported { get; set; } = true;

	public AppHealthPlatformInfo Platform { get; set; } = AppHealthPlatformInfo.Net;

	public int CollectCount { get; private set; }

	public Action? OnChanged { get; private set; }

	public Task<EnvironmentMetrics> CollectAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		CollectCount++;
		return Task.FromResult(Metrics);
	}

	public IDisposable? Watch(Action onChanged)
	{
		OnChanged = onChanged;
		return new WatchHandle(this);
	}

	public void RaiseChanged() => OnChanged?.Invoke();

	sealed class WatchHandle(FakeEnvironmentProbe probe) : IDisposable
	{
		public void Dispose() => probe.OnChanged = null;
	}
}

static class HealthHarness
{
	public static AppHealthImplementation Create(
		AppHealthOptions? options = null,
		FakeEnvironmentProbe? probe = null,
		FakeClock? clock = null)
	{
		options ??= new AppHealthOptions();
		return AppHealth.Create(options, probe ?? new FakeEnvironmentProbe(), clock ?? new FakeClock());
	}

	public static EnvironmentMetrics Healthy() => new()
	{
		BatteryPercent = 80,
		BatteryState = BatteryChargeStateKind.Discharging,
		EnergySaverOn = false,
		FreeStorageBytes = 8L * 1024 * 1024 * 1024,
		TotalStorageBytes = 64L * 1024 * 1024 * 1024,
		AppDataWritable = true,
		AvailableMemoryBytes = 4L * 1024 * 1024 * 1024,
		TotalMemoryBytes = 8L * 1024 * 1024 * 1024,
		AppUsedMemoryBytes = 200L * 1024 * 1024,
		IsLowMemory = false,
		MemoryPressure = MemoryPressureKind.Normal,
		ThermalState = ThermalStateKind.Nominal,
		HasNetwork = true,
		HasInternet = true,
		IsConstrained = false,
		IsExpensive = false,
		IsAirplaneMode = false,
		IsVirtualDevice = false,
		DeviceModel = "Pixel",
		DeviceManufacturer = "Google",
		PlatformName = "Android",
		OsVersion = "14.0",
		Idiom = "Phone",
		AppVersion = "1.0.0",
		AppBuild = "1",
		DebuggerAttached = false
	};

	public static HealthReport Evaluate(EnvironmentMetrics metrics, AppHealthOptions? options = null)
	{
		options ??= new AppHealthOptions();
		return HealthEvaluator.Evaluate(
			metrics,
			options,
			options.Checks.EnabledKinds(),
			new DateTimeOffset(2026, 8, 27, 12, 0, 0, TimeSpan.Zero));
	}
}
