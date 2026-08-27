#if ANDROID
#pragma warning disable CA1416, CA1422
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using MauiPlatform = Microsoft.Maui.ApplicationModel.Platform;

namespace Plugin.Maui.AppHealth;

sealed class AndroidEnvironmentProbe : IEnvironmentProbe
{
	public bool IsSupported => true;

	public AppHealthPlatformInfo Platform => AppHealthPlatformInfo.Android;

	public Task<EnvironmentMetrics> CollectAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var builder = new EnvironmentMetricsBuilder();
		MauiSharedCollector.ApplySharedSignals(builder);
		Try("storage", builder, CollectStorage);
		Try("memory", builder, CollectMemory);
		Try("thermal", builder, CollectThermal);
		Try("airplane", builder, CollectAirplane);
		return Task.FromResult(builder.Build());
	}

	public IDisposable? Watch(Action onChanged)
	{
		var subscriptions = new CompositeDisposable();
		subscriptions.Add(MauiSharedCollector.WatchShared(onChanged));

		if (OperatingSystem.IsAndroidVersionAtLeast(29))
		{
			try
			{
				var power = MauiPlatform.AppContext.GetSystemService(Context.PowerService) as PowerManager;
				if (power is not null)
				{
					var listener = new ThermalStatusListener(onChanged);
					power.AddThermalStatusListener(listener);
					subscriptions.Add(() =>
					{
						try
						{
							power.RemoveThermalStatusListener(listener);
						}
						catch
						{
							// Listener may already be gone if the process is tearing down.
						}

						listener.Dispose();
					});
				}
			}
			catch
			{
				// Thermal listeners require API 29 and a live context.
			}
		}

		return subscriptions;
	}

	static void CollectStorage(EnvironmentMetricsBuilder builder)
	{
		var path = MauiPlatform.AppContext.FilesDir?.AbsolutePath;
		if (string.IsNullOrEmpty(path))
			return;

		var stat = new StatFs(path);
		builder.FreeStorageBytes = stat.AvailableBytes;
		builder.TotalStorageBytes = stat.TotalBytes;
	}

	static void CollectMemory(EnvironmentMetricsBuilder builder)
	{
		if (MauiPlatform.AppContext.GetSystemService(Context.ActivityService) is not ActivityManager manager)
			return;

		var info = new ActivityManager.MemoryInfo();
		manager.GetMemoryInfo(info);
		builder.AvailableMemoryBytes = info.AvailMem;
		builder.TotalMemoryBytes = info.TotalMem;
		builder.IsLowMemory = info.LowMemory;

		if (info.LowMemory)
			builder.MemoryPressure = MemoryPressureKind.Critical;
		else if (info.TotalMem > 0 && info.AvailMem <= info.Threshold)
			builder.MemoryPressure = MemoryPressureKind.Warning;
		else
			builder.MemoryPressure = MemoryPressureKind.Normal;
	}

	static void CollectThermal(EnvironmentMetricsBuilder builder)
	{
		if (!OperatingSystem.IsAndroidVersionAtLeast(29))
			return;

		if (MauiPlatform.AppContext.GetSystemService(Context.PowerService) is not PowerManager power)
			return;

		builder.ThermalState = MapThermalStatus(power.CurrentThermalStatus);
	}

	static void CollectAirplane(EnvironmentMetricsBuilder builder)
	{
		builder.IsAirplaneMode = Settings.Global.GetInt(
			MauiPlatform.AppContext.ContentResolver,
			Settings.Global.AirplaneModeOn,
			0) == 1;
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

	static ThermalStateKind MapThermalStatus(ThermalStatus status) => status switch
	{
		ThermalStatus.None => ThermalStateKind.Nominal,
		ThermalStatus.Light => ThermalStateKind.Fair,
		ThermalStatus.Moderate => ThermalStateKind.Fair,
		ThermalStatus.Severe => ThermalStateKind.Serious,
		ThermalStatus.Critical => ThermalStateKind.Critical,
		ThermalStatus.Emergency => ThermalStateKind.Critical,
		ThermalStatus.Shutdown => ThermalStateKind.Critical,
		_ => ThermalStateKind.Unknown
	};

	sealed class ThermalStatusListener(Action onChanged) : Java.Lang.Object, PowerManager.IOnThermalStatusChangedListener
	{
		public void OnThermalStatusChanged(ThermalStatus status) => onChanged();
	}
}
#endif
