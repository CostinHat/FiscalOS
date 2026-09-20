using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FiscalOS.LegalKnowledge.Corpus;

public sealed record StructuralAtomSaveResult(int SavedDocumentCount, int SkippedDocumentCount, int SavedAtomCount);

public interface IStructuralAtomRepository
{
    Task<StructuralAtomSaveResult> SaveAsync(
        IEnumerable<StructuralLegalDocument> documents,
        CancellationToken cancellationToken = default);
}

public sealed class FileStructuralAtomRepository : IStructuralAtomRepository
{
    private readonly string documentsDirectory;

    public FileStructuralAtomRepository(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        documentsDirectory = Path.Combine(root, "structural-documents");
        Directory.CreateDirectory(documentsDirectory);
    }

    public async Task<StructuralAtomSaveResult> SaveAsync(
        IEnumerable<StructuralLegalDocument> documents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);
        var savedDocuments = 0;
        var skippedDocuments = 0;
        var savedAtoms = 0;

        foreach (var document in documents.OrderBy(DocumentKey, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Validate(document);
            var key = DocumentKey(document);
            var path = Path.Combine(documentsDirectory, FileName(key));
            if (File.Exists(path))
            {
                var existing = JsonSerializer.Deserialize<PersistedStructuralDocument>(
                    await File.ReadAllTextAsync(path, cancellationToken))
                    ?? throw new InvalidDataException($"Corrupt structural document: {path}.");
                if (!string.Equals(existing.DocumentKey, key, StringComparison.Ordinal))
                    throw new InvalidDataException($"Structural document identity mismatch: {path}.");
                skippedDocuments++;
                continue;
            }

            var spans = document.Atoms.SelectMany(atom => atom.SourceSpans).Distinct().ToArray();
            var persisted = new PersistedStructuralDocument(
                key,
                document.RawArtifactId!,
                document.NormalizationVersion!,
                document.AtomizerVersion,
                document.Error,
                document.ErrorMessage,
                spans,
                document.Atoms
                    .OrderBy(atom => atom.Ordinal)
                    .ThenBy(atom => atom.StructuralAtomId, StringComparer.Ordinal)
                    .Select(atom => new PersistedStructuralAtom(
                        atom.StructuralAtomId,
                        atom.AtomType,
                        atom.Designation,
                        atom.ParentAtomId,
                        atom.Ordinal,
                        atom.NormalizedStart,
                        atom.NormalizedLength,
                        atom.AtomizerVersion))
                    .ToArray());
            var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            await File.WriteAllTextAsync(temporaryPath, JsonSerializer.Serialize(persisted), cancellationToken);
            File.Move(temporaryPath, path, false);
            savedDocuments++;
            savedAtoms += persisted.Atoms.Count;
        }

        return new(savedDocuments, skippedDocuments, savedAtoms);
    }

    private static string DocumentKey(StructuralLegalDocument document) =>
        $"{document.RawArtifactId}|{document.NormalizationVersion}|{document.AtomizerVersion}";

    private static string FileName(string key) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant() + ".json";

    private static void Validate(StructuralLegalDocument document)
    {
        if (string.IsNullOrWhiteSpace(document.RawArtifactId) || string.IsNullOrWhiteSpace(document.NormalizationVersion))
            throw new InvalidDataException("A structural document must retain raw and normalization identities.");
        if (document.Atoms.Any(atom => !string.Equals(atom.AtomizerVersion, document.AtomizerVersion, StringComparison.Ordinal)))
            throw new InvalidDataException("Structural atomizer version mismatch.");
        if (document.Atoms.Select(atom => atom.StructuralAtomId).Distinct(StringComparer.Ordinal).Count() != document.Atoms.Count)
            throw new InvalidDataException("Duplicate structural atom identity within a document.");
    }

    private sealed record PersistedStructuralDocument(
        string DocumentKey,
        string RawArtifactId,
        string NormalizationVersion,
        string AtomizerVersion,
        AtomizationError Error,
        string? ErrorMessage,
        IReadOnlyList<NormalizedSourceSpan> SourceSpans,
        IReadOnlyList<PersistedStructuralAtom> Atoms);

    private sealed record PersistedStructuralAtom(
        string StructuralAtomId,
        StructuralAtomType AtomType,
        string Designation,
        string? ParentAtomId,
        int Ordinal,
        int NormalizedStart,
        int NormalizedLength,
        string AtomizerVersion);
}
