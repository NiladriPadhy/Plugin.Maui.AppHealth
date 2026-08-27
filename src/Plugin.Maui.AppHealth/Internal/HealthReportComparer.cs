namespace Plugin.Maui.AppHealth;

static class HealthReportComparer
{
	public static bool AreEquivalent(HealthReport? left, HealthReport right)
	{
		if (left is null)
			return false;

		if (left.Status != right.Status)
			return false;

		if (left.Findings.Count != right.Findings.Count)
			return false;

		var leftCodes = left.Findings.Select(Signature).OrderBy(value => value, StringComparer.Ordinal);
		var rightCodes = right.Findings.Select(Signature).OrderBy(value => value, StringComparer.Ordinal);
		return leftCodes.SequenceEqual(rightCodes, StringComparer.Ordinal);
	}

	public static (IReadOnlyList<HealthFinding> Added, IReadOnlyList<HealthFinding> Removed) Diff(
		HealthReport? previous,
		HealthReport current)
	{
		var previousCodes = new HashSet<string>(
			previous?.Findings.Select(finding => finding.Code) ?? [],
			StringComparer.OrdinalIgnoreCase);
		var currentCodes = new HashSet<string>(
			current.Findings.Select(finding => finding.Code),
			StringComparer.OrdinalIgnoreCase);

		var added = current.Findings.Where(finding => !previousCodes.Contains(finding.Code)).ToArray();
		var removed = (previous?.Findings ?? []).Where(finding => !currentCodes.Contains(finding.Code)).ToArray();
		return (added, removed);
	}

	static string Signature(HealthFinding finding) => $"{finding.Code}:{finding.Severity}";
}
