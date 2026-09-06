using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FiscalOS.LegalKnowledge.Corpus;

public enum CurationDisposition { Accepted, Rejected, NeedsReview, SecondaryOnly }
public enum CurationError { None, InvalidInput, UnknownStructuralAtomId, DuplicateStructuralAtomId, EmptyAtomScope, RawArtifactMismatch, NormalizationVersionMismatch, AtomizerVersionMismatch, UnknownPredecessor, SelfSupersession, SupersessionCycle, InvalidCurator, InvalidReason, UnknownAtom = UnknownStructuralAtomId, DuplicateAtom = DuplicateStructuralAtomId, EmptyScope = EmptyAtomScope, ArtifactMismatch = RawArtifactMismatch, VersionMismatch = NormalizationVersionMismatch }
public sealed class CurationDecision : IEquatable<CurationDecision>
{
    public string CurationDecisionId { get; init; } = ""; public string RawArtifactId { get; init; } = ""; public string AtomizerVersion { get; init; } = ""; public string NormalizationVersion { get; init; } = ""; public CurationDisposition Disposition { get; init; } public string Reason { get; init; } = ""; public string[] StructuralAtomIds { get; init; } = Array.Empty<string>(); public bool DocumentScope { get; init; } public DateTimeOffset KnowledgeTime { get; init; } public string Curator { get; init; } = ""; public string CurationVersion { get; init; } = ""; public string? SupersedesDecisionId { get; init; }
    public CurationDecision() { }
    public CurationDecision(string id, string raw, string atomizer, string normalization, CurationDisposition disposition, string reason, string[] atoms, bool documentScope, DateTimeOffset time, string curator, string version, string? supersedes = null) => (CurationDecisionId, RawArtifactId, AtomizerVersion, NormalizationVersion, Disposition, Reason, StructuralAtomIds, DocumentScope, KnowledgeTime, Curator, CurationVersion, SupersedesDecisionId) = (id, raw, atomizer, normalization, disposition, reason, atoms.ToArray(), documentScope, time, curator, version, supersedes);
    public bool Equals(CurationDecision? other) => other is not null && CurationDecisionId == other.CurationDecisionId && RawArtifactId == other.RawArtifactId && AtomizerVersion == other.AtomizerVersion && NormalizationVersion == other.NormalizationVersion && Disposition == other.Disposition && Reason == other.Reason && DocumentScope == other.DocumentScope && KnowledgeTime == other.KnowledgeTime && Curator == other.Curator && CurationVersion == other.CurationVersion && SupersedesDecisionId == other.SupersedesDecisionId && StructuralAtomIds.SequenceEqual(other.StructuralAtomIds);
    public override bool Equals(object? obj) => Equals(obj as CurationDecision);
    public override int GetHashCode() { var hash = new HashCode(); hash.Add(CurationDecisionId); hash.Add(RawArtifactId); hash.Add(AtomizerVersion); hash.Add(NormalizationVersion); hash.Add(Disposition); hash.Add(Reason); hash.Add(DocumentScope); hash.Add(KnowledgeTime); hash.Add(Curator); hash.Add(CurationVersion); hash.Add(SupersedesDecisionId); foreach (var atom in StructuralAtomIds) hash.Add(atom); return hash.ToHashCode(); }
}
public sealed record CuratedStructuralMaterial(CurationDecision Decision, StructuralLegalDocument StructuralDocument);
public sealed record CurationResult(CuratedStructuralMaterial? Material, CurationError Error = CurationError.None, string? ErrorMessage = null);
public sealed record CurationSaveResult(CurationDecision? Decision, CurationError Error = CurationError.None, string? ErrorMessage = null);

public static class StructuralCuration
{
    public const string Version = "corpus-curation-v1";

    public static CurationResult Decide(StructuralLegalDocument document, string rawArtifactId, string normalizationVersion, CurationDisposition disposition, string reason, IEnumerable<string>? atomIds, bool documentScope, DateTimeOffset knowledgeTime, string curator, string? supersedes = null)
    {
        if (string.IsNullOrWhiteSpace(rawArtifactId)) return Fail(CurationError.RawArtifactMismatch, "Raw artifact is required.");
        if (string.IsNullOrWhiteSpace(reason)) return Fail(CurationError.InvalidReason, "Reason is required.");
        if (string.IsNullOrWhiteSpace(curator)) return Fail(CurationError.InvalidCurator, "Curator is required.");
        var requested = (atomIds ?? Array.Empty<string>()).ToArray();
        if (!documentScope && requested.Length == 0) return Fail(CurationError.EmptyAtomScope, "Atom scope cannot be empty.");
        if (requested.Distinct().Count() != requested.Length) return Fail(CurationError.DuplicateStructuralAtomId, "Duplicate atom in scope.");
        var available = document.Atoms.Select(a => a.StructuralAtomId).ToHashSet();
        if (requested.Any(id => !available.Contains(id))) return Fail(CurationError.UnknownStructuralAtomId, "Unknown structural atom.");
        if (document.RawArtifactId is not null && document.RawArtifactId != rawArtifactId) return Fail(CurationError.RawArtifactMismatch, "Structural document lineage mismatch.");
        if (document.NormalizationVersion is not null && document.NormalizationVersion != normalizationVersion) return Fail(CurationError.NormalizationVersionMismatch, "Normalization version mismatch.");
        if (document.AtomizerVersion != document.Atoms.FirstOrDefault()?.AtomizerVersion) return Fail(CurationError.AtomizerVersionMismatch, "Atomizer version mismatch.");
        var selected = documentScope ? document.Atoms.Select(a => a.StructuralAtomId).ToArray() : requested;
        var seed = string.Join("|", rawArtifactId, document.AtomizerVersion, normalizationVersion, disposition, reason, string.Join(",", selected), documentScope, knowledgeTime.ToUniversalTime().ToString("O"), curator, Version, supersedes);
        var id = "curation:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(seed))).ToLowerInvariant();
        var decision = new CurationDecision(id, rawArtifactId, document.AtomizerVersion, normalizationVersion, disposition, reason, selected, documentScope, knowledgeTime, curator, Version, supersedes);
        return disposition == CurationDisposition.Accepted ? new(new CuratedStructuralMaterial(decision, document)) : new(null, CurationError.None, "Decision does not promote material.");
    }

    private static CurationResult Fail(CurationError error, string message) => new(null, error, message);
}

public interface IStructuralCurationRepository
{
    Task<CurationSaveResult> SaveAsync(CurationDecision decision, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CurationDecision>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class FileStructuralCurationRepository : IStructuralCurationRepository
{
    private readonly string path;
    public FileStructuralCurationRepository(string root) { Directory.CreateDirectory(root); path = Path.Combine(root, "curation-decisions.jsonl"); }
    public async Task<CurationSaveResult> SaveAsync(CurationDecision decision, CancellationToken cancellationToken = default)
    {
        var existing = await GetAllAsync(cancellationToken);
        if (decision.SupersedesDecisionId == decision.CurationDecisionId) return new(null, CurationError.SelfSupersession, "A decision cannot supersede itself.");
        if (decision.SupersedesDecisionId is not null && existing.All(x => x.CurationDecisionId != decision.SupersedesDecisionId)) return new(null, CurationError.UnknownPredecessor, "Unknown predecessor.");
        if (decision.SupersedesDecisionId is not null && existing.Any(x => x.SupersedesDecisionId == decision.CurationDecisionId)) return new(null, CurationError.SupersessionCycle, "Supersession cycle.");
        var same = existing.FirstOrDefault(x => x.CurationDecisionId == decision.CurationDecisionId);
        if (same is not null) { if (!Equals(same, decision)) throw new InvalidDataException("Conflicting curation decision identity."); return new(same); }
        await File.AppendAllTextAsync(path, JsonSerializer.Serialize(decision) + Environment.NewLine, cancellationToken); return new(decision);
    }

    public async Task<CurationSaveResult> TrySaveAsync(CurationDecision decision, CancellationToken cancellationToken = default)
    {
        var existing = await GetAllAsync(cancellationToken);
        if (decision.SupersedesDecisionId == decision.CurationDecisionId) return new(null, CurationError.SelfSupersession, "A decision cannot supersede itself.");
        if (decision.SupersedesDecisionId is not null && existing.All(x => x.CurationDecisionId != decision.SupersedesDecisionId)) return new(null, CurationError.UnknownPredecessor, "Unknown predecessor.");
        if (decision.SupersedesDecisionId is not null && existing.Any(x => x.SupersedesDecisionId == decision.CurationDecisionId)) return new(null, CurationError.SupersessionCycle, "Supersession cycle.");
        return await SaveAsync(decision, cancellationToken);
    }
    public async Task<IReadOnlyList<CurationDecision>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path)) return Array.Empty<CurationDecision>();
        var list = new List<CurationDecision>(); foreach (var line in await File.ReadAllLinesAsync(path, cancellationToken)) if (!string.IsNullOrWhiteSpace(line)) list.Add(JsonSerializer.Deserialize<CurationDecision>(line)!); return list;
    }
}
