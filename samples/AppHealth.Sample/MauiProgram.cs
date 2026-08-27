using Microsoft.Extensions.Logging;
using Plugin.Maui.AppHealth;

namespace AppHealth.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseAppHealth(options =>
			{
				options.EnableLogging = true;
				options.LowBatteryPercent = 20;
				options.CriticalBatteryPercent = 5;
				options.LowStorageMegabytes = 512;
				options.CriticalStorageMegabytes = 128;
				options.WatchInterval = TimeSpan.FromSeconds(15);
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
