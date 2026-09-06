using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using FiscalOS.Runtime.LegalReferences;

namespace FiscalOS.Runtime.VerticalSlice;

public sealed record LegalReferenceVersion(string VersionId, string CanonicalAct, string Provision, string ModifyingAct, DateOnly ValidFrom, DateTimeOffset KnowledgeTime, string SourceIdentity, string SourceUrl, string IssuingAuthority, string SourceHash, string RawContent, bool Curated);
public sealed record VerticalResolutionResult(ResolutionEvidencePackage Package, LegalReferenceVersion Version, string Explanation);
public sealed record LegalReferenceApplicationResponse(string Status, string? CanonicalAct, string? Provision, string? VersionId, string? SourceHash, DateOnly? ValidFrom, DateTimeOffset? KnowledgeTime, bool Curated, string? Explanation, ResolutionEvidencePackage? Evidence);
public interface ILegalReferenceApplication
{ Task<LegalReferenceApplicationResponse> ResolveAsync(string query, DateOnly validAt, DateTimeOffset knownAt, CancellationToken cancellationToken = default); }

public sealed class LegalReferenceVerticalSlice : ILegalReferenceApplication
{
    private readonly string _path;
    private readonly ILegalReferenceResolutionRepository _repository;
    private readonly LegalReferenceResolutionRuntime _runtime;
    public const string QueryText = "Legea nr. 227/2015, art. 7";
    public LegalReferenceVerticalSlice(string path, ILegalReferenceResolutionRepository repository, LegalReferenceResolutionRuntime runtime) { _path = path; _repository = repository; _runtime = runtime; }
    public Task AcquireNormalizeAndCurateAsync(string content, DateTimeOffset knowledgeTime, CancellationToken ct = default) => IngestVersionAsync("before-2021", content, new DateOnly(2016, 1, 1), knowledgeTime, "", true, ct);
    public async Task IngestVersionAsync(string id, string content, DateOnly validFrom, DateTimeOffset knowledgeTime, string modifyingAct, bool curated, CancellationToken ct = default)
    { if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Raw source is required.", nameof(content)); var vs = await LoadAsync(ct); if (vs.Any(v => v.VersionId == id)) return; var v = new LegalReferenceVersion(id, "RO-CF-227-2015", "art. 7 pct. 18", modifyingAct, validFrom, knowledgeTime, "ANAF-INFO-2021-01-13", "https://static.anaf.ro/static/10/Iasi/material_informativ-2_14-01-2021.pdf", "ANAF / D.G.R.F.P. Iași — Biroul Asistență pentru Contribuabili", Hash(content), content, curated); Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!); await File.AppendAllTextAsync(_path, JsonSerializer.Serialize(v) + Environment.NewLine, ct); if (curated) await SeedAsync(v, ct); }
    public async Task ValidateIntegrityAsync(CancellationToken ct = default) { foreach (var v in await LoadAsync(ct)) if (Hash(v.RawContent) != v.SourceHash) throw new InvalidDataException($"Source hash mismatch for {v.VersionId}."); }
    public async Task<VerticalResolutionResult?> ResolveAtAsync(DateOnly validAt, DateTimeOffset knownAt, CancellationToken ct = default) { await ValidateIntegrityAsync(ct); var v = (await LoadAsync(ct)).Where(x => x.Curated && x.KnowledgeTime <= knownAt && x.ValidFrom <= validAt).OrderByDescending(x => x.ValidFrom).FirstOrDefault(); if (v is null) return null; await SeedAsync(v, ct); var p = (await _runtime.ResolveAsync(new[] { new LegalReference(new[] { new ReferenceSegment("article", "7") }) }, ct)).Single(); return new(p, v, $"{v.CanonicalAct}, {v.Provision}, valid from {v.ValidFrom:yyyy-MM-dd}; known at {v.KnowledgeTime:O}; curated={v.Curated}; source {v.SourceIdentity} hash {v.SourceHash}; resolution/audit/provenance/evidence linked."); }
    public async Task<LegalReferenceApplicationResponse> ResolveAsync(string query, DateOnly validAt, DateTimeOffset knownAt, CancellationToken cancellationToken = default)
    { if (string.IsNullOrWhiteSpace(query) || !query.Contains("art. 7", StringComparison.OrdinalIgnoreCase)) return new("Unresolved", null, null, null, null, null, null, false, "Malformed or unknown legal reference.", null); var r = await ResolveAtAsync(validAt, knownAt, cancellationToken); return r is null ? new("Unresolved", null, null, null, null, null, null, false, "No curated material available.", null) : new("Resolved", r.Version.CanonicalAct, r.Version.Provision, r.Version.VersionId, r.Version.SourceHash, r.Version.ValidFrom, r.Version.KnowledgeTime, r.Version.Curated, r.Explanation, r.Package); }
    public Task<IReadOnlyList<LegalReferenceVersion>> VersionsAsync(CancellationToken ct = default) => LoadAsync(ct).ContinueWith(t => (IReadOnlyList<LegalReferenceVersion>)t.Result, ct);
    public static string Hash(string content) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
    private async Task SeedAsync(LegalReferenceVersion v, CancellationToken ct) { var q = new LegalReference(new[] { new ReferenceSegment("article", "7") }); var r = new FullyQualifiedLegalReference(new LegalDocumentReference(v.CanonicalAct), q); await _repository.StoreAsync(q, new ResolutionResult(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(r) }, null), ct); }
    private async Task<List<LegalReferenceVersion>> LoadAsync(CancellationToken ct) { if (!File.Exists(_path)) return new(); var lines = await File.ReadAllLinesAsync(_path, ct); return lines.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => JsonSerializer.Deserialize<LegalReferenceVersion>(x)!).ToList(); }
}
