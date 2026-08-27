namespace Plugin.Maui.AppHealth.Tests;

public sealed class EvaluatorTests
{
	[Fact]
	public void HealthyEnvironment_IsHealthy()
	{
		var report = HealthHarness.Evaluate(HealthHarness.Healthy());

		Assert.True(report.IsHealthy);
		Assert.Equal(HealthStatus.Healthy, report.Status);
		Assert.Empty(report.Warnings);
		Assert.Empty(report.Critical);
	}

	[Fact]
	public void BatteryLow_IsWarning()
	{
		var metrics = HealthHarness.Healthy() with { BatteryPercent = 12, BatteryState = BatteryChargeStateKind.Discharging };
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Degraded, report.Status);
		Assert.Equal(HealthCodes.BatteryLow, report[HealthCodes.BatteryLow]?.Code);
		Assert.Equal(HealthSeverity.Warning, report[HealthCodes.BatteryLow]?.Severity);
	}

	[Fact]
	public void BatteryCritical_IsUnhealthy()
	{
		var metrics = HealthHarness.Healthy() with { BatteryPercent = 3, BatteryState = BatteryChargeStateKind.Discharging };
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Unhealthy, report.Status);
		Assert.Equal(HealthSeverity.Critical, report[HealthCodes.BatteryCritical]?.Severity);
	}

	[Fact]
	public void BatteryCriticalWhileCharging_IsInfo()
	{
		var metrics = HealthHarness.Healthy() with { BatteryPercent = 3, BatteryState = BatteryChargeStateKind.Charging };
		var report = HealthHarness.Evaluate(metrics);

		Assert.True(report.IsHealthy);
		Assert.Equal(HealthCodes.BatteryChargingLow, report[HealthCodes.BatteryChargingLow]?.Code);
		Assert.Equal(HealthSeverity.Info, report[HealthCodes.BatteryChargingLow]?.Severity);
	}

	[Fact]
	public void EnergySaver_IsWarning()
	{
		var metrics = HealthHarness.Healthy() with { EnergySaverOn = true };
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Degraded, report.Status);
		Assert.True(report.Has(HealthCodes.PowerEnergySaver));
	}

	[Fact]
	public void StorageCritical_IsUnhealthy()
	{
		var metrics = HealthHarness.Healthy() with { FreeStorageBytes = 50L * 1024 * 1024 };
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Unhealthy, report.Status);
		Assert.True(report.Has(HealthCodes.StorageCritical));
	}

	[Fact]
	public void StorageLow_IsWarning()
	{
		var metrics = HealthHarness.Healthy() with { FreeStorageBytes = 200L * 1024 * 1024 };
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Degraded, report.Status);
		Assert.True(report.Has(HealthCodes.StorageLow));
	}

	[Fact]
	public void StorageNotWritable_IsCritical()
	{
		var metrics = HealthHarness.Healthy() with { AppDataWritable = false };
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Unhealthy, report.Status);
		Assert.True(report.Has(HealthCodes.StorageNotWritable));
	}

	[Fact]
	public void MemoryPressure_IsCritical()
	{
		var metrics = HealthHarness.Healthy() with { IsLowMemory = true, MemoryPressure = MemoryPressureKind.Critical };
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Unhealthy, report.Status);
		Assert.True(report.Has(HealthCodes.MemoryPressure));
	}

	[Fact]
	public void HighMemory_IsWarning()
	{
		var metrics = HealthHarness.Healthy() with
		{
			TotalMemoryBytes = 1000,
			AvailableMemoryBytes = 150,
			MemoryPressure = MemoryPressureKind.Warning
		};
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Degraded, report.Status);
		Assert.True(report.Has(HealthCodes.MemoryHigh));
	}

	[Fact]
	public void ThermalSerious_IsCritical()
	{
		var metrics = HealthHarness.Healthy() with { ThermalState = ThermalStateKind.Serious };
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Unhealthy, report.Status);
		Assert.True(report.Has(HealthCodes.ThermalSerious));
	}

	[Fact]
	public void ThermalFair_IsWarning()
	{
		var metrics = HealthHarness.Healthy() with { ThermalState = ThermalStateKind.Fair };
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Degraded, report.Status);
		Assert.True(report.Has(HealthCodes.ThermalFair));
	}

	[Fact]
	public void Offline_IsCritical()
	{
		var metrics = HealthHarness.Healthy() with { HasNetwork = false, HasInternet = false };
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Unhealthy, report.Status);
		Assert.True(report.Has(HealthCodes.NetworkOffline));
	}

	[Fact]
	public void NoInternet_IsWarning()
	{
		var metrics = HealthHarness.Healthy() with { HasNetwork = true, HasInternet = false };
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Degraded, report.Status);
		Assert.True(report.Has(HealthCodes.NetworkNoInternet));
	}

	[Fact]
	public void AirplaneMode_IsWarning()
	{
		var metrics = HealthHarness.Healthy() with { IsAirplaneMode = true, HasNetwork = true, HasInternet = true };
		var report = HealthHarness.Evaluate(metrics);

		Assert.True(report.Has(HealthCodes.NetworkAirplane));
	}

	[Fact]
	public void ConstrainedNetwork_IsWarning()
	{
		var metrics = HealthHarness.Healthy() with { IsConstrained = true };
		var report = HealthHarness.Evaluate(metrics);

		Assert.True(report.Has(HealthCodes.NetworkConstrained));
	}

	[Fact]
	public void ExpensiveNetwork_IsInfo()
	{
		var metrics = HealthHarness.Healthy() with { IsExpensive = true };
		var report = HealthHarness.Evaluate(metrics);

		Assert.True(report.IsHealthy);
		Assert.True(report.Has(HealthCodes.NetworkExpensive));
	}

	[Fact]
	public void VirtualDevice_IsInfo()
	{
		var metrics = HealthHarness.Healthy() with { IsVirtualDevice = true };
		var report = HealthHarness.Evaluate(metrics);

		Assert.True(report.IsHealthy);
		Assert.True(report.Has(HealthCodes.DeviceVirtual));
	}

	[Fact]
	public void OutdatedOs_IsWarning()
	{
		var options = new AppHealthOptions { MinimumAndroidVersion = new Version(15, 0) };
		var metrics = HealthHarness.Healthy() with { PlatformName = "Android", OsVersion = "14.0" };
		var report = HealthHarness.Evaluate(metrics, options);

		Assert.Equal(HealthStatus.Degraded, report.Status);
		Assert.True(report.Has(HealthCodes.DeviceOsOutdated));
	}

	[Fact]
	public void Debugger_IsInfo()
	{
		var metrics = HealthHarness.Healthy() with { DebuggerAttached = true };
		var report = HealthHarness.Evaluate(metrics);

		Assert.True(report.IsHealthy);
		Assert.True(report.Has(HealthCodes.AppDebugger));
	}

	[Fact]
	public void DisabledCheck_IsSkipped()
	{
		var options = new AppHealthOptions();
		options.Checks.Battery = false;
		var metrics = HealthHarness.Healthy() with { BatteryPercent = 1, BatteryState = BatteryChargeStateKind.Discharging };
		var report = HealthHarness.Evaluate(metrics, options);

		Assert.True(report.IsHealthy);
		Assert.False(report.Has(HealthCodes.BatteryCritical));
	}

	[Fact]
	public void CollectionError_IsWarning()
	{
		var metrics = HealthHarness.Healthy() with { CollectionErrors = ["Failed to collect thermal: denied"] };
		var report = HealthHarness.Evaluate(metrics);

		Assert.Equal(HealthStatus.Degraded, report.Status);
		Assert.True(report.Has(HealthCodes.CheckFailed));
	}
}
