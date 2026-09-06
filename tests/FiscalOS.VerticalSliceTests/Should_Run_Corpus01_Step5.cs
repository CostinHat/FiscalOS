using FiscalOS.LegalKnowledge;
using FiscalOS.LegalKnowledge.Corpus;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Corpus01_Step5
{
    private static CuratedStructuralMaterial Curated(string raw = "raw-1", params string[] ids)
    {
        if (ids.Length == 0) ids = new[] { "atom-1" };
        var atoms = ids.Select((id, i) => new StructuralAtom(id, StructuralAtomType.Article, (i + 1).ToString(), null, i * 10, 0, 10, "corpus-atomizer-v1", new[] { new NormalizedSourceSpan(raw, i * 10, 10, i * 10, 10) })).ToArray();
        var doc = new StructuralLegalDocument("corpus-atomizer-v1", atoms) { RawArtifactId = raw, NormalizationVersion = "corpus-normalizer-v1" };
        var decision = new CurationDecision("cur-" + raw, raw, "corpus-atomizer-v1", "corpus-normalizer-v1", CurationDisposition.Accepted, "reviewed", ids, false, DateTimeOffset.UnixEpoch, "reviewer", StructuralCuration.Version);
        return new CuratedStructuralMaterial(decision, doc);
    }
    private static SemanticKnowledgeCandidate Candidate(string raw = "raw-1", string text = "text", string act = "RO:Law:227/2015", string scope = "Article/7", params string[] ids)
        => SemanticKnowledgeCandidate.Create(SemanticKnowledgeKind.LegalDefinition, "termen", text, Curated(raw, ids), ids.Length == 0 ? new[] { "atom-1" } : ids, new StableLegalScope(act, scope)).Candidate!;

    [Fact] public void Legal_definition_requires_curated_structural_grounding() { var r = SemanticKnowledgeCandidate.Create(SemanticKnowledgeKind.LegalDefinition, "termen", "Conținut furnizat explicit.", Curated(), new[] { "atom-1" }, new StableLegalScope("RO:Law:227/2015", "Article/7")); Assert.Equal(SemanticCandidateError.None, r.Error); }
    [Fact] public void Semantic_identity_is_deterministic_and_grounding_order_is_canonical() { var a = Candidate(ids: new[] { "atom-1", "atom-2" }); var b = Candidate(ids: new[] { "atom-2", "atom-1" }); Assert.Equal(a.SemanticConceptId, b.SemanticConceptId); Assert.Equal(a.SemanticCandidateId, b.SemanticCandidateId); Assert.Equal(a, b); }
    [Fact] public void Changed_text_changes_candidate_not_concept() { var a = Candidate(text: "text"); var b = Candidate(text: "corrected"); Assert.Equal(a.SemanticConceptId, b.SemanticConceptId); Assert.NotEqual(a.SemanticCandidateId, b.SemanticCandidateId); }
    [Fact] public void Changed_grounding_changes_candidate_not_concept() { var a = Candidate(raw: "raw-a"); var b = Candidate(raw: "raw-b"); Assert.Equal(a.SemanticConceptId, b.SemanticConceptId); Assert.NotEqual(a.SemanticCandidateId, b.SemanticCandidateId); }
    [Fact] public void Legal_scope_and_act_prevent_same_term_collisions() { Assert.NotEqual(Candidate(act: "RO:Law:1/2020").SemanticConceptId, Candidate(act: "RO:Law:2/2020").SemanticConceptId); Assert.NotEqual(Candidate(scope: "Article/7").SemanticConceptId, Candidate(scope: "Article/8").SemanticConceptId); }
    [Fact] public void Candidate_is_not_authoritative_knowledge() { var c = Candidate(); Assert.NotNull(c); Assert.DoesNotContain("Authoritative", c.GetType().GetProperties().Select(p => p.Name)); }
    [Fact] public void Unsupported_or_invalid_grounding_returns_typed_errors() { var c = Curated(); Assert.Equal(SemanticCandidateError.UnsupportedSemanticKind, SemanticKnowledgeCandidate.Create((SemanticKnowledgeKind)99, "t", "x", c, new[] { "atom-1" }, new StableLegalScope("RO:Law:1/1", "Article/1")).Error); Assert.Equal(SemanticCandidateError.MissingStructuralGrounding, SemanticKnowledgeCandidate.Create(SemanticKnowledgeKind.LegalDefinition, "t", "x", c, Array.Empty<string>(), new StableLegalScope("RO:Law:1/1", "Article/1")).Error); Assert.Equal(SemanticCandidateError.UnknownGroundingAtomId, SemanticKnowledgeCandidate.Create(SemanticKnowledgeKind.LegalDefinition, "t", "x", c, new[] { "missing" }, new StableLegalScope("RO:Law:1/1", "Article/1")).Error); Assert.Equal(SemanticCandidateError.InvalidDefinedTerm, SemanticKnowledgeCandidate.Create(SemanticKnowledgeKind.LegalDefinition, " ", "x", c, new[] { "atom-1" }, new StableLegalScope("RO:Law:1/1", "Article/1")).Error); Assert.Equal(SemanticCandidateError.InvalidDefinitionText, SemanticKnowledgeCandidate.Create(SemanticKnowledgeKind.LegalDefinition, "t", " ", c, new[] { "atom-1" }, new StableLegalScope("RO:Law:1/1", "Article/1")).Error); }
}
