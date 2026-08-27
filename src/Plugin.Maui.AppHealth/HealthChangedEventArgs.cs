namespace Plugin.Maui.AppHealth;

/// <summary>
/// Raised when a watch session produces a report that differs from the previous one.
/// </summary>
public sealed class HealthChangedEventArgs : EventArgs
{
	public HealthChangedEventArgs(HealthReport? previous, HealthReport current)
	{
		Previous = previous;
		Current = current;
	}

	public HealthReport? Previous { get; }

	public HealthReport Current { get; }
}
