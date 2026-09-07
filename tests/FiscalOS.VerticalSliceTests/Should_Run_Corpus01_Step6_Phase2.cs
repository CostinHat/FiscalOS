using FiscalOS.LegalKnowledge;
using FiscalOS.LegalKnowledge.Corpus;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Corpus01_Step6_Phase2
{
    [Fact] public async Task Empty_act_and_provision_are_not_found()
    {
        var repo = new EmptySemanticRepository();
        var service = new LegalKnowledgeRetrievalService(Array.Empty<CuratedProvisionSource>(), repo);
        var act = new LegalActIdentity("RO", LegalActType.Law, 227, 2015);
        Assert.Equal(LegalKnowledgeRetrievalStatus.NotFound, (await service.GetLegalActAsync(new GetLegalActRequest(act))).Status);
        Assert.Equal(LegalKnowledgeRetrievalStatus.NotFound, (await service.GetLegalProvisionAsync(new GetLegalProvisionRequest(new StableLegalScope(act.ToString(), "Article/1")))).Status);
    }

    [Fact] public void Requests_require_canonical_scope_and_both_temporal_axes()
    {
        var act = new LegalActIdentity("RO", LegalActType.Law, 227, 2015);
        var scope = new StableLegalScope(act.ToString(), "Article/1");
        var request = new GetLegalProvisionAtDateRequest(scope, new DateOnly(2026,1,1), new DateTimeOffset(2026,2,1,0,0,0,TimeSpan.Zero));
        Assert.Equal(scope, request.Scope); Assert.Equal(new DateOnly(2026,1,1), request.LegalDate); Assert.Equal(DateTimeOffset.Parse("2026-02-01Z"), request.KnowledgeAsOf);
    }

    [Fact] public void Statuses_are_exact_and_structural_is_not_authority()
    {
        Assert.Equal(new[]{"Found","NotFound","Ambiguous","StructurallyKnownWithoutAcceptedSemanticKnowledge"}, Enum.GetNames<LegalKnowledgeRetrievalStatus>());
        var result = new LegalKnowledgeRetrievalResult(LegalKnowledgeRetrievalStatus.StructurallyKnownWithoutAcceptedSemanticKnowledge);
        Assert.Null(result.Semantic);
    }

    [Fact] public void History_is_diagnostic_and_supersession_is_not_amendment()
    {
        var p = new LegalKnowledgeProvenance("d","c","raw",null,null,"n","a");
        var h = new ProvisionHistoryEntry(new SemanticConceptId("concept:x"),"candidate:x",SemanticCurationDisposition.Accepted,DateTimeOffset.UnixEpoch,null,"previous",p);
        Assert.Equal("previous", h.SupersedesSemanticDecisionId); Assert.NotNull(h.Provenance);
    }

    private sealed class EmptySemanticRepository : ISemanticKnowledgeRepository
    {
        public Task<SemanticPersistenceResult> SaveAsync(SemanticKnowledgeCandidate c, SemanticCurationDecision d, CancellationToken t=default)=>Task.FromResult(new SemanticPersistenceResult());
        public Task<SemanticCurationDecision?> GetDecisionAsync(string id,CancellationToken t=default)=>Task.FromResult<SemanticCurationDecision?>(null);
        public Task<IReadOnlyList<SemanticCurationDecision>> GetHistoryAsync(string id,CancellationToken t=default)=>Task.FromResult<IReadOnlyList<SemanticCurationDecision>>(Array.Empty<SemanticCurationDecision>());
        public Task<SemanticKnowledgeCandidate?> GetCandidateAsync(string id,CancellationToken t=default)=>Task.FromResult<SemanticKnowledgeCandidate?>(null);
        public Task<IReadOnlyList<SemanticKnowledgeCandidate>> GetCandidatesAsync(CancellationToken t=default)=>Task.FromResult<IReadOnlyList<SemanticKnowledgeCandidate>>(Array.Empty<SemanticKnowledgeCandidate>());
        public Task<SemanticAsOfResult> GetAsOfAsync(string id,DateOnly d,DateTimeOffset k,CancellationToken t=default)=>Task.FromResult(new SemanticAsOfResult(SemanticAsOfStatus.NotFound));
    }
}
