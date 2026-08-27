using Plugin.Maui.AppHealth;

namespace AppHealth.Sample;

public partial class MainPage : ContentPage, IAppHealthLogger
{
	readonly IAppHealth _health;
	readonly List<string> _logLines = [];

	public MainPage()
	{
		InitializeComponent();
		_health = Plugin.Maui.AppHealth.AppHealth.Current;
		_health.HealthChanged += OnHealthChanged;
		_health.FindingChanged += OnFindingChanged;
		_health.EnableLogging(true, this);
		_ = InspectAsync();
	}

	async void OnInspectClicked(object? sender, EventArgs e) => await InspectAsync();

	async void OnNetworkClicked(object? sender, EventArgs e) =>
		await InspectAsync(new InspectOptions { Only = [HealthCheckKind.Network] });

	void OnStartWatchClicked(object? sender, EventArgs e)
	{
		_health.StartWatching(new WatchOptions { Interval = TimeSpan.FromSeconds(15) });
		AppendLog("Watch started.");
		PlatformLabel.Text = DescribePlatform();
	}

	void OnStopWatchClicked(object? sender, EventArgs e)
	{
		_health.StopWatching();
		AppendLog("Watch stopped.");
		PlatformLabel.Text = DescribePlatform();
	}

	void OnLoggingToggled(object? sender, ToggledEventArgs e)
	{
		_health.EnableLogging(e.Value, this);
		AppendLog(e.Value ? "Logging enabled by user." : "Logging disabled by user.");
	}

	void OnHealthChanged(object? sender, HealthChangedEventArgs e) =>
		MainThread.BeginInvokeOnMainThread(() =>
		{
			AppendLog($"HEALTH {e.Previous?.Status ?? HealthStatus.Healthy} -> {e.Current.Status}");
			ShowReport(e.Current);
		});

	void OnFindingChanged(object? sender, HealthFindingChangedEventArgs e) =>
		MainThread.BeginInvokeOnMainThread(() =>
		{
			foreach (var added in e.Added)
				AppendLog($"ADDED {added.Severity} {added.Code}");
			foreach (var removed in e.Removed)
				AppendLog($"REMOVED {removed.Code}");
		});

	async Task InspectAsync(InspectOptions? options = null)
	{
		try
		{
			var report = await _health.InspectAsync(options);
			ShowReport(report);
		}
		catch (Exception ex)
		{
			AppendLog($"ERROR {ex.Message}");
			StatusLabel.Text = ex.Message;
		}
	}

	void ShowReport(HealthReport report)
	{
		PlatformLabel.Text = DescribePlatform();
		StatusLabel.Text = report.IsHealthy
			? $"Status: {report.Status}"
			: $"Status: {report.Status}  ·  {report.Findings.Count(finding => finding.Severity != HealthSeverity.Info)} problem(s)";

		var env = report.Environment;
		EnvironmentLabel.Text = string.Join(Environment.NewLine,
		[
			$"Device: {env.DeviceManufacturer} {env.DeviceModel} ({env.Idiom})",
			$"OS: {env.PlatformName} {env.OsVersion}  ·  virtual={env.IsVirtualDevice}",
			$"App: {env.AppVersion} ({env.AppBuild})",
			$"Battery: {FormatOptional(env.BatteryPercent, value => $"{value:0.#}%")}  ·  {env.BatteryState}  ·  saver={env.EnergySaverOn}",
			$"Storage: {FormatBytes(env.FreeStorageBytes)} free of {FormatBytes(env.TotalStorageBytes)}  ·  writable={env.AppDataWritable}",
			$"Memory: {FormatBytes(env.AvailableMemoryBytes)} available of {FormatBytes(env.TotalMemoryBytes)}  ·  used={FormatOptional(env.UsedMemoryPercent, value => $"{value:0.#}%")}  ·  {env.MemoryPressure}",
			$"Thermal: {env.ThermalState}",
			$"Network: net={env.HasNetwork}  internet={env.HasInternet}  constrained={env.IsConstrained}  expensive={env.IsExpensive}  airplane={env.IsAirplaneMode}"
		]);

		FindingsLabel.Text = report.Findings.Count == 0
			? "No problems detected."
			: string.Join(Environment.NewLine, report.Findings.Select(finding =>
				$"{finding.Severity}  {finding.Code}{Environment.NewLine}  {finding.Title}{Environment.NewLine}  {finding.Message}"));
	}

	string DescribePlatform() =>
		_health.IsSupported
			? $"Platform: {_health.Platform.Name}  ·  thermal={_health.Platform.SupportsThermal}  ·  airplane={_health.Platform.SupportsAirplaneMode}  ·  watching={_health.IsWatching}"
			: "Platform: not supported";

	public void Log(AppHealthLogLevel level, string message, Exception? exception = null)
	{
		var line = exception is null
			? $"{DateTime.Now:HH:mm:ss} {level}: {message}"
			: $"{DateTime.Now:HH:mm:ss} {level}: {message} ({exception.GetType().Name})";

		MainThread.BeginInvokeOnMainThread(() => AppendLog(line));
	}

	void AppendLog(string line)
	{
		_logLines.Insert(0, line);
		if (_logLines.Count > 40)
			_logLines.RemoveAt(_logLines.Count - 1);

		LogLabel.Text = string.Join(Environment.NewLine, _logLines);
	}

	static string FormatBytes(long? bytes) =>
		bytes is not { } value
			? "n/a"
			: value >= 1024L * 1024 * 1024
				? $"{value / (1024d * 1024 * 1024):0.#} GB"
				: $"{value / (1024d * 1024):0.#} MB";

	static string FormatOptional(double? value, Func<double, string> format) =>
		value is { } actual ? format(actual) : "n/a";
}
