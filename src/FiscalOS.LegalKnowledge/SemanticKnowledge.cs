using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using FiscalOS.LegalKnowledge.Corpus;

namespace FiscalOS.LegalKnowledge;

public enum SemanticKnowledgeKind { LegalDefinition }
public enum SemanticCandidateError { None, UnsupportedSemanticKind, MissingStructuralGrounding, UnknownGroundingAtomId, InvalidDefinedTerm, InvalidDefinitionText }
public enum SemanticCurationDisposition { Accepted, Rejected, NeedsReview }
public enum SemanticCurationError { None, UnknownSemanticCandidate, CandidateConceptMismatch, InvalidSemanticCurator, InvalidSemanticReason, InvalidKnowledgeTime, UnsupportedSemanticDisposition, UnknownPredecessor, SelfSupersession, SupersessionCycle, SemanticConceptMismatchAcrossSupersession, MissingLegalValidFrom, InvalidLegalValidRange }
public sealed record LegalValidTime(DateOnly ValidFrom, DateOnly? ValidTo = null)
{
    public static (LegalValidTime? Value, SemanticCurationError Error) Create(DateOnly? from, DateOnly? to)
        => from is null ? (null, SemanticCurationError.MissingLegalValidFrom) : to < from ? (null, SemanticCurationError.InvalidLegalValidRange) : (new LegalValidTime(from.Value, to), SemanticCurationError.None);
}
public sealed record StableLegalScope(string CanonicalActIdentity, string StructuralPath);

public sealed class StructuralGrounding : IEquatable<StructuralGrounding>
{
    public string RawArtifactId { get; }
    public string NormalizationVersion { get; }
    public string AtomizerVersion { get; }
    public IReadOnlyList<string> StructuralAtomIds { get; }
    public StructuralGrounding(string rawArtifactId, IEnumerable<string> atomIds, string normalizationVersion = "", string atomizerVersion = "")
    {
        RawArtifactId = rawArtifactId;
        NormalizationVersion = normalizationVersion;
        AtomizerVersion = atomizerVersion;
        StructuralAtomIds = new ReadOnlyCollection<string>(atomIds.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray());
    }
    public bool Equals(StructuralGrounding? other) => other is not null && RawArtifactId == other.RawArtifactId && StructuralAtomIds.SequenceEqual(other.StructuralAtomIds);
    public override bool Equals(object? obj) => Equals(obj as StructuralGrounding);
    public override int GetHashCode() { var h = new HashCode(); h.Add(RawArtifactId); foreach (var id in StructuralAtomIds) h.Add(id, StringComparer.Ordinal); return h.ToHashCode(); }
}

public sealed class SemanticKnowledgeCandidate : IEquatable<SemanticKnowledgeCandidate>
{
    private const string SchemaVersion = "semantic-candidate-v1";
    public SemanticKnowledgeKind Kind { get; }
    public string SemanticConceptId { get; }
    public string SemanticCandidateId { get; }
    public string DefinedTerm { get; }
    public string DefinitionText { get; }
    public StableLegalScope LegalScope { get; }
    public StructuralGrounding Grounding { get; }
    private SemanticKnowledgeCandidate(SemanticKnowledgeKind kind, string conceptId, string candidateId, string term, string text, StableLegalScope scope, StructuralGrounding grounding) { Kind = kind; SemanticConceptId = conceptId; SemanticCandidateId = candidateId; DefinedTerm = term; DefinitionText = text; LegalScope = scope; Grounding = grounding; }
    internal static SemanticKnowledgeCandidate Rehydrate(SemanticKnowledgeKind kind, string conceptId, string candidateId, string term, string text, StableLegalScope scope, StructuralGrounding grounding) => new(kind, conceptId, candidateId, term, text, scope, grounding);
    public static (SemanticKnowledgeCandidate? Candidate, SemanticCandidateError Error) Create(SemanticKnowledgeKind kind, string term, string text, CuratedStructuralMaterial material, IEnumerable<string> atomIds, StableLegalScope legalScope)
    {
        if (kind != SemanticKnowledgeKind.LegalDefinition) return (null, SemanticCandidateError.UnsupportedSemanticKind);
        if (string.IsNullOrWhiteSpace(term)) return (null, SemanticCandidateError.InvalidDefinedTerm);
        if (string.IsNullOrWhiteSpace(text)) return (null, SemanticCandidateError.InvalidDefinitionText);
        var ids = atomIds?.ToArray() ?? Array.Empty<string>();
        if (ids.Length == 0) return (null, SemanticCandidateError.MissingStructuralGrounding);
        var available = material.Decision.StructuralAtomIds.ToHashSet(StringComparer.Ordinal);
        if (ids.Any(id => !available.Contains(id))) return (null, SemanticCandidateError.UnknownGroundingAtomId);
        var normalizedTerm = term.Trim(); var normalizedText = text.Trim();
        var grounding = new StructuralGrounding(material.Decision.RawArtifactId, ids, material.Decision.NormalizationVersion, material.Decision.AtomizerVersion);
        var conceptId = "concept:" + Hash(Canonical(kind, legalScope.CanonicalActIdentity.Trim(), legalScope.StructuralPath.Trim(), normalizedTerm));
        var candidateId = "candidate:" + Hash(Canonical(conceptId, kind, normalizedTerm, normalizedText, grounding.RawArtifactId, string.Join(",", grounding.StructuralAtomIds), material.Decision.NormalizationVersion, material.Decision.AtomizerVersion, SchemaVersion));
        return (new SemanticKnowledgeCandidate(kind, conceptId, candidateId, normalizedTerm, normalizedText, legalScope, grounding), SemanticCandidateError.None);
    }
    private static string Canonical(params object[] values) => string.Join("\u001F", values.Select(v => v.ToString() ?? ""));
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    internal bool HasValidIdentity(string normalizationVersion = "norm-v1", string atomizerVersion = "atom-v1")
    {
        var concept = "concept:" + Hash(Canonical(Kind, LegalScope.CanonicalActIdentity.Trim(), LegalScope.StructuralPath.Trim(), DefinedTerm));
        var candidate = "candidate:" + Hash(Canonical(concept, Kind, DefinedTerm, DefinitionText, Grounding.RawArtifactId, string.Join(",", Grounding.StructuralAtomIds), normalizationVersion, atomizerVersion, SchemaVersion));
        return concept == SemanticConceptId && candidate == SemanticCandidateId;
    }
    public bool Equals(SemanticKnowledgeCandidate? other) => other is not null && SemanticCandidateId == other.SemanticCandidateId && SemanticConceptId == other.SemanticConceptId && Kind == other.Kind && DefinedTerm == other.DefinedTerm && DefinitionText == other.DefinitionText && EqualityComparer<StableLegalScope>.Default.Equals(LegalScope, other.LegalScope) && Grounding.Equals(other.Grounding);
    public override bool Equals(object? obj) => Equals(obj as SemanticKnowledgeCandidate);
    public override int GetHashCode() => HashCode.Combine(SemanticConceptId, SemanticCandidateId, Kind, DefinedTerm, DefinitionText, LegalScope, Grounding);
}

public sealed record SemanticCurationDecision(string SemanticCurationDecisionId, string SemanticCandidateId, string SemanticConceptId, SemanticCurationDisposition Disposition, string Reason, string Curator, DateTimeOffset KnowledgeTime, string CurationVersion, string? SupersedesSemanticDecisionId = null, LegalValidTime? LegalValidTime = null);
public sealed record SemanticCurationResult(SemanticCurationDecision? Decision, SemanticCurationError Error = SemanticCurationError.None);
public sealed record AcceptedSemanticKnowledge(SemanticCurationDecision Decision, SemanticKnowledgeCandidate Candidate)
{
    public SemanticConceptId ConceptId => new(SemanticConceptIdValue);
    private string SemanticConceptIdValue => Candidate.SemanticConceptId;
}
public readonly record struct SemanticConceptId(string Value);

public static class SemanticCuration
{
    public const string Version = "semantic-curation-v1";
    public static SemanticCurationResult Decide(SemanticKnowledgeCandidate? candidate, SemanticKnowledgeCandidate? suppliedCandidate, SemanticCurationDisposition disposition, string reason, string curator, DateTimeOffset knowledgeTime, LegalValidTime? legalValidTime = null)
    {
        if (candidate is null || suppliedCandidate is null) return new(null, SemanticCurationError.UnknownSemanticCandidate);
        if (candidate.SemanticCandidateId != suppliedCandidate.SemanticCandidateId) return new(null, SemanticCurationError.UnknownSemanticCandidate);
        if (candidate.SemanticConceptId != suppliedCandidate.SemanticConceptId) return new(null, SemanticCurationError.CandidateConceptMismatch);
        if (string.IsNullOrWhiteSpace(curator)) return new(null, SemanticCurationError.InvalidSemanticCurator);
        if (string.IsNullOrWhiteSpace(reason)) return new(null, SemanticCurationError.InvalidSemanticReason);
        if (knowledgeTime == default) return new(null, SemanticCurationError.InvalidKnowledgeTime);
        var temporal = legalValidTime is null ? null : $"legal-valid:{legalValidTime.ValidFrom:yyyy-MM-dd}:{(legalValidTime.ValidTo.HasValue ? legalValidTime.ValidTo.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : "none")}"; var seed = temporal is null ? string.Join("\u001F", candidate.SemanticCandidateId, candidate.SemanticConceptId, disposition, reason.Trim(), curator.Trim(), knowledgeTime.ToUniversalTime().ToString("O"), Version) : string.Join("\u001F", candidate.SemanticCandidateId, candidate.SemanticConceptId, disposition, reason.Trim(), curator.Trim(), knowledgeTime.ToUniversalTime().ToString("O"), Version, temporal);
        var id = "semantic-curation:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(seed))).ToLowerInvariant();
        return new(new SemanticCurationDecision(id, candidate.SemanticCandidateId, candidate.SemanticConceptId, disposition, reason.Trim(), curator.Trim(), knowledgeTime, Version, null, legalValidTime));
    }
    public static AcceptedSemanticKnowledge? Promote(SemanticKnowledgeCandidate candidate, SemanticCurationDecision decision)
        => decision.Disposition == SemanticCurationDisposition.Accepted && decision.SemanticCandidateId == candidate.SemanticCandidateId ? new(decision, candidate) : null;
}
