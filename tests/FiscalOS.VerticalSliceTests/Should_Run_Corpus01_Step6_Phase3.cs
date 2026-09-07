using FiscalOS.LegalKnowledge;
using FiscalOS.LegalKnowledge.Corpus;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Corpus01_Step6_Phase3
{
    [Fact]
    public async Task Real_fixture_bitemporal_retrieval_survives_reload()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","..","..","tests","FiscalOS.VerticalSliceTests","Fixtures","corpus","legea-227-2015-official.html"));
        var bytes = await File.ReadAllBytesAsync(path);
        var hash = FileRawLegalCorpusRepository.Hash(bytes);
        var act = new LegalActIdentity("RO", LegalActType.Law, 227, 2015);
        var artifact = new RawCorpusArtifact("raw:"+hash,"legea-227-2015-official.html","text/html",bytes.Length,hash,SourceClassification.OfficialPortal,"https://legislatie.just.ro/Public/DetaliiDocument/171280","171280",DateTimeOffset.UnixEpoch,"MO",act,DocumentForm.Base,path);
        var normalized = LegalCorpusNormalizer.Normalize(artifact, bytes);
        var document = LegalStructuralAtomizer.Atomize(normalized);
        var article = document.Atoms.Single(a => a.AtomType == StructuralAtomType.Article && a.Designation == "7");
        var structural = StructuralCuration.Decide(document, artifact.RawArtifactId, normalized.NormalizationVersion, CurationDisposition.Accepted, "reviewed", new[]{article.StructuralAtomId}, false, DateTimeOffset.UnixEpoch, "reviewer");
        var scope = new StableLegalScope(act.ToString(), "Article/" + article.Designation);
        var candidate = SemanticKnowledgeCandidate.Create(SemanticKnowledgeKind.LegalDefinition, "contribuabil", "persoana definita expres in articolul 7", structural.Material!, new[]{article.StructuralAtomId}, scope).Candidate!;
        var decision = SemanticCuration.Decide(candidate,candidate,SemanticCurationDisposition.Accepted,"explicit semantic review","reviewer",DateTimeOffset.Parse("2026-01-01Z"),new LegalValidTime(new DateOnly(2026,1,1))).Decision!;
        var root = Path.Combine(Path.GetTempPath(),"fos-step6-e2e-"+Guid.NewGuid().ToString("N"));
        var source = new CuratedProvisionSource(scope, structural.Material!);
        var before = new LegalKnowledgeRetrievalService(new[]{source}, new FileSemanticKnowledgeRepository(root));
        Assert.Equal(LegalKnowledgeRetrievalStatus.StructurallyKnownWithoutAcceptedSemanticKnowledge, (await before.GetLegalProvisionAsync(new GetLegalProvisionRequest(scope))).Status);
        await new FileSemanticKnowledgeRepository(root).SaveAsync(candidate,decision);
        var service = new LegalKnowledgeRetrievalService(new[]{source}, new FileSemanticKnowledgeRepository(root));
        var result = await service.GetLegalProvisionAtDateAsync(new GetLegalProvisionAtDateRequest(scope,new DateOnly(2026,1,1),DateTimeOffset.Parse("2026-02-01Z")));
        Assert.Equal(LegalKnowledgeRetrievalStatus.Found,result.Status);
        Assert.Equal(candidate.Grounding.RawArtifactId,result.Semantic!.Provenance.RawArtifactId);
        Assert.Equal(candidate.Grounding.StructuralAtomIds, structural.Material!.Decision.StructuralAtomIds);
        Assert.Equal("RO:Law:227/2015",scope.CanonicalActIdentity);
        Assert.Equal(result.Status,(await new LegalKnowledgeRetrievalService(new[]{new CuratedProvisionSource(scope, structural.Material!)}, new FileSemanticKnowledgeRepository(root)).GetLegalProvisionAtDateAsync(new GetLegalProvisionAtDateRequest(scope,new DateOnly(2026,1,1),DateTimeOffset.Parse("2026-02-01Z")))).Status);
    }
}
