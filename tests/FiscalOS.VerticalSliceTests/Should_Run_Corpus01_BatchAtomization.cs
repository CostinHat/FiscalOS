using FiscalOS.LegalKnowledge.Corpus;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Corpus01_BatchAtomization
{
    [Fact]
    public async Task All_52_exercise_documents_are_accounted_for_deterministically()
    {
        var fixtureRoot = Path.Combine(AppContext.BaseDirectory, "Fixtures", "corpus", "imported-legislation");
        var repositoryRoot = Path.Combine(Path.GetTempPath(), "fiscalos-batch-" + Guid.NewGuid());
        var repository = new FileRawLegalCorpusRepository(repositoryRoot);

        foreach (var row in ReadManifest(Path.Combine(fixtureRoot, "_legislation-manifest.csv")))
        {
            var relativePath = row.RelativePath.Replace('\\', Path.DirectorySeparatorChar);
            var path = Path.Combine(fixtureRoot, relativePath);
            var payload = await File.ReadAllBytesAsync(path);
            Assert.Equal(row.Length, payload.LongLength);
            Assert.Equal(row.Sha256, FileRawLegalCorpusRepository.Hash(payload), ignoreCase: true);
            var contentType = path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ? "application/pdf"
                : "text/html";
            await repository.AcquireAsync(
                Path.GetFileName(path), contentType, payload, SourceClassification.Unknown, null, null,
                DocumentForm.Unknown, null, DateTimeOffset.UnixEpoch,
                ImportDisposition.AcceptedRawCandidate, "corpus exercise");
        }

        var first = await LegalCorpusBatchAtomizer.AtomizeAsync(repository);
        var second = await LegalCorpusBatchAtomizer.AtomizeAsync(repository);

        Assert.Equal(52, first.TotalCount);
        Assert.Equal(52, first.AtomizedCount);
        Assert.Equal(0, first.FailedCount);
        Assert.All(first.Items.Where(i => i.Status == CorpusAtomizationStatus.Atomized), i =>
        {
            Assert.True(i.ArticleCount > 0);
            Assert.True(i.AtomCount > i.ArticleCount);
        });
        Assert.Equal(
            first.Items.Select(Signature),
            second.Items.Select(Signature));
    }

    private static string Signature(CorpusAtomizationItem item) =>
        $"{item.Artifact.RawArtifactId}|{item.Status}|{item.AtomCount}|{item.ArticleCount}";

    private static IEnumerable<(string RelativePath, long Length, string Sha256)> ReadManifest(string path)
    {
        foreach (var line in File.ReadLines(path).Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            var values = line.Trim().Trim('"').Split("\",\"");
            yield return (values[0], long.Parse(values[1]), values[2]);
        }
    }
}
