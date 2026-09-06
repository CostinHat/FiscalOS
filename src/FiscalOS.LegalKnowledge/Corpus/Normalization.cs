using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace FiscalOS.LegalKnowledge.Corpus;

public enum NormalizationError { None, UnsupportedFormat, UnsupportedEncoding, InvalidDocument, EmptyNormalizedContent }
public sealed record NormalizedSourceSpan(string RawArtifactId, long SourceStart, long SourceLength, int NormalizedStart, int NormalizedLength)
{
    public long SourceByteLength => SourceLength;
}
public sealed record NormalizedLegalMaterial(string RawArtifactId, long OriginalByteLength, string NormalizedText, string Encoding, string NormalizationVersion, IReadOnlyList<NormalizedSourceSpan> SourceSpans, NormalizationError Error = NormalizationError.None, string? ErrorMessage = null);

public static class LegalCorpusNormalizer
{
    public const string Version = "corpus-normalizer-v1";
    private static readonly Regex Tags = new("<[^>]+>", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex NonContent = new("<(script|style|noscript)[^>]*>[\\s\\S]*?</\\1>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static NormalizedLegalMaterial Normalize(RawCorpusArtifact artifact, ReadOnlySpan<byte> bytes)
    {
        if (artifact.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase) || artifact.OriginalFileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return Failure(artifact, NormalizationError.UnsupportedFormat, "PDF text extraction is not enabled.");
        if (!TryDecode(bytes, out var text, out var encoding))
            return Failure(artifact, NormalizationError.UnsupportedEncoding, "Input is not valid UTF-8.");
        var content = NonContent.Replace(text, "");
        var matches = Regex.Matches(content, @">(?<text>[^<]+)<", RegexOptions.Singleline);
        var pieces = new List<(string Text, int RawCharStart)>();
        foreach (Match match in matches)
            if (!string.IsNullOrWhiteSpace(match.Groups["text"].Value)) pieces.Add((match.Groups["text"].Value, match.Groups["text"].Index));
        var normalized = WebUtility.HtmlDecode(Tags.Replace(content, ""));
        normalized = Regex.Replace(normalized.Replace("\r\n", "\n").Replace('\r', '\n'), "[ \t]+", " ");
        normalized = Regex.Replace(normalized, "\n{3,}", "\n\n").Trim();
        if (normalized.Length == 0) return Failure(artifact, NormalizationError.EmptyNormalizedContent, "No normalized content.");
        var spans = new List<NormalizedSourceSpan>();
        var normalizedCursor = 0;
        foreach (var piece in pieces)
        {
            var decoded = WebUtility.HtmlDecode(piece.Text);
            var start = normalized.IndexOf(decoded, normalizedCursor, StringComparison.Ordinal);
            if (start < 0) continue;
            var rawStart = Encoding.UTF8.GetByteCount(content[..piece.RawCharStart]);
            spans.Add(new(artifact.RawArtifactId, rawStart, Encoding.UTF8.GetByteCount(piece.Text), start, decoded.Length));
            normalizedCursor = start + decoded.Length;
        }
        if (spans.Count == 0) spans.Add(new(artifact.RawArtifactId, 0, bytes.Length, 0, normalized.Length));
        return new(artifact.RawArtifactId, bytes.Length, normalized, encoding, Version, spans);
    }

    private static bool TryDecode(ReadOnlySpan<byte> bytes, out string text, out string encoding)
    {
        encoding = "UTF-8"; var offset = bytes.StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }) ? 3 : 0;
        try { text = new UTF8Encoding(false, true).GetString(bytes[offset..]); return true; }
        catch (DecoderFallbackException) { text = ""; return false; }
    }

    private static NormalizedLegalMaterial Failure(RawCorpusArtifact a, NormalizationError e, string message) =>
        new(a.RawArtifactId, a.ByteLength, "", "Unknown", Version, Array.Empty<NormalizedSourceSpan>(), e, message);
}
