namespace FiscalOS.LegalKnowledge;

public sealed class RuleCandidateGenerator
{
    public RuleCandidate GenerateFromFragment(LegalFragment fragment)
    {
        return new RuleCandidate
        {
            Id = Guid.NewGuid(),
            FragmentId = fragment.Id,
            Name = $"Rule candidate for {fragment.Article}",
            Description = fragment.Text,
            ConditionExpression = string.Empty,
            Conclusion = string.Empty
        };
    }

    public IReadOnlyCollection<RuleCandidate> GenerateFromFragments(
        IReadOnlyCollection<LegalFragment> fragments)
    {
        return fragments
            .Select(GenerateFromFragment)
            .ToList()
            .AsReadOnly();
    }
}