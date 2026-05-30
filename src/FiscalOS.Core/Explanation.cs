namespace FiscalOS.Core;

public sealed record Explanation(string Text, IReadOnlyCollection<ObservedFact> Evidence);
