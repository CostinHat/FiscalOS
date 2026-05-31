namespace FiscalOS.LegalKnowledge;

public sealed class FiscalRuleFactory
{
    public FiscalRule CreateFromCandidate(RuleCandidate candidate)
    {
        return new FiscalRule
        {
            Id = Guid.NewGuid(),
            Name = candidate.Name,
            Version = "1.0",
            EffectiveFrom = DateOnly.FromDateTime(DateTime.Today),
            EffectiveTo = null,
            ConditionExpression = candidate.ConditionExpression,
            Conclusion = candidate.Conclusion,
            LegalReference = candidate.FragmentId.ToString()
        };
    }
}