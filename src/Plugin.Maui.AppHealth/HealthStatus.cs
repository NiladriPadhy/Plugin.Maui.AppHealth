namespace Plugin.Maui.AppHealth;

/// <summary>
/// Overall result of an inspection after findings are aggregated.
/// </summary>
public enum HealthStatus
{
	/// <summary>
	/// No warning or critical findings. Informational findings are allowed.
	/// </summary>
	Healthy,

	/// <summary>
	/// At least one warning and no critical findings.
	/// </summary>
	Degraded,

	/// <summary>
	/// At least one critical finding.
	/// </summary>
	Unhealthy
}
