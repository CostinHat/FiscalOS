using System.Text.Json;

namespace FiscalOS.LegalKnowledge;

public interface ISemanticKnowledgeRepository
{
    Task<SemanticPersistenceResult> SaveAsync(SemanticKnowledgeCandidate candidate, SemanticCurationDecision decision, CancellationToken cancellationToken = default);
    Task<SemanticCurationDecision?> GetDecisionAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SemanticCurationDecision>> GetHistoryAsync(string conceptId, CancellationToken cancellationToken = default);
    Task<SemanticKnowledgeCandidate?> GetCandidateAsync(string id, CancellationToken cancellationToken = default);
}

public enum SemanticPersistenceError { None, UnknownSemanticPredecessor, SelfSemanticSupersession, SemanticSupersessionCycle, SemanticConceptMismatchAcrossSupersession, ConflictingIdentity, CorruptRecord }
public sealed record SemanticPersistenceResult(SemanticPersistenceError Error = SemanticPersistenceError.None);

public sealed class FileSemanticKnowledgeRepository : ISemanticKnowledgeRepository
{
    private readonly string path;
    public FileSemanticKnowledgeRepository(string root) { Directory.CreateDirectory(root); path = Path.Combine(root, "semantic-knowledge.jsonl"); }
    public async Task<SemanticPersistenceResult> SaveAsync(SemanticKnowledgeCandidate candidate, SemanticCurationDecision decision, CancellationToken cancellationToken = default)
    {
        var records = await ReadAsync(cancellationToken);
        if (decision.SupersedesSemanticDecisionId == decision.SemanticCurationDecisionId) return new(SemanticPersistenceError.SelfSemanticSupersession);
        var existing = records.FirstOrDefault(x => x.Decision.SemanticCurationDecisionId == decision.SemanticCurationDecisionId);
        if (existing is not null) return Equals(existing.Decision, decision) && Equals(existing.Candidate, candidate) ? new() : new(SemanticPersistenceError.ConflictingIdentity);
        if (decision.SupersedesSemanticDecisionId is not null)
        {
            var predecessor = records.FirstOrDefault(x => x.Decision.SemanticCurationDecisionId == decision.SupersedesSemanticDecisionId);
            if (predecessor is null) return new(SemanticPersistenceError.UnknownSemanticPredecessor);
            if (predecessor.Decision.SemanticConceptId != decision.SemanticConceptId) return new(SemanticPersistenceError.SemanticConceptMismatchAcrossSupersession);
            var cursor = predecessor.Decision;
            while (cursor.SupersedesSemanticDecisionId is not null)
            {
                if (cursor.SupersedesSemanticDecisionId == decision.SemanticCurationDecisionId) return new(SemanticPersistenceError.SemanticSupersessionCycle);
                cursor = records.First(x => x.Decision.SemanticCurationDecisionId == cursor.SupersedesSemanticDecisionId).Decision;
            }
        }
        await File.AppendAllTextAsync(path, JsonSerializer.Serialize(Persisted.From(candidate, decision)) + Environment.NewLine, cancellationToken);
        return new();
    }
    public async Task<SemanticCurationDecision?> GetDecisionAsync(string id, CancellationToken cancellationToken = default) => (await ReadAsync(cancellationToken)).Select(x => x.Decision).FirstOrDefault(x => x.SemanticCurationDecisionId == id);
    public async Task<SemanticKnowledgeCandidate?> GetCandidateAsync(string id, CancellationToken cancellationToken = default) => (await ReadAsync(cancellationToken)).FirstOrDefault(x => x.Candidate.SemanticCandidateId == id)?.Candidate;
    public async Task<IReadOnlyList<SemanticCurationDecision>> GetHistoryAsync(string conceptId, CancellationToken cancellationToken = default) => (await ReadAsync(cancellationToken)).Where(x => x.Decision.SemanticConceptId == conceptId).Select(x => x.Decision).OrderBy(x => x.KnowledgeTime).ThenBy(x => x.SemanticCurationDecisionId, StringComparer.Ordinal).ToArray();
    private async Task<List<Persisted>> ReadAsync(CancellationToken token) { if (!File.Exists(path)) return new(); var result = new List<Persisted>(); foreach (var line in await File.ReadAllLinesAsync(path, token)) { if (!string.IsNullOrWhiteSpace(line)) { var p = JsonSerializer.Deserialize<Persisted>(line) ?? throw new InvalidDataException("Corrupt semantic record."); if (!p.Candidate.HasValidIdentity(p.NormalizationVersion,p.AtomizerVersion)) throw new InvalidDataException("Semantic candidate identity mismatch."); if (p.Decision.SemanticCandidateId != p.Candidate.SemanticCandidateId || p.Decision.SemanticConceptId != p.Candidate.SemanticConceptId) throw new InvalidDataException("Semantic decision lineage mismatch."); result.Add(p); } } ValidateLineage(result); return result; }
    private static void ValidateLineage(List<Persisted> records) { var map=records.ToDictionary(x=>x.Decision.SemanticCurationDecisionId); foreach(var p in records){ var d=p.Decision; var seed=string.Join("\u001F",d.SemanticCandidateId,d.SemanticConceptId,d.Disposition,d.Reason.Trim(),d.Curator.Trim(),d.KnowledgeTime.ToUniversalTime().ToString("O"),d.CurationVersion); var expected="semantic-curation:"+Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(seed))).ToLowerInvariant(); if(expected!=d.SemanticCurationDecisionId) throw new InvalidDataException("Semantic decision identity mismatch."); var s=d.SupersedesSemanticDecisionId; if(s is null) continue; if(s==d.SemanticCurationDecisionId || !map.TryGetValue(s,out var pred) || pred.Decision.SemanticConceptId!=d.SemanticConceptId) throw new InvalidDataException("Invalid semantic supersession lineage."); var seen=new HashSet<string>(); var cur=s; while(cur is not null){ if(!seen.Add(cur) || cur==d.SemanticCurationDecisionId) throw new InvalidDataException("Semantic supersession cycle."); cur=map[cur].Decision.SupersedesSemanticDecisionId; } } }
    private sealed record Persisted(string CandidateId, string ConceptId, SemanticKnowledgeKind Kind, string DefinedTerm, string DefinitionText, StableLegalScope LegalScope, string RawArtifactId, string[] StructuralAtomIds, string NormalizationVersion, string AtomizerVersion, SemanticCurationDecision Decision)
    {
        public SemanticKnowledgeCandidate Candidate => SemanticKnowledgeCandidate.Rehydrate(Kind, ConceptId, CandidateId, DefinedTerm, DefinitionText, LegalScope, new StructuralGrounding(RawArtifactId, StructuralAtomIds, NormalizationVersion, AtomizerVersion));
        public static Persisted From(SemanticKnowledgeCandidate c, SemanticCurationDecision d) => new(c.SemanticCandidateId, c.SemanticConceptId, c.Kind, c.DefinedTerm, c.DefinitionText, c.LegalScope, c.Grounding.RawArtifactId, c.Grounding.StructuralAtomIds.ToArray(), c.Grounding.NormalizationVersion, c.Grounding.AtomizerVersion, d);
    }
}
