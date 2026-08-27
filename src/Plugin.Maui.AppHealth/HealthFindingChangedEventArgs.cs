namespace Plugin.Maui.AppHealth;

/// <summary>
/// Raised when findings are added or removed between two watch reports.
/// </summary>
public sealed class HealthFindingChangedEventArgs : EventArgs
{
	public HealthFindingChangedEventArgs(
		HealthReport current,
		IReadOnlyList<HealthFinding> added,
		IReadOnlyList<HealthFinding> removed)
	{
		Current = current;
		Added = added;
		Removed = removed;
	}

	public HealthReport Current { get; }

	public IReadOnlyList<HealthFinding> Added { get; }

	public IReadOnlyList<HealthFinding> Removed { get; }
}
