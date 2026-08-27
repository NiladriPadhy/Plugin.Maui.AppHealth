namespace Plugin.Maui.AppHealth;

sealed class NetEnvironmentProbe : IEnvironmentProbe
{
	public bool IsSupported => false;

	public AppHealthPlatformInfo Platform => AppHealthPlatformInfo.Net;

	public Task<EnvironmentMetrics> CollectAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.FromResult(EnvironmentMetrics.Empty);
	}

	public IDisposable? Watch(Action onChanged) => null;
}
