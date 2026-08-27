namespace Plugin.Maui.AppHealth;

sealed class AppHealthImplementation : IAppHealth, IDisposable
{
	readonly AppHealthOptions _options;
	readonly IEnvironmentProbe _probe;
	readonly IClock _clock;
	readonly SemaphoreSlim _inspectLock = new(1, 1);
	readonly object _watchGate = new();

	IAppHealthLogger? _logger;
	bool _loggingEnabled;
	CancellationTokenSource? _watchCts;
	IDisposable? _watchSubscription;
	InspectOptions? _watchInspectOptions;
	TimeSpan _watchInterval;
	int _watchGeneration;

	public AppHealthImplementation(AppHealthOptions options, IEnvironmentProbe probe, IClock clock)
	{
		_options = options;
		_probe = probe;
		_clock = clock;
		_logger = options.Logger;
		_loggingEnabled = options.EnableLogging;
	}

	public bool IsSupported => _probe.IsSupported;

	public AppHealthPlatformInfo Platform => _probe.Platform;

	public HealthReport? LastReport { get; private set; }

	public bool IsWatching { get; private set; }

	public event EventHandler<HealthChangedEventArgs>? HealthChanged;

	public event EventHandler<HealthFindingChangedEventArgs>? FindingChanged;

	public async Task<HealthReport> InspectAsync(InspectOptions? options = null, CancellationToken cancellationToken = default)
	{
		await _inspectLock.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			return await InspectCoreAsync(options, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			_inspectLock.Release();
		}
	}

	public void StartWatching(WatchOptions? options = null)
	{
		lock (_watchGate)
		{
			if (IsWatching)
				StopWatchingCore();

			_watchInterval = options?.Interval ?? _options.WatchInterval;
			_watchInspectOptions = options?.Only is { Count: > 0 }
				? new InspectOptions { Only = options.Only }
				: null;
			_watchCts = new CancellationTokenSource();
			_watchGeneration++;
			IsWatching = true;

			_watchSubscription = _probe.Watch(() => ScheduleInspect(_watchCts.Token));
			_ = WatchLoopAsync(_watchCts.Token);
			Log(AppHealthLogLevel.Information, $"Watch started. Interval={_watchInterval}.");
		}
	}

	public void StopWatching()
	{
		lock (_watchGate)
		{
			StopWatchingCore();
		}
	}

	public void EnableLogging(bool enabled, IAppHealthLogger? logger = null)
	{
		_loggingEnabled = enabled;
		if (logger is not null)
			_logger = logger;

		_logger ??= new DebugAppHealthLogger();
		Log(AppHealthLogLevel.Information, enabled ? "Logging enabled." : "Logging disabled.");
	}

	public void Dispose()
	{
		StopWatching();
		_inspectLock.Dispose();
	}

	void StopWatchingCore()
	{
		if (!IsWatching)
			return;

		_watchCts?.Cancel();
		_watchCts?.Dispose();
		_watchCts = null;
		_watchSubscription?.Dispose();
		_watchSubscription = null;
		IsWatching = false;
		Log(AppHealthLogLevel.Information, "Watch stopped.");
	}

	async Task WatchLoopAsync(CancellationToken cancellationToken)
	{
		try
		{
			await PublishAsync(cancellationToken).ConfigureAwait(false);

			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(_watchInterval, cancellationToken).ConfigureAwait(false);
				await PublishAsync(cancellationToken).ConfigureAwait(false);
			}
		}
		catch (OperationCanceledException)
		{
			// Watch stopped.
		}
		catch (Exception ex)
		{
			Log(AppHealthLogLevel.Error, "Watch loop failed.", ex);
		}
	}

	void ScheduleInspect(CancellationToken cancellationToken)
	{
		var generation = _watchGeneration;
		_ = DebouncedPublishAsync(generation, cancellationToken);
	}

	async Task DebouncedPublishAsync(int generation, CancellationToken cancellationToken)
	{
		try
		{
			if (_options.WatchDebounce > TimeSpan.Zero)
				await Task.Delay(_options.WatchDebounce, cancellationToken).ConfigureAwait(false);

			if (generation != _watchGeneration)
				return;

			await PublishAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			// Watch stopped.
		}
		catch (Exception ex)
		{
			Log(AppHealthLogLevel.Error, "Debounced inspect failed.", ex);
		}
	}

	async Task PublishAsync(CancellationToken cancellationToken)
	{
		HealthReport report;
		HealthReport? previous;
		await _inspectLock.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			previous = LastReport;
			report = await InspectCoreAsync(_watchInspectOptions, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			_inspectLock.Release();
		}

		if (HealthReportComparer.AreEquivalent(previous, report))
			return;

		LastReport = report;
		HealthChanged?.Invoke(this, new HealthChangedEventArgs(previous, report));

		var (added, removed) = HealthReportComparer.Diff(previous, report);
		if (added.Count > 0 || removed.Count > 0)
			FindingChanged?.Invoke(this, new HealthFindingChangedEventArgs(report, added, removed));

		Log(AppHealthLogLevel.Information, $"Health changed to {report.Status} with {report.Findings.Count} finding(s).");
	}

	async Task<HealthReport> InspectCoreAsync(InspectOptions? options, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		var checks = _options.ResolveChecks(options?.Only);
		Log(AppHealthLogLevel.Debug, $"Inspecting {checks.Count} check group(s).");

		EnvironmentMetrics metrics;
		try
		{
			metrics = await _probe.CollectAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			Log(AppHealthLogLevel.Error, "Environment probe failed.", ex);
			metrics = new EnvironmentMetrics
			{
				CollectionErrors = [$"Environment probe failed: {ex.Message}"]
			};
		}

		var report = HealthEvaluator.Evaluate(metrics, _options, checks, _clock.UtcNow);
		LastReport = report;
		Log(AppHealthLogLevel.Information, $"Inspect complete: {report.Status}, {report.Findings.Count} finding(s).");
		return report;
	}

	void Log(AppHealthLogLevel level, string message, Exception? exception = null)
	{
		if (!_loggingEnabled)
			return;

		(_logger ?? new DebugAppHealthLogger()).Log(level, message, exception);
	}
}
