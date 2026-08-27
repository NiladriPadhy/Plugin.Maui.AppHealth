namespace Plugin.Maui.AppHealth;

/// <summary>
/// Result of inspecting the current app, device, and environment.
/// </summary>
public sealed class HealthReport
{
	public HealthReport(
		DateTimeOffset capturedAt,
		HealthStatus status,
		DeviceEnvironment environment,
		IReadOnlyList<HealthFinding> findings,
		IReadOnlyList<HealthCheckKind> checks)
	{
		CapturedAt = capturedAt;
		Status = status;
		Environment = environment;
		Findings = findings;
		Checks = checks;
	}

	public DateTimeOffset CapturedAt { get; }

	public HealthStatus Status { get; }

	/// <summary>
	/// <c>true</c> when <see cref="Status"/> is <see cref="HealthStatus.Healthy"/>.
	/// </summary>
	public bool IsHealthy => Status == HealthStatus.Healthy;

	public DeviceEnvironment Environment { get; }

	public IReadOnlyList<HealthFinding> Findings { get; }

	public IReadOnlyList<HealthCheckKind> Checks { get; }

	public IEnumerable<HealthFinding> Critical => Findings.Where(finding => finding.Severity == HealthSeverity.Critical);

	public IEnumerable<HealthFinding> Warnings => Findings.Where(finding => finding.Severity == HealthSeverity.Warning);

	public IEnumerable<HealthFinding> Infos => Findings.Where(finding => finding.Severity == HealthSeverity.Info);

	public HealthFinding? this[string code] =>
		Findings.FirstOrDefault(finding => string.Equals(finding.Code, code, StringComparison.OrdinalIgnoreCase));

	public IEnumerable<HealthFinding> this[HealthCheckKind kind] =>
		Findings.Where(finding => finding.Kind == kind);

	public bool Has(string code) => this[code] is not null;
}
