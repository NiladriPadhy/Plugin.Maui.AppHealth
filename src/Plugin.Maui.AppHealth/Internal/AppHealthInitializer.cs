using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Plugin.Maui.AppHealth;

sealed class AppHealthInitializer : IMauiInitializeService
{
	public void Initialize(IServiceProvider services)
	{
		var options = services.GetService<AppHealthOptions>() ?? new AppHealthOptions();
		var health = services.GetService<IAppHealth>() ?? AppHealth.Current;

		if (options.EnableLogging)
		{
			var logger = options.Logger
				?? MauiAppBuilderExtensions.CreateLoggerAdapter(services)
				?? new DebugAppHealthLogger();
			health.EnableLogging(true, logger);
		}
	}
}
