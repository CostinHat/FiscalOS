namespace FiscalOS.LegalKnowledge;

public sealed class FiscalRuleRepository
{
    private readonly List<FiscalRule> _rules = [];

    public void Add(FiscalRule rule)
    {
        _rules.Add(rule);
    }

    public IReadOnlyCollection<FiscalRule> GetAll()
    {
        return _rules.AsReadOnly();
    }

    public FiscalRule? FindById(Guid id)
    {
        return _rules.FirstOrDefault(x => x.Id == id);
    }
}