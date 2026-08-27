using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

namespace Plugin.Maui.AppHealth;

/// <summary>
/// Registers the AppHealth plugin with the MAUI dependency injection container.
/// </summary>
public static class MauiAppBuilderExtensions
{
	/// <summary>
	/// Adds <see cref="IAppHealth"/> as a singleton.
	/// </summary>
	/// <example>
	/// <code>
	/// builder.UseAppHealth(options =>
	/// {
	///     options.EnableLogging = true;
	///     options.LowBatteryPercent = 15;
	///     options.CriticalStorageMegabytes = 64;
	/// });
	/// </code>
	/// </example>
	public static MauiAppBuilder UseAppHealth(this MauiAppBuilder builder, Action<AppHealthOptions>? configure = null)
	{
		ArgumentNullException.ThrowIfNull(builder);

		var options = new AppHealthOptions();
		configure?.Invoke(options);

		builder.Services.AddSingleton(options);
		builder.Services.AddSingleton<IAppHealth>(services =>
		{
			options.Logger ??= CreateLoggerAdapter(services);
			var health = AppHealth.Create(options);
			AppHealth.SetDefault(health);
			return health;
		});
		builder.Services.AddTransient<IMauiInitializeService, AppHealthInitializer>();

		return builder;
	}

	internal static IAppHealthLogger? CreateLoggerAdapter(IServiceProvider serviceProvider)
	{
		var factory = serviceProvider.GetService<ILoggerFactory>();
		return factory is null ? null : new MicrosoftLoggerAdapter(factory.CreateLogger("Plugin.Maui.AppHealth"));
	}
}
