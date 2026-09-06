using FiscalOS.LegalKnowledge.Corpus;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Corpus01_Step4
{
    [Fact]
    public void Curation_decision_identity_is_deterministic_without_clock_or_guid()
    {
        var a = StructuralCuration.Decide(Document(), "raw", "v1", CurationDisposition.Accepted, "reviewed", null, true, DateTimeOffset.UnixEpoch, "reviewer").Material!.Decision;
        var b = StructuralCuration.Decide(Document(), "raw", "v1", CurationDisposition.Accepted, "reviewed", null, true, DateTimeOffset.UnixEpoch, "reviewer").Material!.Decision;
        Assert.Equal(a.CurationDecisionId, b.CurationDecisionId);
        Assert.StartsWith("curation:", a.CurationDecisionId);
    }

    [Fact]
    public void Non_official_secondary_material_requires_explicit_policy()
    {
        var d = Document();
        var result = StructuralCuration.Decide(d, "raw:nonofficial", "v1", CurationDisposition.SecondaryOnly, "informational", null, true, DateTimeOffset.UnixEpoch, "reviewer");
        Assert.Null(result.Material);
    }

    [Fact]
    public void Normalization_and_atomization_do_not_depend_on_curation()
    {
        var normalization = typeof(LegalCorpusNormalizer).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToHashSet();
        Assert.DoesNotContain("FiscalOS.LegalKnowledge", normalization.Where(n => n != typeof(LegalCorpusNormalizer).Assembly.GetName().Name));
        var forbidden = new[] { typeof(StructuralCuration), typeof(CuratedStructuralMaterial), typeof(CurationDecision) };
        foreach (var type in new[] { typeof(LegalCorpusNormalizer), typeof(LegalStructuralAtomizer) })
            foreach (var member in type.GetMembers())
                Assert.DoesNotContain(member switch { System.Reflection.MethodInfo m => m.ReturnType, System.Reflection.PropertyInfo p => p.PropertyType, _ => null }, forbidden);
    }

    [Fact]
    public void Curation_does_not_depend_on_classification_or_resolution()
    {
        var refs = typeof(StructuralCuration).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
        Assert.DoesNotContain("FiscalOS.Runtime", refs);
        Assert.DoesNotContain("FiscalOS.Resolution", refs);
    }

    [Fact]
    public void Curated_material_exposes_no_semantic_legal_knowledge()
    {
        var names = typeof(CuratedStructuralMaterial).GetProperties().Select(p => p.PropertyType.Name).ToArray();
        Assert.DoesNotContain(names, n => n.Contains("Classification", StringComparison.OrdinalIgnoreCase) || n.Contains("LegalBasis", StringComparison.OrdinalIgnoreCase) || n.Contains("Condition", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Only_explicit_accepted_decision_promotes_material()
    {
        var d = Document();
        Assert.NotNull(StructuralCuration.Decide(d, "raw", "v1", CurationDisposition.Accepted, "reviewed", null, true, DateTimeOffset.UnixEpoch, "r").Material);
        Assert.Null(StructuralCuration.Decide(d, "raw", "v1", CurationDisposition.Rejected, "rejected", null, true, DateTimeOffset.UnixEpoch, "r").Material);
        Assert.Null(StructuralCuration.Decide(d, "raw", "v1", CurationDisposition.NeedsReview, "pending", null, true, DateTimeOffset.UnixEpoch, "r").Material);
        Assert.Null(StructuralCuration.Decide(d, "raw", "v1", CurationDisposition.SecondaryOnly, "secondary", null, true, DateTimeOffset.UnixEpoch, "r").Material);
    }
    private static string Fixture(string relative) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "corpus", "imported-legislation", relative);

    [Fact]
    public async Task Logical_forms_are_independent_curation_subjects()
    {
        var root = Path.Combine(Path.GetTempPath(), "cur-" + Guid.NewGuid());
        try
        {
            var repo = new FileRawLegalCorpusRepository(root);
            async Task<(RawImportRecord Import, StructuralLegalDocument Document, CurationResult Curation)> Build(string file, LegalActIdentity act, DocumentForm form, ImportDisposition disposition)
            {
                var bytes = await File.ReadAllBytesAsync(Fixture(file));
                var imported = await repo.AcquireAsync(file, "text/html", bytes, SourceClassification.OfficialPortal, "https://legislatie.just.ro/Public/DetaliiDocument/305817", act, form, null, DateTimeOffset.UnixEpoch, disposition, file);
                var normalized = LegalCorpusNormalizer.Normalize(imported.Artifact, bytes);
                var document = LegalStructuralAtomizer.Atomize(normalized);
                var atom = document.Atoms.First(a => a.AtomType == StructuralAtomType.Article);
                var curation = StructuralCuration.Decide(document, imported.Artifact.RawArtifactId, normalized.NormalizationVersion, disposition == ImportDisposition.AcceptedRawCandidate ? CurationDisposition.Accepted : CurationDisposition.NeedsReview, file, new[] { atom.StructuralAtomId }, false, DateTimeOffset.UnixEpoch, "reviewer");
                return (imported, document, curation);
            }
            var act89 = new LegalActIdentity("RO", LegalActType.EmergencyOrdinance, 89, 2025);
            var a = await Build("oug_89_2025.html", act89, DocumentForm.Base, ImportDisposition.AcceptedRawCandidate);
            var b = await Build(Path.Combine("SEO", "ORD DE URGENTA 89 23_12_2025 - Portal Legislativ.html"), act89, DocumentForm.Base, ImportDisposition.VersionVariant);
            Assert.NotEqual(a.Import.Artifact.RawArtifactId, b.Import.Artifact.RawArtifactId); Assert.Null(b.Curation.Material);
            Assert.All(a.Curation.Material!.Decision.StructuralAtomIds, id => Assert.Contains(a.Document.Atoms, x => x.StructuralAtomId == id));
            Assert.DoesNotContain(a.Curation.Material.Decision.StructuralAtomIds, id => b.Document.Atoms.Any(x => x.StructuralAtomId == id));
            var act1074 = new LegalActIdentity("RO", LegalActType.GovernmentDecision, 1074, 2021);
            var h1 = await Build(Path.Combine("SEO", "hg_1074_2021.html"), act1074, DocumentForm.Base, ImportDisposition.AcceptedRawCandidate);
            var h2 = await Build(Path.Combine("SEO", "HOTARARE (A) 1074 04_10_2021 - Portal Legislativ.html"), act1074, DocumentForm.Amended, ImportDisposition.VersionVariant);
            Assert.NotEqual(h1.Import.Artifact.RawArtifactId, h2.Import.Artifact.RawArtifactId); Assert.NotEqual(h1.Document.Atoms.First().StructuralAtomId, h2.Document.Atoms.First().StructuralAtomId); Assert.NotNull(h1.Curation.Material); Assert.Null(h2.Curation.Material);
            Assert.All(h1.Curation.Material!.Decision.StructuralAtomIds, id => Assert.Contains(h1.Document.Atoms, x => x.StructuralAtomId == id));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
    private static StructuralLegalDocument Document() => new("corpus-atomizer-v1", new[] { new StructuralAtom("a1", StructuralAtomType.Act, "ACT", null, 0, 0, 20, "corpus-atomizer-v1", Array.Empty<NormalizedSourceSpan>()) });

    [Fact]
    public async Task Accepted_decision_is_explicit_and_survives_reload()
    {
        var d = Document(); var result = StructuralCuration.Decide(d, "raw:x", "corpus-normalizer-v1", CurationDisposition.Accepted, "reviewed", null, true, DateTimeOffset.UnixEpoch, "reviewer");
        Assert.NotNull(result.Material); var root = Path.Combine(Path.GetTempPath(), "cur-" + Guid.NewGuid());
        try { var repo = new FileStructuralCurationRepository(root); await repo.SaveAsync(result.Material!.Decision); var loaded = Assert.Single(await new FileStructuralCurationRepository(root).GetAllAsync()); Assert.Equal(result.Material.Decision, loaded); }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Non_accepted_decisions_do_not_promote_and_invalid_scopes_are_typed()
    {
        var d = Document();
        Assert.Null(StructuralCuration.Decide(d, "raw:x", "v1", CurationDisposition.NeedsReview, "pending", null, true, DateTimeOffset.UnixEpoch, "r").Material);
        Assert.Null(StructuralCuration.Decide(d, "raw:x", "v1", CurationDisposition.Rejected, "bad", null, true, DateTimeOffset.UnixEpoch, "r").Material);
        Assert.Equal(CurationError.UnknownAtom, StructuralCuration.Decide(d, "raw:x", "v1", CurationDisposition.Accepted, "x", new[] { "missing" }, false, DateTimeOffset.UnixEpoch, "r").Error);
        Assert.Equal(CurationError.DuplicateAtom, StructuralCuration.Decide(new("v1", new[] { new StructuralAtom("a", StructuralAtomType.Act, "ACT", null, 0, 0, 1, "v1", Array.Empty<NormalizedSourceSpan>()) }), "raw", "v1", CurationDisposition.Accepted, "x", new[] { "a", "a" }, false, DateTimeOffset.UnixEpoch, "r").Error);
    }

    [Fact]
    public async Task Supersession_preserves_history_after_reload()
    {
        var accepted = StructuralCuration.Decide(Document(), "raw:x", "v1", CurationDisposition.Accepted, "initial", null, true, DateTimeOffset.UnixEpoch, "reviewer").Material!.Decision;
        var successor = new CurationDecision("curation:successor", accepted.RawArtifactId, accepted.AtomizerVersion, accepted.NormalizationVersion, accepted.Disposition, "corrected", accepted.StructuralAtomIds, accepted.DocumentScope, accepted.KnowledgeTime, accepted.Curator, accepted.CurationVersion, accepted.CurationDecisionId);
        var root = Path.Combine(Path.GetTempPath(), "cur-" + Guid.NewGuid());
        try { var repo = new FileStructuralCurationRepository(root); await repo.SaveAsync(accepted); await repo.SaveAsync(successor); var all = await new FileStructuralCurationRepository(root).GetAllAsync(); Assert.Equal(2, all.Count); Assert.Equal(accepted.CurationDecisionId, successor.SupersedesDecisionId); }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public async Task SupersessionCycle_returns_typed_error()
    {
        var a = StructuralCuration.Decide(Document(), "raw:x", "v1", CurationDisposition.Accepted, "a", null, true, DateTimeOffset.UnixEpoch, "r").Material!.Decision;
        var b = new CurationDecision("b", "raw:x", "v1", "v1", CurationDisposition.Accepted, "b", a.StructuralAtomIds, true, DateTimeOffset.UnixEpoch, "r", StructuralCuration.Version, a.CurationDecisionId);
        var root = Path.Combine(Path.GetTempPath(), "cur-" + Guid.NewGuid());
        try { var repo = new FileStructuralCurationRepository(root); Assert.Equal(CurationError.None, (await repo.SaveAsync(a)).Error); Assert.Equal(CurationError.None, (await repo.SaveAsync(b)).Error); var cycle = new CurationDecision(a.CurationDecisionId, b.RawArtifactId, b.AtomizerVersion, b.NormalizationVersion, b.Disposition, b.Reason, b.StructuralAtomIds, b.DocumentScope, b.KnowledgeTime, b.Curator, b.CurationVersion, b.CurationDecisionId); Assert.Equal(CurationError.SupersessionCycle, (await repo.SaveAsync(cycle)).Error); }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public async Task Multi_hop_supersession_chain_survives_reload_without_mutation()
    {
        var a = StructuralCuration.Decide(Document(), "raw:chain", "v1", CurationDisposition.Accepted, "A", null, true, DateTimeOffset.UnixEpoch, "r").Material!.Decision;
        var b = new CurationDecision("chain-b", "raw:chain", "v1", "v1", CurationDisposition.Accepted, "B", a.StructuralAtomIds, true, DateTimeOffset.UnixEpoch.AddDays(1), "r", StructuralCuration.Version, a.CurationDecisionId);
        var c = new CurationDecision("chain-c", "raw:chain", "v1", "v1", CurationDisposition.Accepted, "C", a.StructuralAtomIds, true, DateTimeOffset.UnixEpoch.AddDays(2), "r", StructuralCuration.Version, b.CurationDecisionId);
        var root = Path.Combine(Path.GetTempPath(), "cur-" + Guid.NewGuid());
        try { var repo = new FileStructuralCurationRepository(root); await repo.SaveAsync(a); await repo.SaveAsync(b); await repo.SaveAsync(c); var all = await new FileStructuralCurationRepository(root).GetAllAsync(); Assert.Equal(3, all.Count); Assert.Equal(a.CurationDecisionId, all[0].CurationDecisionId); Assert.Equal(a.CurationDecisionId, all[1].SupersedesDecisionId); Assert.Equal(b.CurationDecisionId, all[2].SupersedesDecisionId); Assert.Equal("A", all[0].Reason); }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public async Task Real_fixture_pipeline_preserves_raw_provenance_and_deterministic_identity()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "corpus", "legea-227-2015-official.html"); var bytes = await File.ReadAllBytesAsync(path);
        var raw = new RawCorpusArtifact("raw:law", "law.html", "text/html", bytes.Length, FileRawLegalCorpusRepository.Hash(bytes), SourceClassification.OfficialPortal, null, "171280", DateTimeOffset.UnixEpoch, null, null, DocumentForm.Base, "");
        var normalized = LegalCorpusNormalizer.Normalize(raw, bytes); var structural = LegalStructuralAtomizer.Atomize(normalized); var article = structural.Atoms.First(a => a.AtomType == StructuralAtomType.Article && a.Designation == "47");
        var curated = StructuralCuration.Decide(structural, raw.RawArtifactId, normalized.NormalizationVersion, CurationDisposition.Accepted, "reviewed structural material", new[] { article.StructuralAtomId }, false, DateTimeOffset.UnixEpoch, "reviewer");
        Assert.NotNull(curated.Material); Assert.Equal(raw.RawArtifactId, curated.Material!.Decision.RawArtifactId); Assert.NotEmpty(article.SourceSpans);
    }

    [Fact]
    public async Task Accepted_real_material_preserves_end_to_end_provenance()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "corpus", "legea-227-2015-official.html");
        var bytes = await File.ReadAllBytesAsync(path);
        var raw = new RawCorpusArtifact("raw:law-official", "official.html", "text/html", bytes.Length, FileRawLegalCorpusRepository.Hash(bytes), SourceClassification.OfficialPortal, null, "171280", DateTimeOffset.UnixEpoch, null, null, DocumentForm.Base, "");
        var normalized = LegalCorpusNormalizer.Normalize(raw, bytes);
        var structural = LegalStructuralAtomizer.Atomize(normalized);
        var article = structural.Atoms.First(a => a.AtomType == StructuralAtomType.Article);
        var curation = StructuralCuration.Decide(structural, raw.RawArtifactId, normalized.NormalizationVersion, CurationDisposition.Accepted, "structural review", new[] { article.StructuralAtomId }, false, DateTimeOffset.UnixEpoch, "reviewer");
        Assert.NotNull(curation.Material); var curated = curation.Material!;
        Assert.Equal(curation.Material!.Decision.CurationDecisionId, curated.Decision.CurationDecisionId);
        Assert.Equal(new[] { article.StructuralAtomId }, curated.Decision.StructuralAtomIds);
        var selected = structural.Atoms.Single(a => a.StructuralAtomId == curated.Decision.StructuralAtomIds.Single());
        Assert.Equal(raw.RawArtifactId, structural.RawArtifactId); Assert.Equal(normalized.NormalizationVersion, structural.NormalizationVersion); Assert.Equal(LegalStructuralAtomizer.Version, selected.AtomizerVersion);
        Assert.InRange(selected.NormalizedStart, 0, normalized.NormalizedText.Length - 1); Assert.InRange(selected.NormalizedLength, 1, normalized.NormalizedText.Length - selected.NormalizedStart);
        Assert.NotEmpty(selected.SourceSpans); Assert.All(selected.SourceSpans, span => { Assert.Equal(raw.RawArtifactId, span.RawArtifactId); Assert.InRange(span.SourceStart, 0, bytes.Length - 1); Assert.InRange(span.SourceLength, 1, bytes.Length - span.SourceStart); Assert.InRange(span.NormalizedStart, 0, normalized.NormalizedText.Length - 1); Assert.InRange(span.NormalizedLength, 1, normalized.NormalizedText.Length - span.NormalizedStart); });
    }

    [Fact]
    public async Task All_curation_validation_failures_are_typed()
    {
        var atom = new StructuralAtom("a", StructuralAtomType.Act, "ACT", null, 0, 0, 1, "atom-v1", Array.Empty<NormalizedSourceSpan>());
        var doc = new StructuralLegalDocument("atom-v1", new[] { atom }) { RawArtifactId = "raw", NormalizationVersion = "norm-v1" };
        Assert.Equal(CurationError.RawArtifactMismatch, StructuralCuration.Decide(doc, "other", "norm-v1", CurationDisposition.Accepted, "r", new[] { "a" }, false, DateTimeOffset.UnixEpoch, "c").Error);
        Assert.Equal(CurationError.NormalizationVersionMismatch, StructuralCuration.Decide(doc, "raw", "other", CurationDisposition.Accepted, "r", new[] { "a" }, false, DateTimeOffset.UnixEpoch, "c").Error);
        Assert.Equal(CurationError.AtomizerVersionMismatch, StructuralCuration.Decide(new("other", new[] { atom }), "raw", "norm-v1", CurationDisposition.Accepted, "r", new[] { "a" }, false, DateTimeOffset.UnixEpoch, "c").Error);
        Assert.Equal(CurationError.InvalidCurator, StructuralCuration.Decide(doc, "raw", "norm-v1", CurationDisposition.Accepted, "r", new[] { "a" }, false, DateTimeOffset.UnixEpoch, " ").Error);
        Assert.Equal(CurationError.InvalidReason, StructuralCuration.Decide(doc, "raw", "norm-v1", CurationDisposition.Accepted, " ", new[] { "a" }, false, DateTimeOffset.UnixEpoch, "c").Error);
        Assert.Equal(CurationError.EmptyAtomScope, StructuralCuration.Decide(doc, "raw", "norm-v1", CurationDisposition.Accepted, "r", Array.Empty<string>(), false, DateTimeOffset.UnixEpoch, "c").Error);
        Assert.Equal(CurationError.UnknownStructuralAtomId, StructuralCuration.Decide(doc, "raw", "norm-v1", CurationDisposition.Accepted, "r", new[] { "x" }, false, DateTimeOffset.UnixEpoch, "c").Error);
        Assert.Equal(CurationError.DuplicateStructuralAtomId, StructuralCuration.Decide(doc, "raw", "norm-v1", CurationDisposition.Accepted, "r", new[] { "a", "a" }, false, DateTimeOffset.UnixEpoch, "c").Error);
        var root = Path.Combine(Path.GetTempPath(), "cur-" + Guid.NewGuid()); try { var repo = new FileStructuralCurationRepository(root); var a = StructuralCuration.Decide(doc, "raw", "norm-v1", CurationDisposition.Accepted, "r", null, true, DateTimeOffset.UnixEpoch, "c").Material!.Decision; Assert.Equal(CurationError.UnknownPredecessor, (await repo.TrySaveAsync(new CurationDecision("b", "raw", "atom-v1", "norm-v1", CurationDisposition.Accepted, "r", new[] { "a" }, true, DateTimeOffset.UnixEpoch, "c", StructuralCuration.Version, "missing"))).Error); Assert.Equal(CurationError.SelfSupersession, (await repo.TrySaveAsync(new CurationDecision(a.CurationDecisionId, a.RawArtifactId, a.AtomizerVersion, a.NormalizationVersion, a.Disposition, a.Reason, a.StructuralAtomIds, a.DocumentScope, a.KnowledgeTime, a.Curator, a.CurationVersion, a.CurationDecisionId))).Error); } finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
