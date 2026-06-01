using System.Collections.Generic;
using System.Linq;

namespace FiscalOS.Runtime.Classification;

public static class PurposeAwareNarrativeProjector
{
    public static ExplanationNarrative Project(PurposeAwareExplanation aware)
    {
        ArgumentNullException.ThrowIfNull(aware);

        var lines = new List<string>(
            ExplanationNarrativeProjector.Project(aware.Explanation).Lines);

        foreach (var reference in aware.GoverningPurposes())
        {
            lines.Add($"Purpose: {reference.PurposeNode.Description}.");
        }

        return new ExplanationNarrative(lines);
    }
}
