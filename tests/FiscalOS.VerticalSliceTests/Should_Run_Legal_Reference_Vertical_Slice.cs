using FiscalOS.Domain.LegalReferences;
using FiscalOS.Runtime.LegalReferences;
using FiscalOS.Runtime.VerticalSlice;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Legal_Reference_Vertical_Slice
{
    [Fact]
    public async Task Should_Persist_Reload_And_Resolve_Article_Seven()
    {
        var path = Temp(); try { var text = await Fixture("art7-pct18-before.txt"); var a = Create(path); await a.IngestVersionAsync("before", text, new(2016, 1, 1), new(2020, 1, 1, 0, 0, 0, TimeSpan.Zero), "", true); var first = await a.ResolveAtAsync(new(2020, 12, 31), new(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)); var b = Create(path); var second = await b.ResolveAtAsync(new(2020, 12, 31), new(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)); Assert.Equal("before", first!.Version.VersionId); Assert.Equal(first.Package.Result, second!.Package.Result, new ResultComparer()); var response = await b.ResolveAsync(LegalReferenceVerticalSlice.QueryText, new(2020, 12, 31), new(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)); Assert.Equal("Resolved", response.Status); Assert.Equal("art. 7 pct. 18", response.Provision); } finally { Delete(path); }
    }

    [Fact]
    public async Task Should_Replay_Before_And_After_And_Preserve_History()
    { var path = Temp(); try { var before = await Fixture("art7-pct18-before.txt"); var after = await Fixture("art7-pct18-after.txt"); Assert.Equal("CF63BA3B3E2FE0DBF5036F65237104C71B1FD64A9AE076FE3B291AAAB1A937F8", LegalReferenceVerticalSlice.Hash(before)); Assert.Equal("054AFCA1A459ADF19A344442E928C79BD5A60F7A2A7FFCF00CB19D0106F48D89", LegalReferenceVerticalSlice.Hash(after)); var a = Create(path); await a.IngestVersionAsync("before", before, new(2016, 1, 1), new(2020, 12, 31, 0, 0, 0, TimeSpan.Zero), "", true); var old = (await a.ResolveAtAsync(new(2020, 12, 31), new(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)))!; Assert.Equal("before", old.Version.VersionId); Assert.NotNull(old.Package.AuditTrail); Assert.NotNull(old.Package.Provenance); await a.IngestVersionAsync("after", after, new(2021, 1, 1), new(2021, 1, 2, 0, 0, 0, TimeSpan.Zero), "Legea nr. 296/2020 art. I pct. 1", true); var b = Create(path); await b.ValidateIntegrityAsync(); Assert.Equal("before", (await b.ResolveAtAsync(new(2020, 12, 31), new(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)))!.Version.VersionId); var newer = (await b.ResolveAtAsync(new(2021, 1, 1), new(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)))!; Assert.Equal("after", newer.Version.VersionId); Assert.NotNull(newer.Package.Provenance); Assert.Equal(2, (await b.VersionsAsync()).Count); } finally { Delete(path); } }

    [Fact]
    public async Task Should_Block_Uncurated_And_Detect_Corruption()
    { var path = Temp(); try { var text = await Fixture("art7-pct18-before.txt"); var a = Create(path); await a.IngestVersionAsync("raw", text, new(2016, 1, 1), DateTimeOffset.UtcNow, "", false); Assert.Null(await a.ResolveAtAsync(new(2020, 1, 1), DateTimeOffset.UtcNow)); await a.IngestVersionAsync("curated", text, new(2016, 1, 1), DateTimeOffset.UtcNow, "", true); var json = await File.ReadAllTextAsync(path); await File.WriteAllTextAsync(path, json.Replace("locul", "CORRUPT", StringComparison.Ordinal)); await Assert.ThrowsAsync<InvalidDataException>(() => a.ValidateIntegrityAsync()); } finally { Delete(path); } }

    private static string Temp() => Path.Combine(Path.GetTempPath(), $"fos-vs01-{Guid.NewGuid():N}.json");
    private static void Delete(string p) { if (File.Exists(p)) File.Delete(p); }
    private static async Task<string> Fixture(string n) => await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", n));
    private static LegalReferenceVerticalSlice Create(string p) { var s = new ServiceCollection(); s.AddLegalReferenceResolutionRuntime(); var x = s.BuildServiceProvider(); return new(p, x.GetRequiredService<ILegalReferenceResolutionRepository>(), x.GetRequiredService<LegalReferenceResolutionRuntime>()); }
    private sealed class ResultComparer : IEqualityComparer<ResolutionResult> { public bool Equals(ResolutionResult? x, ResolutionResult? y) => x?.Status == y?.Status && x?.ResolvedReference == y?.ResolvedReference; public int GetHashCode(ResolutionResult obj) => HashCode.Combine(obj.Status, obj.ResolvedReference); }
}
