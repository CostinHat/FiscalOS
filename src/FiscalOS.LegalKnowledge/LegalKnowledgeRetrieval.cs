using FiscalOS.LegalKnowledge.Corpus;
namespace FiscalOS.LegalKnowledge;

public sealed record GetLegalActRequest(LegalActIdentity Act);
public sealed record GetLegalProvisionRequest(StableLegalScope Scope);
public sealed record GetLegalProvisionAtDateRequest(StableLegalScope Scope, DateOnly LegalDate, DateTimeOffset KnowledgeAsOf);
public sealed record GetProvisionHistoryRequest(StableLegalScope Scope);
public enum LegalKnowledgeRetrievalStatus { Found, NotFound, Ambiguous, StructurallyKnownWithoutAcceptedSemanticKnowledge }
public sealed record LegalKnowledgeProvenance(string SemanticCurationDecisionId, string SemanticCandidateId, string RawArtifactId, string? SourceLocator, string? SourceHash, string NormalizationVersion, string AtomizerVersion);
public sealed record StructuralProvisionProjection
{
    public StableLegalScope Scope { get; }
    public IReadOnlyList<string> StructuralAtomIds { get; }
    public string Text { get; }
    public LegalKnowledgeProvenance? Provenance { get; }
    public StructuralProvisionProjection(StableLegalScope scope, IEnumerable<string> structuralAtomIds, string text, LegalKnowledgeProvenance? provenance) { Scope=scope; StructuralAtomIds=Array.AsReadOnly(structuralAtomIds.ToArray()); Text=text; Provenance=provenance; }
}
public sealed record SemanticProvisionProjection(SemanticConceptId ConceptId, string CandidateId, SemanticKnowledgeKind Kind, string Text, StableLegalScope Scope, LegalKnowledgeProvenance Provenance);
public sealed record LegalKnowledgeRetrievalResult(LegalKnowledgeRetrievalStatus Status, StructuralProvisionProjection? Structural = null, SemanticProvisionProjection? Semantic = null);
public sealed record ProvisionHistoryEntry(SemanticConceptId ConceptId, string CandidateId, SemanticCurationDisposition Disposition, DateTimeOffset KnowledgeTime, LegalValidTime? LegalValidTime, string? SupersedesSemanticDecisionId, LegalKnowledgeProvenance Provenance);
public sealed record ProvisionHistoryResult(LegalKnowledgeRetrievalStatus Status, IReadOnlyList<ProvisionHistoryEntry> Entries);
