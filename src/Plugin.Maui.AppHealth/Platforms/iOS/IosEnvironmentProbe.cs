#if IOS
using System.Runtime.InteropServices;
using Foundation;
using UIKit;

namespace Plugin.Maui.AppHealth;

sealed class IosEnvironmentProbe : IEnvironmentProbe
{
	public bool IsSupported => true;

	public AppHealthPlatformInfo Platform => AppHealthPlatformInfo.iOS;

	public Task<EnvironmentMetrics> CollectAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var builder = new EnvironmentMetricsBuilder();
		MauiSharedCollector.ApplySharedSignals(builder);
		Try("storage", builder, CollectStorage);
		Try("memory", builder, CollectMemory);
		Try("thermal", builder, CollectThermal);
		return Task.FromResult(builder.Build());
	}

	public IDisposable? Watch(Action onChanged)
	{
		var subscriptions = new CompositeDisposable();
		subscriptions.Add(MauiSharedCollector.WatchShared(onChanged));

		var thermal = NSNotificationCenter.DefaultCenter.AddObserver(
			NSProcessInfo.ThermalStateDidChangeNotification,
			_ => onChanged());
		subscriptions.Add(() => NSNotificationCenter.DefaultCenter.RemoveObserver(thermal));

		var memory = NSNotificationCenter.DefaultCenter.AddObserver(
			UIApplication.DidReceiveMemoryWarningNotification,
			_ => onChanged());
		subscriptions.Add(() => NSNotificationCenter.DefaultCenter.RemoveObserver(memory));

		return subscriptions;
	}

	static void CollectStorage(EnvironmentMetricsBuilder builder)
	{
		var path = NSSearchPath.GetDirectories(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User).FirstOrDefault();
		if (string.IsNullOrEmpty(path))
			return;

		var attributes = NSFileManager.DefaultManager.GetFileSystemAttributes(path, out var error);
		if (attributes is null)
		{
			if (error is not null)
				builder.AddError($"Failed to collect storage: {error.LocalizedDescription}");
			return;
		}

		builder.FreeStorageBytes = (long)attributes.FreeSize;
		builder.TotalStorageBytes = (long)attributes.Size;
	}

	static void CollectMemory(EnvironmentMetricsBuilder builder)
	{
		builder.TotalMemoryBytes = (long)NSProcessInfo.ProcessInfo.PhysicalMemory;

		var available = (long)OsProcAvailableMemory();
		if (available >= 0)
			builder.AvailableMemoryBytes = available;

		if (builder.TotalMemoryBytes is not > 0 || builder.AvailableMemoryBytes is not { } remaining)
			return;

		var usedPercent = (builder.TotalMemoryBytes.Value - remaining) * 100d / builder.TotalMemoryBytes.Value;
		builder.MemoryPressure = usedPercent >= 92
			? MemoryPressureKind.Critical
			: usedPercent >= 80
				? MemoryPressureKind.Warning
				: MemoryPressureKind.Normal;
	}

	static void CollectThermal(EnvironmentMetricsBuilder builder)
	{
		builder.ThermalState = NSProcessInfo.ProcessInfo.ThermalState switch
		{
			NSProcessInfoThermalState.Nominal => ThermalStateKind.Nominal,
			NSProcessInfoThermalState.Fair => ThermalStateKind.Fair,
			NSProcessInfoThermalState.Serious => ThermalStateKind.Serious,
			NSProcessInfoThermalState.Critical => ThermalStateKind.Critical,
			_ => ThermalStateKind.Unknown
		};
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

	[DllImport("/usr/lib/libSystem.dylib", EntryPoint = "os_proc_available_memory")]
	static extern nuint OsProcAvailableMemory();
}
#endif
