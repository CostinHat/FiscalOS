using System.Text.Json;
using FiscalOS.LegalKnowledge.Corpus;

if (args.Length != 3)
    throw new ArgumentException("Expected: <raw-repository-root> <fixture-root> <output-root>.");

var repository = new FileRawLegalCorpusRepository(args[0]);
var fixtureRoot = args[1];
var outputRoot = args[2];
foreach (var line in File.ReadLines(Path.Combine(fixtureRoot, "_legislation-manifest.csv")).Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)))
{
    var values = line.Trim().Trim('"').Split("\",\"");
    var relativePath = values[0].Replace('\\', Path.DirectorySeparatorChar);
    var path = Path.Combine(fixtureRoot, relativePath);
    var payload = await File.ReadAllBytesAsync(path);
    if (payload.LongLength != long.Parse(values[1]) ||
        !FileRawLegalCorpusRepository.Hash(payload).Equals(values[2], StringComparison.OrdinalIgnoreCase))
        throw new InvalidDataException($"Manifest mismatch: {relativePath}");
    await repository.AcquireAsync(
        Path.GetFileName(path),
        path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? "application/pdf" : "text/html",
        payload,
        SourceClassification.Unknown,
        null,
        null,
        DocumentForm.Unknown,
        null,
        DateTimeOffset.UnixEpoch,
        ImportDisposition.AcceptedRawCandidate,
        "corpus batch atomization");
}

var atoms = new FileStructuralAtomRepository(outputRoot);
var first = await LegalCorpusBatchAtomizer.AtomizeAndSaveAsync(repository, atoms);
var replay = await LegalCorpusBatchAtomizer.AtomizeAndSaveAsync(repository, atoms);
var summary = new
{
    SourceCommit = Environment.GetEnvironmentVariable("GITHUB_SHA"),
    NormalizerVersion = LegalCorpusNormalizer.Version,
    AtomizerVersion = LegalStructuralAtomizer.Version,
    Documents = first.Atomization.TotalCount,
    AtomizedDocuments = first.Atomization.AtomizedCount,
    FailedDocuments = first.Atomization.FailedCount,
    Atoms = first.Atomization.TotalAtomCount,
    Articles = first.Atomization.TotalArticleCount,
    SavedDocuments = first.Persistence.SavedDocumentCount,
    SavedAtoms = first.Persistence.SavedAtomCount,
    ReplaySkippedDocuments = replay.Persistence.SkippedDocumentCount
};
Directory.CreateDirectory(outputRoot);
await File.WriteAllTextAsync(
    Path.Combine(outputRoot, "run-summary.json"),
    JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
Console.WriteLine(JsonSerializer.Serialize(summary));
