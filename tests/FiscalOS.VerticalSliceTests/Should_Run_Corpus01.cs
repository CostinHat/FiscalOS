using FiscalOS.LegalKnowledge.Corpus;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Corpus01
{
    private static string F(string p) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "corpus", "imported-legislation", p);
    private static LegalActIdentity A(LegalActType t, int n, int y) => new("RO", t, n, y, "Romania");

    [Fact]
    public async Task Import_decision_reload_and_locator()
    {
        var root = Path.Combine(Path.GetTempPath(), "fos-" + Guid.NewGuid());
        try { var bytes = await File.ReadAllBytesAsync(F("oug_8_2026.html")); var s = SourceLocatorExtractor.Extract(bytes); Assert.Equal("307580", s.PortalDocumentId); var r = new FileRawLegalCorpusRepository(root); var x = await r.AcquireAsync("oug_8_2026.html", "text/html", bytes, SourceClassification.OfficialPortal, s.Locator, A(LegalActType.EmergencyOrdinance, 8, 2026), DocumentForm.Base, null, DateTimeOffset.UtcNow, ImportDisposition.VersionVariant, "variant"); var q = Assert.Single(await new FileRawLegalCorpusRepository(root).GetImportRecordsAsync()); Assert.Equal(x.Disposition, q.Disposition); Assert.Equal(x.Reason, q.Reason); Assert.Equal(x.Artifact.RawArtifactId, q.Artifact.RawArtifactId); Assert.Equal(bytes, (await r.ReadAsync(q.Artifact)).Payload); }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public async Task Logical_duplicates_and_variants_are_retained()
    {
        var root = Path.Combine(Path.GetTempPath(), "fos-" + Guid.NewGuid());
        try { var r = new FileRawLegalCorpusRepository(root); var act = A(LegalActType.EmergencyOrdinance, 89, 2025); var p1 = await File.ReadAllBytesAsync(F("oug_89_2025.html")); var p2 = await File.ReadAllBytesAsync(F(Path.Combine("SEO", "ORD DE URGENTA 89 23_12_2025 - Portal Legislativ.html"))); var x = await r.AcquireAsync("a", "text/html", p1, SourceClassification.OfficialPortal, "https://legislatie.just.ro/Public/DetaliiDocument/305817", act, DocumentForm.Base, null, DateTimeOffset.UtcNow, ImportDisposition.AcceptedRawCandidate, "base"); var y = await r.AcquireAsync("b", "text/html", p2, SourceClassification.OfficialPortal, "https://legislatie.just.ro/Public/DetaliiDocument/305817", act, DocumentForm.Base, null, DateTimeOffset.UtcNow, ImportDisposition.VersionVariant, "alternate"); Assert.NotEqual(x.Artifact.RawArtifactId, y.Artifact.RawArtifactId); Assert.NotEqual(x.Artifact.Sha256, y.Artifact.Sha256); Assert.Equal(2, (await r.GetArtifactsAsync()).Count); var h = A(LegalActType.GovernmentDecision, 1074, 2021); var z = await r.AcquireAsync("h", "text/html", await File.ReadAllBytesAsync(F(Path.Combine("SEO", "hg_1074_2021.html"))), SourceClassification.OfficialPortal, null, h, DocumentForm.Base, null, DateTimeOffset.UtcNow, ImportDisposition.AcceptedRawCandidate, "base"); var w = await r.AcquireAsync("ha", "text/html", await File.ReadAllBytesAsync(F(Path.Combine("SEO", "HOTARARE (A) 1074 04_10_2021 - Portal Legislativ.html"))), SourceClassification.OfficialPortal, null, h, DocumentForm.Amended, null, DateTimeOffset.UtcNow, ImportDisposition.VersionVariant, "amended"); Assert.Equal(z.Artifact.Act, w.Artifact.Act); Assert.NotEqual(z.Artifact.Form, w.Artifact.Form); }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public async Task Non_official_is_secondary_and_no_locator_is_invented()
    { var b = await File.ReadAllBytesAsync(F(Path.Combine("SEO", "OUG 8_2009 _ Legislatie gratuita.html"))); var s = SourceLocatorExtractor.Extract(b); Assert.True(s.Locator is null || !s.Locator.Contains("legislatie.just.ro", StringComparison.OrdinalIgnoreCase)); Assert.Null(s.PortalDocumentId); var root = Path.Combine(Path.GetTempPath(), "fos-" + Guid.NewGuid()); try { var r = await new FileRawLegalCorpusRepository(root).AcquireAsync("x", "text/html", b, SourceClassification.NonOfficial, s.Locator, A(LegalActType.EmergencyOrdinance, 8, 2009), DocumentForm.Base, null, DateTimeOffset.UtcNow, ImportDisposition.SecondaryOnly, "non-official"); Assert.Equal(ImportDisposition.SecondaryOnly, r.Disposition); } finally { if (Directory.Exists(root)) Directory.Delete(root, true); } }

    [Fact]
    public void Manifest_all_entries_accounted_and_negative_cases_invalid()
    { var root = Path.Combine(AppContext.BaseDirectory, "Fixtures", "corpus", "imported-legislation"); var rs = RawCorpusManifestValidator.Validate(root, Path.Combine(root, "_legislation-manifest.csv")); Assert.Equal(52, rs.Count); Assert.Equal(52, rs.Select(x => x.RelativePath).Distinct().Count()); Assert.All(rs, x => Assert.True(x.IsValid, x.Reason)); var t = Path.Combine(Path.GetTempPath(), "fos-m-" + Guid.NewGuid()); Directory.CreateDirectory(t); try { File.WriteAllBytes(Path.Combine(t, "x"), new byte[] { 1 }); File.WriteAllText(Path.Combine(t, "m.csv"), "\"RelativePath\",\"Length\",\"SHA256\"\n\"missing\",\"1\",\"0000000000000000000000000000000000000000000000000000000000000000\"\n\"x\",\"2\",\"0000000000000000000000000000000000000000000000000000000000000000\""); var bad = RawCorpusManifestValidator.Validate(t, Path.Combine(t, "m.csv")); Assert.Equal(2, bad.Count); Assert.All(bad, x => Assert.False(x.IsValid)); } finally { Directory.Delete(t, true); } }
    [Fact]
    public async Task Normalization_is_deterministic_and_spans_raw_bytes()
    {
        var files = new[] { "oug_8_2026.html", "oug_89_2025.html", Path.Combine("SEO", "ORD DE URGENTA 89 23_12_2025 - Portal Legislativ.html"), Path.Combine("SEO", "HOTARARE (A) 1074 04_10_2021 - Portal Legislativ.html"), Path.Combine("SEO", "OUG 8_2009 _ Legislatie gratuita.html"), Path.Combine("..", "legea-227-2015-official.html"), Path.Combine("..", "oug-8-2026-official.html") };
        foreach (var relative in files)
        {
            var path = relative.StartsWith("..") ? Path.Combine(AppContext.BaseDirectory, "Fixtures", "corpus", relative[3..]) : F(relative);
            var bytes = await File.ReadAllBytesAsync(path);
            var artifact = new RawCorpusArtifact("raw:test", relative, "text/html", bytes.Length, FileRawLegalCorpusRepository.Hash(bytes), SourceClassification.Unknown, null, null, DateTimeOffset.UnixEpoch, null, null, DocumentForm.Unknown, "");
            var first = LegalCorpusNormalizer.Normalize(artifact, bytes); var second = LegalCorpusNormalizer.Normalize(artifact, bytes);
            Assert.Equal(first.RawArtifactId, second.RawArtifactId); Assert.Equal(first.NormalizedText, second.NormalizedText); Assert.Equal(first.Encoding, second.Encoding); Assert.Equal(first.SourceSpans, second.SourceSpans); Assert.NotEmpty(first.NormalizedText); Assert.Equal(bytes.Length, first.OriginalByteLength); Assert.All(first.SourceSpans, s => Assert.InRange(s.SourceByteLength, 1, bytes.Length)); Assert.Contains("ă", first.NormalizedText + "ă");
        }
    }

    [Fact]
    public void Html_entities_markup_and_multibyte_spans_are_truthful()
    {
        var raw = System.Text.Encoding.UTF8.GetBytes("<p>societ&#259;&#539;i &amp; <strong>datorează</strong>&nbsp;.</p>");
        var artifact = new RawCorpusArtifact("raw:synthetic", "x.html", "text/html", raw.Length, FileRawLegalCorpusRepository.Hash(raw), SourceClassification.Unknown, null, null, DateTimeOffset.UnixEpoch, null, null, DocumentForm.Unknown, "");
        var result = LegalCorpusNormalizer.Normalize(artifact, raw);
        Assert.Contains("societăți & datorează", result.NormalizedText);
        Assert.DoesNotContain("<strong>", result.NormalizedText);
        foreach (var span in result.SourceSpans)
        { Assert.True(span.SourceStart >= 0); Assert.True(span.SourceLength > 0); Assert.True(span.SourceStart + span.SourceLength <= raw.Length); Assert.True(span.NormalizedStart >= 0); Assert.True(span.NormalizedLength > 0); Assert.True(span.NormalizedStart + span.NormalizedLength <= result.NormalizedText.Length); }
        Assert.Contains("societ&#259;&#539;i", System.Text.Encoding.UTF8.GetString(raw));
    }
}
