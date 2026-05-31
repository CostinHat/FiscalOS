namespace FiscalOS.LegalKnowledge;

public sealed class LegalFragmentExtractor
{
    public IReadOnlyCollection<LegalFragment> Extract(LegalSource source)
    {
        var fragments = new List<LegalFragment>();

        var lines = source.Content.Split(Environment.NewLine);

        foreach (var line in lines)
        {
            if (!line.StartsWith("Art."))
            {
                continue;
            }

            fragments.Add(new LegalFragment
            {
                Id = Guid.NewGuid(),
                SourceId = source.Id,
                Article = line.Split('-')[0].Trim(),
                Text = line
            });
        }

        return fragments;
    }
}