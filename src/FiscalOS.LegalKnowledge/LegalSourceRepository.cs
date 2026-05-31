namespace FiscalOS.LegalKnowledge;

public sealed class LegalSourceRepository
{
    private readonly List<LegalSource> _sources = new();

    public void Add(LegalSource source)
    {
        _sources.Add(source);
    }

    public IReadOnlyCollection<LegalSource> GetAll()
    {
        return _sources.AsReadOnly();
    }

    public LegalSource? FindByReference(string reference)
    {
        return _sources.FirstOrDefault(x => x.Reference == reference);
    }
}