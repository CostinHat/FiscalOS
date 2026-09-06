using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FiscalOS.LegalKnowledge.Corpus;

public enum LegalActType { Law, EmergencyOrdinance, Ordinance, GovernmentDecision, Order, Decree, Decision, Other }
public enum SourceClassification { OfficialPortal, OfficialOther, NonOfficial, Unknown }
public enum DocumentForm { Base, Amended, Republished, Consolidated, PreAmendment, Unknown }
public enum ImportDisposition { AcceptedRawCandidate, VersionVariant, SecondaryOnly, Quarantined }

public record LegalActIdentity(string Jurisdiction, LegalActType ActType, int Number, int Year, string? Issuer = null)
{
    public override string ToString() => $"{Jurisdiction}:{ActType}:{Number}/{Year}";
}

public sealed record RawCorpusArtifact(
    string RawArtifactId, string OriginalFileName, string ContentType, long ByteLength,
    string Sha256, SourceClassification SourceClassification, string? SourceLocator,
    string? PortalDocumentId, DateTimeOffset AcquiredAt, string? PublicationMetadata,
    LegalActIdentity? Act, DocumentForm Form, string PayloadPath);

public sealed record RawImportRecord(RawCorpusArtifact Artifact, ImportDisposition Disposition, string Reason);
public sealed record RawCorpusReadResult(RawCorpusArtifact Artifact, byte[] Payload, bool IsValid, string? IntegrityError);
public enum ManifestErrorCode { None, MissingFile, LengthMismatch, HashMismatch, InvalidManifestRow }
public sealed record ManifestValidationResult(string RelativePath, bool IsValid, string Reason, ManifestErrorCode ErrorCode = ManifestErrorCode.None);

public interface IRawLegalCorpusRepository
{
    Task<RawImportRecord> ImportAsync(RawImportRecord record, CancellationToken cancellationToken = default);
    Task<RawCorpusReadResult> ReadAsync(RawCorpusArtifact artifact, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RawCorpusArtifact>> GetArtifactsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RawImportRecord>> GetImportRecordsAsync(CancellationToken cancellationToken = default);
}

public static class SourceLocatorExtractor
{
    private static readonly Regex SavedUrl = new("saved from url=\\(\\d+\\)(?<url>https?://[^\\s'\"<>]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex PortalId = new("/DetaliiDocument(?:Afis)?/(?<id>\\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static (string? Locator, string? PortalDocumentId) Extract(ReadOnlySpan<byte> bytes)
    {
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        var locator = SavedUrl.Match(text).Groups["url"].Value;
        if (string.IsNullOrWhiteSpace(locator)) return (null, null);
        var id = PortalId.Match(locator).Groups["id"].Value;
        return (locator, string.IsNullOrWhiteSpace(id) ? null : id);
    }
}

public sealed class FileRawLegalCorpusRepository : IRawLegalCorpusRepository
{
    private readonly string metadataPath;
    private readonly string decisionsPath;
    private readonly string payloadDirectory;

    public FileRawLegalCorpusRepository(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        metadataPath = Path.Combine(root, "raw-metadata.jsonl");
        decisionsPath = Path.Combine(root, "raw-import-decisions.jsonl");
        payloadDirectory = Path.Combine(root, "raw-payloads");
        Directory.CreateDirectory(payloadDirectory);
    }

    public static string Hash(ReadOnlySpan<byte> payload) => Convert.ToHexString(SHA256.HashData(payload));

    public async Task<RawImportRecord> ImportAsync(RawImportRecord record, CancellationToken cancellationToken = default)
    {
        var artifact = record.Artifact;
        var payload = await File.ReadAllBytesAsync(artifact.PayloadPath, cancellationToken);
        var destination = Path.Combine(payloadDirectory, artifact.Sha256 + ".bin");
        if (!File.Exists(destination)) await File.WriteAllBytesAsync(destination, payload, cancellationToken);
        var persisted = artifact with { PayloadPath = destination };
        await File.AppendAllTextAsync(metadataPath, JsonSerializer.Serialize(persisted) + Environment.NewLine, cancellationToken);
        var decision = record with { Artifact = persisted };
        await File.AppendAllTextAsync(decisionsPath, JsonSerializer.Serialize(decision) + Environment.NewLine, cancellationToken);
        return decision;
    }

    public async Task<RawImportRecord> AcquireAsync(string fileName, string contentType, ReadOnlyMemory<byte> payload,
        SourceClassification source, string? locator, LegalActIdentity? act, DocumentForm form,
        string? publicationMetadata, DateTimeOffset acquiredAt, ImportDisposition disposition, string reason,
        CancellationToken cancellationToken = default)
    {
        var hash = Hash(payload.Span);
        var path = Path.Combine(payloadDirectory, hash + ".bin");
        if (!File.Exists(path)) await File.WriteAllBytesAsync(path, payload.ToArray(), cancellationToken);
        var artifact = new RawCorpusArtifact($"raw:{hash}", fileName, contentType, payload.Length, hash, source,
            locator, SourceLocatorExtractor.Extract(payload.Span).PortalDocumentId, acquiredAt,
            publicationMetadata, act, form, path);
        return await ImportAsync(new RawImportRecord(artifact, disposition, reason), cancellationToken);
    }

    public async Task<RawCorpusReadResult> ReadAsync(RawCorpusArtifact artifact, CancellationToken cancellationToken = default)
    {
        var payload = await File.ReadAllBytesAsync(artifact.PayloadPath, cancellationToken);
        var actual = Hash(payload);
        return actual.Equals(artifact.Sha256, StringComparison.OrdinalIgnoreCase)
            ? new(artifact, payload, true, null)
            : new(artifact, payload, false, $"SHA-256 mismatch: expected {artifact.Sha256}, actual {actual}.");
    }

    public async Task<IReadOnlyList<RawCorpusArtifact>> GetArtifactsAsync(CancellationToken cancellationToken = default) =>
        await ReadJsonLines<RawCorpusArtifact>(metadataPath, cancellationToken);

    public async Task<IReadOnlyList<RawImportRecord>> GetImportRecordsAsync(CancellationToken cancellationToken = default) =>
        await ReadJsonLines<RawImportRecord>(decisionsPath, cancellationToken);

    private static async Task<IReadOnlyList<T>> ReadJsonLines<T>(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path)) return Array.Empty<T>();
        var values = new List<T>();
        foreach (var line in await File.ReadAllLinesAsync(path, cancellationToken))
            if (!string.IsNullOrWhiteSpace(line)) values.Add(JsonSerializer.Deserialize<T>(line)!);
        return values;
    }
}

public static class RawCorpusManifestValidator
{
    public static IReadOnlyList<ManifestValidationResult> Validate(string root, string manifestPath)
    {
        var results = new List<ManifestValidationResult>();
        foreach (var line in File.ReadLines(manifestPath).Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            var match = Regex.Match(line, "^\"(?<path>.*)\",\"(?<length>\\d+)\",\"(?<hash>[A-Fa-f0-9]{64})\"$");
            if (!match.Success) { results.Add(new(line, false, "Invalid manifest row.", ManifestErrorCode.InvalidManifestRow)); continue; }
            var relative = match.Groups["path"].Value;
            var path = Path.Combine(root, relative);
            if (!File.Exists(path)) { results.Add(new(relative, false, "File does not exist.", ManifestErrorCode.MissingFile)); continue; }
            var bytes = File.ReadAllBytes(path);
            var actual = FileRawLegalCorpusRepository.Hash(bytes);
            var lengthOk = bytes.LongLength == long.Parse(match.Groups["length"].Value);
            var hashOk = actual.Equals(match.Groups["hash"].Value, StringComparison.OrdinalIgnoreCase);
            var valid = lengthOk && hashOk;
            var code = valid ? ManifestErrorCode.None : !lengthOk ? ManifestErrorCode.LengthMismatch : ManifestErrorCode.HashMismatch;
            results.Add(new(relative, valid, valid ? "Manifest matches." : code.ToString(), code));
        }
        return results;
    }
}
