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

    [Fact]
    public async Task Atomized_documents_are_saved_once_and_replays_are_skipped()
    {
        var rawRoot = Path.Combine(Path.GetTempPath(), "fiscalos-raw-" + Guid.NewGuid());
        var atomRoot = Path.Combine(Path.GetTempPath(), "fiscalos-atoms-" + Guid.NewGuid());
        try
        {
            var raw = System.Text.Encoding.UTF8.GetBytes("<html><body><p>Articolul 1</p><p>(1) Regula.</p></body></html>");
            var corpus = new FileRawLegalCorpusRepository(rawRoot);
            await corpus.AcquireAsync(
                "act.html", "text/html", raw, SourceClassification.OfficialPortal, null, null,
                DocumentForm.Unknown, null, DateTimeOffset.UnixEpoch,
                ImportDisposition.AcceptedRawCandidate, "test");
            var atoms = new FileStructuralAtomRepository(atomRoot);

            var first = await LegalCorpusBatchAtomizer.AtomizeAndSaveAsync(corpus, atoms);
            var replay = await LegalCorpusBatchAtomizer.AtomizeAndSaveAsync(corpus, atoms);

            Assert.Equal(1, first.Persistence.SavedDocumentCount);
            Assert.Equal(first.Atomization.TotalAtomCount, first.Persistence.SavedAtomCount);
            Assert.Equal(0, first.Persistence.SkippedDocumentCount);
            Assert.Equal(0, replay.Persistence.SavedDocumentCount);
            Assert.Equal(0, replay.Persistence.SavedAtomCount);
            Assert.Equal(1, replay.Persistence.SkippedDocumentCount);
            Assert.Single(Directory.GetFiles(Path.Combine(atomRoot, "structural-documents"), "*.json"));
        }
        finally
        {
            if (Directory.Exists(rawRoot)) Directory.Delete(rawRoot, true);
            if (Directory.Exists(atomRoot)) Directory.Delete(atomRoot, true);
        }
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
