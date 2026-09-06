using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using FiscalOS.LegalKnowledge.Corpus;

namespace FiscalOS.LegalKnowledge;

public enum SemanticKnowledgeKind { LegalDefinition }
public enum SemanticCandidateError { None, UnsupportedSemanticKind, MissingStructuralGrounding, UnknownGroundingAtomId, InvalidDefinedTerm, InvalidDefinitionText }
public sealed record StableLegalScope(string CanonicalActIdentity, string StructuralPath);

public sealed class StructuralGrounding : IEquatable<StructuralGrounding>
{
    public string RawArtifactId { get; }
    public IReadOnlyList<string> StructuralAtomIds { get; }
    public StructuralGrounding(string rawArtifactId, IEnumerable<string> atomIds)
    {
        RawArtifactId = rawArtifactId;
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
        var grounding = new StructuralGrounding(material.Decision.RawArtifactId, ids);
        var conceptId = "concept:" + Hash(Canonical(kind, legalScope.CanonicalActIdentity.Trim(), legalScope.StructuralPath.Trim(), normalizedTerm));
        var candidateId = "candidate:" + Hash(Canonical(conceptId, kind, normalizedTerm, normalizedText, grounding.RawArtifactId, string.Join(",", grounding.StructuralAtomIds), material.Decision.NormalizationVersion, material.Decision.AtomizerVersion, SchemaVersion));
        return (new SemanticKnowledgeCandidate(kind, conceptId, candidateId, normalizedTerm, normalizedText, legalScope, grounding), SemanticCandidateError.None);
    }
    private static string Canonical(params object[] values) => string.Join("\u001F", values.Select(v => v.ToString() ?? ""));
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    public bool Equals(SemanticKnowledgeCandidate? other) => other is not null && SemanticCandidateId == other.SemanticCandidateId && SemanticConceptId == other.SemanticConceptId && Kind == other.Kind && DefinedTerm == other.DefinedTerm && DefinitionText == other.DefinitionText && EqualityComparer<StableLegalScope>.Default.Equals(LegalScope, other.LegalScope) && Grounding.Equals(other.Grounding);
    public override bool Equals(object? obj) => Equals(obj as SemanticKnowledgeCandidate);
    public override int GetHashCode() => HashCode.Combine(SemanticConceptId, SemanticCandidateId, Kind, DefinedTerm, DefinitionText, LegalScope, Grounding);
}
