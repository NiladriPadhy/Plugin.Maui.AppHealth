namespace Plugin.Maui.AppHealth.Tests;

public sealed class InspectTests
{
	[Fact]
	public async Task InspectAsync_UsesProbeMetrics()
	{
		var probe = new FakeEnvironmentProbe { Metrics = HealthHarness.Healthy() };
		using var health = HealthHarness.Create(probe: probe);

		var report = await health.InspectAsync();

		Assert.True(report.IsHealthy);
		Assert.Equal(1, probe.CollectCount);
		Assert.Same(report, health.LastReport);
	}

	[Fact]
	public async Task InspectAsync_OnlyRunsRequestedChecks()
	{
		var metrics = HealthHarness.Healthy() with
		{
			BatteryPercent = 2,
			BatteryState = BatteryChargeStateKind.Discharging,
			HasNetwork = false,
			HasInternet = false
		};
		var probe = new FakeEnvironmentProbe { Metrics = metrics };
		using var health = HealthHarness.Create(probe: probe);

		var report = await health.InspectAsync(new InspectOptions { Only = [HealthCheckKind.Network] });

		Assert.True(report.Has(HealthCodes.NetworkOffline));
		Assert.False(report.Has(HealthCodes.BatteryCritical));
		Assert.Equal(new[] { HealthCheckKind.Network }, report.Checks);
	}

	[Fact]
	public async Task NetSurface_IsNotSupported()
	{
		var health = AppHealth.Create(new AppHealthOptions());

		Assert.False(health.IsSupported);
		Assert.Equal("net", health.Platform.Name);

		var report = await health.InspectAsync();
		Assert.True(report.IsHealthy);
	}

	[Fact]
	public async Task StartWatching_PublishesChanges()
	{
		var probe = new FakeEnvironmentProbe { Metrics = HealthHarness.Healthy() };
		using var health = HealthHarness.Create(
			options: new AppHealthOptions
			{
				WatchInterval = TimeSpan.FromHours(1),
				WatchDebounce = TimeSpan.Zero
			},
			probe: probe);

		var changed = new TaskCompletionSource<HealthChangedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
		health.HealthChanged += (_, args) =>
		{
			if (args.Current.Has(HealthCodes.NetworkOffline))
				changed.TrySetResult(args);
		};

		health.StartWatching();
		Assert.True(health.IsWatching);

		await WaitUntilAsync(() => probe.CollectCount > 0);

		probe.Metrics = HealthHarness.Healthy() with { HasNetwork = false, HasInternet = false };
		probe.RaiseChanged();

		var args = await changed.Task.WaitAsync(TimeSpan.FromSeconds(5));
		Assert.Equal(HealthStatus.Unhealthy, args.Current.Status);
		Assert.True(args.Current.Has(HealthCodes.NetworkOffline));

		health.StopWatching();
		Assert.False(health.IsWatching);
	}

	[Fact]
	public void Options_RejectInvalidPercent()
	{
		var options = new AppHealthOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.LowBatteryPercent = 140);
		Assert.Throws<ArgumentOutOfRangeException>(() => options.WatchInterval = TimeSpan.Zero);
	}

	static async Task WaitUntilAsync(Func<bool> condition)
	{
		var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
		while (!condition())
		{
			if (DateTime.UtcNow > deadline)
				throw new TimeoutException("Condition was not met.");

			await Task.Delay(20);
		}
	}
}
