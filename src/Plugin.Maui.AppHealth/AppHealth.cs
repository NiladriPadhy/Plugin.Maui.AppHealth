namespace Plugin.Maui.AppHealth;

/// <summary>
/// Entry point for the AppHealth plugin when dependency injection is not used.
/// </summary>
public static class AppHealth
{
	static IAppHealth? _current;

	/// <summary>
	/// Gets the shared <see cref="IAppHealth"/> instance.
	/// </summary>
	public static IAppHealth Current => _current ??= Create(new AppHealthOptions());

	/// <summary>
	/// Creates a new instance using the current platform environment probe.
	/// </summary>
	public static IAppHealth Create(AppHealthOptions? options = null)
	{
		options ??= new AppHealthOptions();
		return new AppHealthImplementation(options, CreateProbe(), SystemClock.Instance);
	}

	/// <summary>
	/// Replaces the shared instance. Intended for tests and custom implementations.
	/// </summary>
	public static void SetDefault(IAppHealth implementation) =>
		_current = implementation ?? throw new ArgumentNullException(nameof(implementation));

	internal static AppHealthImplementation Create(
		AppHealthOptions options,
		IEnvironmentProbe probe,
		IClock clock) =>
		new(options, probe, clock);

	static IEnvironmentProbe CreateProbe()
	{
#if ANDROID
		return new AndroidEnvironmentProbe();
#elif IOS
		return new IosEnvironmentProbe();
#else
		return new NetEnvironmentProbe();
#endif
	}
}
