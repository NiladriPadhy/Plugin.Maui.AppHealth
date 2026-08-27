namespace Plugin.Maui.AppHealth;

/// <summary>
/// Detects app, device, and environment problems that can degrade or block a MAUI app.
/// </summary>
public interface IAppHealth
{
	/// <summary>
	/// Gets a value indicating whether this target can collect native device signals.
	/// Always <c>true</c> on Android and iOS. The shared <c>net10.0</c> surface is for tests.
	/// </summary>
	bool IsSupported { get; }

	/// <summary>
	/// Gets which health signals the current platform can collect natively.
	/// </summary>
	AppHealthPlatformInfo Platform { get; }

	/// <summary>
	/// Gets the most recent inspection, or <c>null</c> before the first run.
	/// </summary>
	HealthReport? LastReport { get; }

	/// <summary>
	/// Gets a value indicating whether a watch session is running.
	/// </summary>
	bool IsWatching { get; }

	/// <summary>
	/// Raised when a watch session produces a report that differs from the previous one.
	/// </summary>
	event EventHandler<HealthChangedEventArgs>? HealthChanged;

	/// <summary>
	/// Raised when findings are added or removed between two watch reports.
	/// </summary>
	event EventHandler<HealthFindingChangedEventArgs>? FindingChanged;

	/// <summary>
	/// Collects current metrics and evaluates configured checks.
	/// </summary>
	Task<HealthReport> InspectAsync(InspectOptions? options = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Starts listening for battery, connectivity, thermal, and memory events, and re-inspects on an interval.
	/// </summary>
	void StartWatching(WatchOptions? options = null);

	/// <summary>
	/// Stops the current watch session.
	/// </summary>
	void StopWatching();

	/// <summary>
	/// Enables or disables plugin diagnostics.
	/// </summary>
	void EnableLogging(bool enabled, IAppHealthLogger? logger = null);
}
