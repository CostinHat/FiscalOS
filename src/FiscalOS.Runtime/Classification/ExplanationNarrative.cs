namespace FiscalOS.Runtime.Classification;

public sealed record ExplanationNarrative(IReadOnlyList<string> Lines)
{
    public string Text => string.Join(Environment.NewLine, Lines);
}
