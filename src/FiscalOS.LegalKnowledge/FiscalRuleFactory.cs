namespace FiscalOS.LegalKnowledge;

public sealed class FiscalRuleFactory
{
    public FiscalRule CreateFromCandidate(RuleCandidate candidate)
    {
        return new FiscalRule
        {
            Id = Guid.NewGuid(),
            Name = candidate.Name,
            ConditionExpression = candidate.ConditionExpression,
            Conclusion = candidate.Conclusion,
            Version = "1.0"
        };
    }
}