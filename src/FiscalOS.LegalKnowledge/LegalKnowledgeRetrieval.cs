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

public sealed record CuratedProvisionSource(StableLegalScope Scope, CuratedStructuralMaterial Material);
public interface ILegalKnowledgeRetrievalService
{
    Task<LegalKnowledgeRetrievalResult> GetLegalActAsync(GetLegalActRequest request, CancellationToken cancellationToken = default);
    Task<LegalKnowledgeRetrievalResult> GetLegalProvisionAsync(GetLegalProvisionRequest request, CancellationToken cancellationToken = default);
    Task<LegalKnowledgeRetrievalResult> GetLegalProvisionAtDateAsync(GetLegalProvisionAtDateRequest request, CancellationToken cancellationToken = default);
    Task<ProvisionHistoryResult> GetProvisionHistoryAsync(GetProvisionHistoryRequest request, CancellationToken cancellationToken = default);
}

public sealed class LegalKnowledgeRetrievalService : ILegalKnowledgeRetrievalService
{
    private readonly IReadOnlyList<CuratedProvisionSource> sources;
    private readonly ISemanticKnowledgeRepository semantic;
    public LegalKnowledgeRetrievalService(IEnumerable<CuratedProvisionSource> sources, ISemanticKnowledgeRepository semantic)
    { this.sources = sources.ToArray(); this.semantic = semantic; }
    public async Task<LegalKnowledgeRetrievalResult> GetLegalActAsync(GetLegalActRequest request, CancellationToken cancellationToken = default)
    {
        var source = sources.FirstOrDefault(s => s.Scope.CanonicalActIdentity == request.Act.ToString());
        return source is null ? new(LegalKnowledgeRetrievalStatus.NotFound) : await ProjectStructuralAsync(source, cancellationToken);
    }
    public async Task<LegalKnowledgeRetrievalResult> GetLegalProvisionAsync(GetLegalProvisionRequest request, CancellationToken cancellationToken = default)
    {
        var source = sources.FirstOrDefault(s => Equals(s.Scope, request.Scope));
        if (source is null) return new(LegalKnowledgeRetrievalStatus.NotFound);
        var candidates = (await semantic.GetCandidatesAsync(cancellationToken)).Where(c => Equals(c.LegalScope, request.Scope)).ToArray();
        var accepted = new List<AcceptedSemanticKnowledge>();
        foreach (var c in candidates) foreach (var d in await semantic.GetHistoryAsync(c.SemanticConceptId, cancellationToken)) if (d.SemanticCandidateId == c.SemanticCandidateId && d.Disposition == SemanticCurationDisposition.Accepted) accepted.Add(new(d, c));
        if (accepted.Count == 0) return await ProjectStructuralAsync(source, cancellationToken);
        var superseded = accepted.SelectMany(x => accepted.Where(y => y.Decision.SupersedesSemanticDecisionId == x.Decision.SemanticCurationDecisionId)).Select(x => x.Decision.SemanticCurationDecisionId).ToHashSet(StringComparer.Ordinal);
        var leaves = accepted.Where(x => !superseded.Contains(x.Decision.SemanticCurationDecisionId)).ToArray();
        return leaves.Length > 1 ? new(LegalKnowledgeRetrievalStatus.Ambiguous) : leaves.Length == 1 ? new(LegalKnowledgeRetrievalStatus.Found, Semantic: Project(leaves[0])) : await ProjectStructuralAsync(source, cancellationToken);
    }
    public async Task<LegalKnowledgeRetrievalResult> GetLegalProvisionAtDateAsync(GetLegalProvisionAtDateRequest request, CancellationToken cancellationToken = default)
    {
        var source = sources.FirstOrDefault(s => Equals(s.Scope, request.Scope));
        if (source is null) return new(LegalKnowledgeRetrievalStatus.NotFound);
        var candidates = (await semantic.GetCandidatesAsync(cancellationToken)).Where(c => Equals(c.LegalScope, request.Scope)).ToArray();
        if (candidates.Length == 0) return await ProjectStructuralAsync(source, cancellationToken);
        var results = await Task.WhenAll(candidates.Select(c => semantic.GetAsOfAsync(c.SemanticConceptId, request.LegalDate, request.KnowledgeAsOf, cancellationToken)));
        var found = results.Where(r => r.Status == SemanticAsOfStatus.Found).Select(r => r.Knowledge!).ToArray();
        if (results.Any(r => r.Status == SemanticAsOfStatus.Ambiguous) || found.Length > 1) return new(LegalKnowledgeRetrievalStatus.Ambiguous);
        return found.Length == 1 ? new(LegalKnowledgeRetrievalStatus.Found, Semantic: Project(found[0])) : await ProjectStructuralAsync(source, cancellationToken);
    }
    public async Task<ProvisionHistoryResult> GetProvisionHistoryAsync(GetProvisionHistoryRequest request, CancellationToken cancellationToken = default)
    {
        var candidates = (await semantic.GetCandidatesAsync(cancellationToken)).Where(c => Equals(c.LegalScope, request.Scope)).ToArray();
        var entries = new List<ProvisionHistoryEntry>();
        foreach (var c in candidates) foreach (var d in await semantic.GetHistoryAsync(c.SemanticConceptId, cancellationToken)) entries.Add(new(new SemanticConceptId(d.SemanticConceptId), d.SemanticCandidateId, d.Disposition, d.KnowledgeTime, d.LegalValidTime, d.SupersedesSemanticDecisionId, Provenance(d,c)));
        entries = entries.OrderBy(e=>e.KnowledgeTime).ThenBy(e=>e.CandidateId, StringComparer.Ordinal).ToList();
        return new(entries.Count == 0 ? LegalKnowledgeRetrievalStatus.NotFound : LegalKnowledgeRetrievalStatus.Found, entries.AsReadOnly());
    }
    private async Task<LegalKnowledgeRetrievalResult> ProjectStructuralAsync(CuratedProvisionSource s, CancellationToken ct)
    { return new(LegalKnowledgeRetrievalStatus.StructurallyKnownWithoutAcceptedSemanticKnowledge, Structural: new StructuralProvisionProjection(s.Scope, s.Material.StructuralDocument.Atoms.Select(a=>a.StructuralAtomId), string.Empty, null)); }
    private static SemanticProvisionProjection Project(AcceptedSemanticKnowledge k) => new(new(k.Candidate.SemanticConceptId), k.Candidate.SemanticCandidateId, k.Candidate.Kind, k.Candidate.DefinitionText, k.Candidate.LegalScope, Provenance(k.Decision,k.Candidate));
    private static LegalKnowledgeProvenance Provenance(SemanticCurationDecision d, SemanticKnowledgeCandidate c) => new(d.SemanticCurationDecisionId,c.SemanticCandidateId,c.Grounding.RawArtifactId,null,null,c.Grounding.NormalizationVersion,c.Grounding.AtomizerVersion);
}
