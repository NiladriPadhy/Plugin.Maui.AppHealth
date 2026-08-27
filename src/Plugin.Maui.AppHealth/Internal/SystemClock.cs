namespace Plugin.Maui.AppHealth;

sealed class SystemClock : IClock
{
	public static SystemClock Instance { get; } = new();

	public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
