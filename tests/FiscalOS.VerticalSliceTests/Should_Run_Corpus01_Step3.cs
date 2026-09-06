using FiscalOS.LegalKnowledge.Corpus;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Corpus01_Step3
{
    [Fact]
    public async Task Real_corpus_atomization_is_deterministic_and_hierarchical()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "corpus", "legea-227-2015-official.html");
        var bytes = await File.ReadAllBytesAsync(path);
        var raw = new RawCorpusArtifact("raw:law", "law.html", "text/html", bytes.Length, FileRawLegalCorpusRepository.Hash(bytes), SourceClassification.OfficialPortal, null, "171280", DateTimeOffset.UnixEpoch, null, null, DocumentForm.Base, "");
        var normalized = LegalCorpusNormalizer.Normalize(raw, bytes);
        var first = LegalStructuralAtomizer.Atomize(normalized);
        var second = LegalStructuralAtomizer.Atomize(normalized);
        Assert.Equal(first.Error, second.Error);
        Assert.Equal(first.AtomizerVersion, second.AtomizerVersion);
        Assert.Equal(first.Atoms.Select(a => a.StructuralAtomId), second.Atoms.Select(a => a.StructuralAtomId));
        Assert.Equal(AtomizationError.None, first.Error);
        Assert.Single(first.Atoms.Where(a => a.AtomType == StructuralAtomType.Act));
        Assert.Contains(first.Atoms, a => a.AtomType == StructuralAtomType.Article && a.Designation == "7");
        Assert.Contains(first.Atoms, a => a.AtomType == StructuralAtomType.Article && a.Designation == "47");
        Assert.Contains(first.Atoms, a => a.AtomType == StructuralAtomType.Article && a.Designation == "48");
        var article = first.Atoms.First(a => a.AtomType == StructuralAtomType.Article && a.Designation == "47");
        var paragraph = first.Atoms.First(a => a.AtomType == StructuralAtomType.Paragraph && a.ParentAtomId == article.StructuralAtomId);
        Assert.True(paragraph.NormalizedStart >= article.NormalizedStart);
        Assert.True(paragraph.NormalizedStart + paragraph.NormalizedLength <= article.NormalizedStart + article.NormalizedLength);
        Assert.NotEmpty(paragraph.SourceSpans);
        Assert.Equal(paragraph.StructuralAtomId, second.Atoms.First(a => a.AtomType == StructuralAtomType.Paragraph && a.ParentAtomId == second.Atoms.First(x => x.StructuralAtomId == article.StructuralAtomId).StructuralAtomId).StructuralAtomId);
        foreach (var atom in first.Atoms)
        {
            Assert.InRange(atom.NormalizedStart, 0, normalized.NormalizedText.Length - 1);
            Assert.InRange(atom.NormalizedLength, 1, normalized.NormalizedText.Length - atom.NormalizedStart);
            if (atom.ParentAtomId is not null) Assert.Contains(first.Atoms, p => p.StructuralAtomId == atom.ParentAtomId);
        }
    }

    [Fact]
    public void Atomizer_does_not_treat_plain_numbers_as_structure()
    {
        var raw = new RawCorpusArtifact("raw:x", "x", "text/plain", 20, "x", SourceClassification.Unknown, null, null, DateTimeOffset.UnixEpoch, null, null, DocumentForm.Unknown, "");
        var material = new NormalizedLegalMaterial("raw:x", 20, "Text obișnuit 1. și (2) fără articol.", "UTF-8", LegalCorpusNormalizer.Version, Array.Empty<NormalizedSourceSpan>());
        var result = LegalStructuralAtomizer.Atomize(material);
        Assert.Equal(AtomizationError.NoStructuralAtoms, result.Error);
    }
}
