namespace Plugin.Maui.AppHealth;

interface IClock
{
	DateTimeOffset UtcNow { get; }
}
