using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FiscalOS.LegalKnowledge.Corpus;

public enum StructuralAtomType { Act, Title, Chapter, Section, Article, Paragraph, Letter, Point }
public enum AtomizationError { None, InvalidNormalizedMaterial, UnsupportedStructure, NoStructuralAtoms, InvalidHierarchy }
public sealed record StructuralAtom(string StructuralAtomId, StructuralAtomType AtomType, string Designation, string? ParentAtomId, int Ordinal, int NormalizedStart, int NormalizedLength, string AtomizerVersion, IReadOnlyList<NormalizedSourceSpan> SourceSpans);
public sealed record StructuralLegalDocument(string AtomizerVersion, IReadOnlyList<StructuralAtom> Atoms, AtomizationError Error = AtomizationError.None, string? ErrorMessage = null);

public static class LegalStructuralAtomizer
{
    public const string Version = "corpus-atomizer-v1";
    private static readonly Regex Article = new(@"(?im)^\s*(?:ART(?:ICOLUL)?\.?\s+)(?<n>\d+[A-Za-z]?)\s*[.—-]?(?<body>.*)$", RegexOptions.Compiled);
    private static readonly Regex Paragraph = new(@"(?m)^\s*\((?<n>\d+[\^]?\d*)\)\s+(?<body>.+)$", RegexOptions.Compiled);
    private static readonly Regex Letter = new(@"(?m)^\s*(?<n>[a-zăâîșț])\)\s+(?<body>.+)$", RegexOptions.Compiled);
    private static readonly Regex Point = new(@"(?m)^\s*(?<n>\d+)\.\s+(?<body>.+)$", RegexOptions.Compiled);

    public static StructuralLegalDocument Atomize(NormalizedLegalMaterial material)
    {
        if (material.Error != NormalizationError.None) return new(Version, Array.Empty<StructuralAtom>(), AtomizationError.InvalidNormalizedMaterial, material.ErrorMessage);
        if (string.IsNullOrWhiteSpace(material.NormalizedText)) return new(Version, Array.Empty<StructuralAtom>(), AtomizationError.NoStructuralAtoms, "Normalized content is empty.");
        var atoms = new List<StructuralAtom>();
        var root = Create(material, StructuralAtomType.Act, "ACT", null, 0, 0, material.NormalizedText.Length);
        atoms.Add(root);
        var articles = Article.Matches(material.NormalizedText).Cast<Match>().ToList();
        foreach (var (match, index) in articles.Select((m, i) => (m, i)))
        {
            var end = index + 1 < articles.Count ? articles[index + 1].Index : material.NormalizedText.Length;
            var article = Create(material, StructuralAtomType.Article, match.Groups["n"].Value, root.StructuralAtomId, index, match.Index, Math.Max(1, end - match.Index));
            atoms.Add(article);
            var segment = material.NormalizedText.Substring(match.Index, end - match.Index);
            foreach (Match child in Paragraph.Matches(segment))
            {
                var childStart = match.Index + child.Index;
                var paragraph = Create(material, StructuralAtomType.Paragraph, child.Groups["n"].Value, article.StructuralAtomId, child.Index, childStart, child.Length);
                atoms.Add(paragraph);
                foreach (Match letter in Letter.Matches(child.Groups["body"].Value))
                    atoms.Add(Create(material, StructuralAtomType.Letter, letter.Groups["n"].Value, paragraph.StructuralAtomId, letter.Index, childStart + child.Groups["body"].Index + letter.Index, letter.Length));
                foreach (Match point in Point.Matches(child.Groups["body"].Value))
                    atoms.Add(Create(material, StructuralAtomType.Point, point.Groups["n"].Value, paragraph.StructuralAtomId, point.Index, childStart + child.Groups["body"].Index + point.Index, point.Length));
            }
        }
        return atoms.Count == 1 ? new(Version, atoms, AtomizationError.NoStructuralAtoms, "No article structure detected.") : new(Version, atoms);
    }

    private static StructuralAtom Create(NormalizedLegalMaterial material, StructuralAtomType type, string designation, string? parent, int ordinal, int start, int length)
    {
        var key = $"{material.RawArtifactId}|{material.NormalizationVersion}|{Version}|{type}|{designation}|{start}|{length}";
        var id = "atom:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant();
        var span = material.SourceSpans.Count == 0 ? Array.Empty<NormalizedSourceSpan>() : material.SourceSpans;
        return new(id, type, designation, parent, ordinal, start, length, Version, span);
    }
}
