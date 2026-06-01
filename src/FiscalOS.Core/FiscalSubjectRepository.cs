namespace FiscalOS.Core;

public sealed class FiscalSubjectRepository
{
    private readonly List<FiscalSubject> _subjects = [];

    public void Add(FiscalSubject subject)
    {
        _subjects.Add(subject);
    }

    public IReadOnlyCollection<FiscalSubject> GetAll()
    {
        return _subjects.AsReadOnly();
    }
}