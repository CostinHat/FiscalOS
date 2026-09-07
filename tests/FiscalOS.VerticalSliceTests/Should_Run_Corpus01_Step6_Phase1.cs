using FiscalOS.LegalKnowledge;
using FiscalOS.LegalKnowledge.Corpus;
using Xunit;

namespace FiscalOS.VerticalSliceTests;
public sealed class Should_Run_Corpus01_Step6_Phase1
{
 [Fact] public void Requests_use_stable_identities_and_explicit_time() { var a=new LegalActIdentity("RO",FiscalOS.LegalKnowledge.Corpus.LegalActType.Law,227,2015); var s=new StableLegalScope(a.ToString(),"Article/7"); var r=new GetLegalProvisionAtDateRequest(s,new DateOnly(2026,1,1),DateTimeOffset.Parse("2026-02-01Z")); Assert.Equal(s,r.Scope); Assert.NotEqual(r.LegalDate.ToString(),r.KnowledgeAsOf.ToString()); }
 [Fact] public void Statuses_are_typed_and_structural_is_not_semantic() { Assert.Equal(4,Enum.GetValues<LegalKnowledgeRetrievalStatus>().Length); var x=new LegalKnowledgeRetrievalResult(LegalKnowledgeRetrievalStatus.StructurallyKnownWithoutAcceptedSemanticKnowledge,new StructuralProvisionProjection(new StableLegalScope("RO:Law:227/2015","Article/7"),Array.Empty<string>(),"text",null)); Assert.Null(x.Semantic); }
 [Fact] public void Provenance_and_history_are_explicit() { var p=new LegalKnowledgeProvenance("d","c","raw",null,"hash","norm","atom"); var e=new ProvisionHistoryEntry(new SemanticConceptId("x"),"y",SemanticCurationDisposition.Accepted,DateTimeOffset.UnixEpoch,null,"prev",p); Assert.Equal("prev",e.SupersedesSemanticDecisionId); }
 [Fact] public void Stable_scope_does_not_require_atom_id() { var r=new GetLegalProvisionRequest(new StableLegalScope("RO:Law:227/2015","Article/7")); Assert.Equal("Article/7",r.Scope.StructuralPath); }
 [Fact] public void Retrieval_contract_has_no_runtime_dependency() { Assert.DoesNotContain("Runtime",typeof(GetLegalActRequest).Assembly.GetReferencedAssemblies().Select(x=>x.Name)); }
 [Fact] public void Structural_projection_defensively_copies_collections() { var ids=new List<string>{"a"}; var p=new StructuralProvisionProjection(new StableLegalScope("RO:Law:1/2020","Article/1"),ids,"text",null); ids.Add("b"); Assert.Single(p.StructuralAtomIds); Assert.Throws<NotSupportedException>(()=>((IList<string>)p.StructuralAtomIds)[0]="x"); }
}
