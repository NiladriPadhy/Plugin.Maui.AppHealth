namespace Plugin.Maui.AppHealth;

/// <summary>
/// One detected problem or diagnostic signal.
/// </summary>
public sealed class HealthFinding
{
	public HealthFinding(
		HealthCheckKind kind,
		string code,
		HealthSeverity severity,
		string title,
		string message,
		string? suggestion = null,
		IReadOnlyDictionary<string, string>? data = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(code);
		ArgumentException.ThrowIfNullOrWhiteSpace(title);
		ArgumentException.ThrowIfNullOrWhiteSpace(message);

		Kind = kind;
		Code = code;
		Severity = severity;
		Title = title;
		Message = message;
		Suggestion = suggestion;
		Data = data ?? new Dictionary<string, string>();
	}

	public HealthCheckKind Kind { get; }

	/// <summary>
	/// Stable identifier such as <see cref="HealthCodes.BatteryLow"/>.
	/// </summary>
	public string Code { get; }

	public HealthSeverity Severity { get; }

	public string Title { get; }

	public string Message { get; }

	/// <summary>
	/// Optional guidance the host app can show to the user.
	/// </summary>
	public string? Suggestion { get; }

	public IReadOnlyDictionary<string, string> Data { get; }

	public override string ToString() => $"{Severity} {Code}: {Title}";
}
