#if ANDROID || IOS
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace Plugin.Maui.AppHealth;

static class MauiSharedCollector
{
	public static void ApplySharedSignals(EnvironmentMetricsBuilder builder)
	{
		Try("battery", builder, CollectBattery);
		Try("network", builder, CollectNetwork);
		Try("device", builder, CollectDevice);
		Try("app", builder, CollectApp);
		Try("writable", builder, CollectWritable);
		builder.DebuggerAttached = Debugger.IsAttached;
		builder.AppUsedMemoryBytes = GC.GetTotalMemory(false);
	}

	public static IDisposable WatchShared(Action onChanged)
	{
		var subscriptions = new CompositeDisposable();

		try
		{
			Battery.BatteryInfoChanged += OnBattery;
			subscriptions.Add(() => Battery.BatteryInfoChanged -= OnBattery);
		}
		catch
		{
			// Battery APIs are unavailable on some hosts.
		}

		try
		{
			Battery.EnergySaverStatusChanged += OnEnergySaver;
			subscriptions.Add(() => Battery.EnergySaverStatusChanged -= OnEnergySaver);
		}
		catch
		{
			// Energy saver events are unavailable on some hosts.
		}

		try
		{
			Connectivity.ConnectivityChanged += OnConnectivity;
			subscriptions.Add(() => Connectivity.ConnectivityChanged -= OnConnectivity);
		}
		catch
		{
			// Connectivity events are unavailable on some hosts.
		}

		return subscriptions;

		void OnBattery(object? sender, BatteryInfoChangedEventArgs e) => onChanged();
		void OnEnergySaver(object? sender, EnergySaverStatusChangedEventArgs e) => onChanged();
		void OnConnectivity(object? sender, ConnectivityChangedEventArgs e) => onChanged();
	}

	static void CollectBattery(EnvironmentMetricsBuilder builder)
	{
		var level = Battery.ChargeLevel;
		builder.BatteryPercent = level < 0 ? null : Math.Clamp(level * 100d, 0, 100);
		builder.BatteryState = Battery.State switch
		{
			BatteryState.Charging => BatteryChargeStateKind.Charging,
			BatteryState.Discharging => BatteryChargeStateKind.Discharging,
			BatteryState.Full => BatteryChargeStateKind.Full,
			BatteryState.NotCharging => BatteryChargeStateKind.NotCharging,
			BatteryState.NotPresent => BatteryChargeStateKind.NotPresent,
			_ => BatteryChargeStateKind.Unknown
		};
		builder.EnergySaverOn = Battery.EnergySaverStatus switch
		{
			EnergySaverStatus.On => true,
			EnergySaverStatus.Off => false,
			_ => null
		};
	}

	static void CollectNetwork(EnvironmentMetricsBuilder builder)
	{
		var access = Connectivity.Current.NetworkAccess;
		builder.HasNetwork = access switch
		{
			NetworkAccess.None => false,
			NetworkAccess.Unknown => null,
			_ => true
		};
		builder.HasInternet = access switch
		{
			NetworkAccess.Internet or NetworkAccess.ConstrainedInternet => true,
			NetworkAccess.Unknown => null,
			_ => false
		};
		builder.IsConstrained = access is NetworkAccess.ConstrainedInternet;

		var profiles = Connectivity.Current.ConnectionProfiles;
		builder.IsExpensive = profiles.Contains(ConnectionProfile.Cellular);
	}

	static void CollectDevice(EnvironmentMetricsBuilder builder)
	{
		builder.DeviceModel = DeviceInfo.Current.Model;
		builder.DeviceManufacturer = DeviceInfo.Current.Manufacturer;
		builder.PlatformName = DeviceInfo.Current.Platform.ToString();
		builder.OsVersion = DeviceInfo.Current.VersionString;
		builder.Idiom = DeviceInfo.Current.Idiom.ToString();
		builder.IsVirtualDevice = DeviceInfo.Current.DeviceType == DeviceType.Virtual;
	}

	static void CollectApp(EnvironmentMetricsBuilder builder)
	{
		builder.AppVersion = AppInfo.Current.VersionString;
		builder.AppBuild = AppInfo.Current.BuildString;
	}

	static void CollectWritable(EnvironmentMetricsBuilder builder)
	{
		var directory = FileSystem.AppDataDirectory;
		Directory.CreateDirectory(directory);
		var path = Path.Combine(directory, ".plugin.maui.apphealth.write-test");
		File.WriteAllText(path, "ok");
		File.Delete(path);
		builder.AppDataWritable = true;
	}

	static void Try(string name, EnvironmentMetricsBuilder builder, Action<EnvironmentMetricsBuilder> collect)
	{
		try
		{
			collect(builder);
		}
		catch (Exception ex)
		{
			builder.AddError($"Failed to collect {name}: {ex.Message}");
		}
	}
}
#endif
