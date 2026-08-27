namespace Plugin.Maui.AppHealth;

interface IEnvironmentProbe
{
	bool IsSupported { get; }

	AppHealthPlatformInfo Platform { get; }

	Task<EnvironmentMetrics> CollectAsync(CancellationToken cancellationToken);

	IDisposable? Watch(Action onChanged);
}
