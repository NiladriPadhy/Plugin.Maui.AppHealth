namespace Plugin.Maui.AppHealth;

static class HealthEvaluator
{
	const long BytesPerMegabyte = 1024L * 1024L;

	public static HealthReport Evaluate(
		EnvironmentMetrics metrics,
		AppHealthOptions options,
		IReadOnlyList<HealthCheckKind> checks,
		DateTimeOffset capturedAt)
	{
		var enabled = checks.ToHashSet();
		var findings = new List<HealthFinding>();

		if (enabled.Contains(HealthCheckKind.Battery))
			AddBattery(findings, metrics, options);

		if (enabled.Contains(HealthCheckKind.PowerMode))
			AddPowerMode(findings, metrics);

		if (enabled.Contains(HealthCheckKind.Storage))
			AddStorage(findings, metrics, options);

		if (enabled.Contains(HealthCheckKind.Memory))
			AddMemory(findings, metrics, options);

		if (enabled.Contains(HealthCheckKind.Thermal))
			AddThermal(findings, metrics);

		if (enabled.Contains(HealthCheckKind.Network))
			AddNetwork(findings, metrics);

		if (enabled.Contains(HealthCheckKind.Device))
			AddDevice(findings, metrics, options);

		if (enabled.Contains(HealthCheckKind.AppRuntime))
			AddAppRuntime(findings, metrics);

		foreach (var error in metrics.CollectionErrors)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.AppRuntime,
				HealthCodes.CheckFailed,
				HealthSeverity.Warning,
				"A health signal could not be collected",
				error));
		}

		var status = findings.Any(finding => finding.Severity == HealthSeverity.Critical)
			? HealthStatus.Unhealthy
			: findings.Any(finding => finding.Severity == HealthSeverity.Warning)
				? HealthStatus.Degraded
				: HealthStatus.Healthy;

		return new HealthReport(capturedAt, status, metrics.ToEnvironment(), findings, checks);
	}

	static void AddBattery(List<HealthFinding> findings, EnvironmentMetrics metrics, AppHealthOptions options)
	{
		if (metrics.BatteryPercent is not { } percent || percent < 0)
		{
			if (metrics.BatteryState is BatteryChargeStateKind.Unknown or BatteryChargeStateKind.NotPresent)
			{
				findings.Add(new HealthFinding(
					HealthCheckKind.Battery,
					HealthCodes.BatteryUnknown,
					HealthSeverity.Info,
					"Battery level is unavailable",
					"The OS did not report a charge level. This is common on simulators and some desktop targets.",
					data: Data(("state", metrics.BatteryState.ToString()))));
			}

			return;
		}

		var charging = metrics.BatteryState is BatteryChargeStateKind.Charging or BatteryChargeStateKind.Full;
		var data = Data(
			("percent", FormatPercent(percent)),
			("state", metrics.BatteryState.ToString()));

		if (percent <= options.CriticalBatteryPercent)
		{
			if (charging)
			{
				findings.Add(new HealthFinding(
					HealthCheckKind.Battery,
					HealthCodes.BatteryChargingLow,
					HealthSeverity.Info,
					"Battery is critically low but charging",
					$"Charge is {FormatPercent(percent)} and the device is charging.",
					"Keep the device plugged in until the charge recovers.",
					data));
			}
			else
			{
				findings.Add(new HealthFinding(
					HealthCheckKind.Battery,
					HealthCodes.BatteryCritical,
					HealthSeverity.Critical,
					"Battery is critically low",
					$"Charge is {FormatPercent(percent)}, which is at or below the critical threshold of {FormatPercent(options.CriticalBatteryPercent)}.",
					"Plug in the device. Background work and network calls may stop without warning.",
					data));
			}

			return;
		}

		if (percent <= options.LowBatteryPercent)
		{
			if (charging)
			{
				findings.Add(new HealthFinding(
					HealthCheckKind.Battery,
					HealthCodes.BatteryChargingLow,
					HealthSeverity.Info,
					"Battery is low but charging",
					$"Charge is {FormatPercent(percent)} and the device is charging.",
					data: data));
			}
			else
			{
				findings.Add(new HealthFinding(
					HealthCheckKind.Battery,
					HealthCodes.BatteryLow,
					HealthSeverity.Warning,
					"Battery is low",
					$"Charge is {FormatPercent(percent)}, which is at or below the warning threshold of {FormatPercent(options.LowBatteryPercent)}.",
					"Ask the user to charge the device before starting long-running work.",
					data));
			}
		}
	}

	static void AddPowerMode(List<HealthFinding> findings, EnvironmentMetrics metrics)
	{
		if (metrics.EnergySaverOn != true)
			return;

		findings.Add(new HealthFinding(
			HealthCheckKind.PowerMode,
			HealthCodes.PowerEnergySaver,
			HealthSeverity.Warning,
			"Energy saver is on",
			"Low-power mode can defer background work, reduce refresh rates, and restrict networking.",
			"Disable Low Power Mode or Battery Saver if this feature needs full performance.",
			Data(("energySaver", "true"))));
	}

	static void AddStorage(List<HealthFinding> findings, EnvironmentMetrics metrics, AppHealthOptions options)
	{
		if (!metrics.AppDataWritable)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.Storage,
				HealthCodes.StorageNotWritable,
				HealthSeverity.Critical,
				"App data directory is not writable",
				"The plugin could not write a test file to the app data folder.",
				"Check disk space, sandbox permissions, and that the app has a valid data directory."));
		}

		if (metrics.FreeStorageBytes is not { } free)
			return;

		var freeMb = free / BytesPerMegabyte;
		var data = Data(
			("freeBytes", free.ToString()),
			("freeMegabytes", freeMb.ToString()),
			("totalBytes", metrics.TotalStorageBytes?.ToString() ?? string.Empty));

		if (freeMb <= options.CriticalStorageMegabytes)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.Storage,
				HealthCodes.StorageCritical,
				HealthSeverity.Critical,
				"Storage space is critically low",
				$"{freeMb} MB free, which is at or below the critical threshold of {options.CriticalStorageMegabytes} MB.",
				"Free space before saving files, caching, or downloading content.",
				data));
			return;
		}

		if (freeMb <= options.LowStorageMegabytes)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.Storage,
				HealthCodes.StorageLow,
				HealthSeverity.Warning,
				"Storage space is low",
				$"{freeMb} MB free, which is at or below the warning threshold of {options.LowStorageMegabytes} MB.",
				"Avoid large downloads until the user frees space.",
				data));
		}
	}

	static void AddMemory(List<HealthFinding> findings, EnvironmentMetrics metrics, AppHealthOptions options)
	{
		if (metrics.IsLowMemory == true || metrics.MemoryPressure == MemoryPressureKind.Critical)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.Memory,
				HealthCodes.MemoryPressure,
				HealthSeverity.Critical,
				"The OS reported memory pressure",
				"The system flagged this process as low on memory. Allocations and caches may be reclaimed.",
				"Release caches, reduce image sizes, and avoid large in-memory collections.",
				Data(("lowMemory", "true"), ("pressure", metrics.MemoryPressure.ToString()))));
		}

		if (metrics.TotalMemoryBytes is not > 0 || metrics.AvailableMemoryBytes is not { } available)
			return;

		var usedPercent = Math.Clamp((metrics.TotalMemoryBytes.Value - available) * 100d / metrics.TotalMemoryBytes.Value, 0, 100);
		var data = Data(
			("usedPercent", FormatPercent(usedPercent)),
			("availableBytes", available.ToString()),
			("totalBytes", metrics.TotalMemoryBytes.Value.ToString()),
			("appUsedBytes", metrics.AppUsedMemoryBytes?.ToString() ?? string.Empty));

		if (usedPercent >= options.CriticalMemoryPercent)
		{
			if (findings.All(finding => finding.Code != HealthCodes.MemoryPressure))
			{
				findings.Add(new HealthFinding(
					HealthCheckKind.Memory,
					HealthCodes.MemoryCritical,
					HealthSeverity.Critical,
					"System memory is critically full",
					$"About {FormatPercent(usedPercent)} of RAM is in use.",
					"Reduce memory use before opening media-heavy screens.",
					data));
			}

			return;
		}

		if (usedPercent >= options.HighMemoryPercent || metrics.MemoryPressure == MemoryPressureKind.Warning)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.Memory,
				HealthCodes.MemoryHigh,
				HealthSeverity.Warning,
				"System memory is high",
				$"About {FormatPercent(usedPercent)} of RAM is in use.",
				"Prefer paging large data and disposing unused resources.",
				data));
		}
	}

	static void AddThermal(List<HealthFinding> findings, EnvironmentMetrics metrics)
	{
		switch (metrics.ThermalState)
		{
			case ThermalStateKind.Fair:
				findings.Add(new HealthFinding(
					HealthCheckKind.Thermal,
					HealthCodes.ThermalFair,
					HealthSeverity.Warning,
					"Device temperature is elevated",
					"The OS reported a fair thermal state. The CPU or GPU may start throttling.",
					"Pause heavy animation, camera, or location work if the session is long.",
					Data(("thermal", "Fair"))));
				break;
			case ThermalStateKind.Serious:
				findings.Add(new HealthFinding(
					HealthCheckKind.Thermal,
					HealthCodes.ThermalSerious,
					HealthSeverity.Critical,
					"Device is overheating",
					"The OS reported a serious thermal state. Performance will drop and background work may stop.",
					"Stop CPU-heavy work and let the device cool.",
					Data(("thermal", "Serious"))));
				break;
			case ThermalStateKind.Critical:
				findings.Add(new HealthFinding(
					HealthCheckKind.Thermal,
					HealthCodes.ThermalCritical,
					HealthSeverity.Critical,
					"Device thermal state is critical",
					"The OS reported a critical thermal state. The app should shed load immediately.",
					"Stop sensors, video, and background processing.",
					Data(("thermal", "Critical"))));
				break;
		}
	}

	static void AddNetwork(List<HealthFinding> findings, EnvironmentMetrics metrics)
	{
		if (metrics.IsAirplaneMode == true)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.Network,
				HealthCodes.NetworkAirplane,
				HealthSeverity.Warning,
				"Airplane mode is on",
				"Radio interfaces are likely disabled.",
				"Ask the user to turn off Airplane mode for features that need the network.",
				Data(("airplane", "true"))));
		}

		if (metrics.HasNetwork == false)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.Network,
				HealthCodes.NetworkOffline,
				HealthSeverity.Critical,
				"No network connection",
				"The device is not connected to Wi-Fi, cellular, or another network interface.",
				"Queue work and retry when connectivity returns.",
				Data(("hasNetwork", "false"))));
			return;
		}

		if (metrics.HasInternet == false)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.Network,
				HealthCodes.NetworkNoInternet,
				HealthSeverity.Warning,
				"Network has no internet access",
				"A local interface is available, but internet reachability was not reported.",
				"Show an offline state instead of failing API calls immediately.",
				Data(("hasInternet", "false"))));
		}

		if (metrics.IsConstrained == true)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.Network,
				HealthCodes.NetworkConstrained,
				HealthSeverity.Warning,
				"Network access is constrained",
				"The OS marked this connection as constrained. Large transfers may fail or be delayed.",
				"Defer large downloads until an unconstrained network is available.",
				Data(("constrained", "true"))));
		}

		if (metrics.IsExpensive == true)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.Network,
				HealthCodes.NetworkExpensive,
				HealthSeverity.Info,
				"The current network may be metered",
				"The active profile looks like cellular or another expensive interface.",
				"Avoid large automatic downloads on a metered connection.",
				Data(("expensive", "true"))));
		}
	}

	static void AddDevice(List<HealthFinding> findings, EnvironmentMetrics metrics, AppHealthOptions options)
	{
		if (metrics.IsVirtualDevice)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.Device,
				HealthCodes.DeviceVirtual,
				HealthSeverity.Info,
				"Running on a simulator or emulator",
				"Virtual devices often omit battery, thermal, and some sensor signals.",
				data: Data(("virtual", "true"), ("model", metrics.DeviceModel ?? string.Empty))));
		}

		var minimum = ResolveMinimumOs(metrics.PlatformName, options);
		if (minimum is null || string.IsNullOrWhiteSpace(metrics.OsVersion))
			return;

		if (!Version.TryParse(NormalizeVersion(metrics.OsVersion), out var current))
			return;

		if (current < minimum)
		{
			findings.Add(new HealthFinding(
				HealthCheckKind.Device,
				HealthCodes.DeviceOsOutdated,
				HealthSeverity.Warning,
				"OS version is below the configured minimum",
				$"{metrics.PlatformName} {metrics.OsVersion} is lower than the required {minimum}.",
				"Warn the user that some features may be unavailable.",
				Data(("osVersion", metrics.OsVersion), ("minimum", minimum.ToString()))));
		}
	}

	static void AddAppRuntime(List<HealthFinding> findings, EnvironmentMetrics metrics)
	{
		if (!metrics.DebuggerAttached)
			return;

		findings.Add(new HealthFinding(
			HealthCheckKind.AppRuntime,
			HealthCodes.AppDebugger,
			HealthSeverity.Info,
			"A debugger is attached",
			"Timing, memory, and networking behavior can differ from a store build.",
			data: Data(("debugger", "true"))));
	}

	static Version? ResolveMinimumOs(string? platformName, AppHealthOptions options)
	{
		if (string.Equals(platformName, "Android", StringComparison.OrdinalIgnoreCase))
			return options.MinimumAndroidVersion;

		if (string.Equals(platformName, "iOS", StringComparison.OrdinalIgnoreCase))
			return options.MinimumIosVersion;

		return null;
	}

	static string NormalizeVersion(string value)
	{
		var parts = value.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		return parts.Length switch
		{
			0 => "0.0",
			1 => $"{parts[0]}.0",
			_ => string.Join('.', parts.Take(4))
		};
	}

	static string FormatPercent(double value) => $"{value:0.#}%";

	static IReadOnlyDictionary<string, string> Data(params (string Key, string Value)[] pairs)
	{
		var data = new Dictionary<string, string>(pairs.Length, StringComparer.Ordinal);
		foreach (var (key, value) in pairs)
			data[key] = value;

		return data;
	}
}
