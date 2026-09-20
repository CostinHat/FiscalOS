namespace FiscalOS.LegalKnowledge.Corpus;

public enum CorpusAtomizationStatus
{
    Atomized,
    IntegrityFailed,
    NormalizationFailed,
    AtomizationFailed
}

public sealed record CorpusAtomizationItem(
    RawCorpusArtifact Artifact,
    CorpusAtomizationStatus Status,
    NormalizedLegalMaterial? NormalizedMaterial,
    StructuralLegalDocument? StructuralDocument,
    string? ErrorMessage)
{
    public int AtomCount => StructuralDocument?.Atoms.Count ?? 0;
    public int ArticleCount => StructuralDocument?.Atoms.Count(a => a.AtomType == StructuralAtomType.Article) ?? 0;
}

public sealed record CorpusAtomizationBatchResult(IReadOnlyList<CorpusAtomizationItem> Items)
{
    public int TotalCount => Items.Count;
    public int AtomizedCount => Items.Count(i => i.Status == CorpusAtomizationStatus.Atomized);
    public int FailedCount => TotalCount - AtomizedCount;
    public int TotalAtomCount => Items.Sum(i => i.AtomCount);
    public int TotalArticleCount => Items.Sum(i => i.ArticleCount);
}

public static class LegalCorpusBatchAtomizer
{
    public static async Task<CorpusAtomizationBatchResult> AtomizeAsync(
        IRawLegalCorpusRepository repository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var artifacts = await repository.GetArtifactsAsync(cancellationToken);
        var items = new List<CorpusAtomizationItem>(artifacts.Count);
        foreach (var artifact in artifacts
                     .OrderBy(a => a.RawArtifactId, StringComparer.Ordinal)
                     .ThenBy(a => a.OriginalFileName, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            RawCorpusReadResult raw;
            try
            {
                raw = await repository.ReadAsync(artifact, cancellationToken);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                items.Add(new(artifact, CorpusAtomizationStatus.IntegrityFailed, null, null, exception.Message));
                continue;
            }

            if (!raw.IsValid)
            {
                items.Add(new(artifact, CorpusAtomizationStatus.IntegrityFailed, null, null, raw.IntegrityError));
                continue;
            }

            var normalized = LegalCorpusNormalizer.Normalize(artifact, raw.Payload);
            if (normalized.Error != NormalizationError.None)
            {
                items.Add(new(artifact, CorpusAtomizationStatus.NormalizationFailed, normalized, null, normalized.ErrorMessage));
                continue;
            }

            var structural = LegalStructuralAtomizer.Atomize(normalized);
            var status = structural.Error == AtomizationError.None
                ? CorpusAtomizationStatus.Atomized
                : CorpusAtomizationStatus.AtomizationFailed;
            items.Add(new(artifact, status, normalized, structural, structural.ErrorMessage));
        }

        return new(items);
    }
}
