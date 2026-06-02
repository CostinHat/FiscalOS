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

        foreach (var chain in aware.GoverningPurposeChains())
        {
            var rendered = string.Join(
                " serves ",
                chain.Nodes.Select(node => $"{node.Kind} \"{node.Description}\""));

            lines.Add($"Purpose chain: {rendered}.");
        }

        return new ExplanationNarrative(lines);
    }
}
