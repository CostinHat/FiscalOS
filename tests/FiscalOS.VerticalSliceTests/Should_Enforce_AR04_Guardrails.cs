using System.IO;
using FiscalOS.LegalCore;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Enforce_AR04_Guardrails
{
    [Fact]
    public void Valid_and_knowledge_time_are_distinct_and_deterministic()
    {
        var valid = new ValidTime(new TemporalInterval(
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), null));
        var known = new KnowledgeTime(new TemporalInterval(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), null));
        var validity = new BiTemporalValidity(valid, known);

        Assert.True(validity.IsApplicableAt(new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)));
        Assert.False(validity.WasKnownAt(new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)));
        Assert.True(validity.WasKnownAt(new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero)));
        Assert.NotEqual(valid.Start, known.Start);
    }

    [Fact]
    public void Temporal_interval_rejects_reversed_bounds()
    {
        Assert.Throws<ArgumentException>(() => new TemporalInterval(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void Canonical_projects_have_no_consumer_project_reference()
    {
        var root = RepositoryRoot();
        foreach (var project in new[] { "FiscalOS.LegalCore", "FiscalOS.Domain", "FiscalOS.LegalKnowledge", "FiscalOS.Runtime" })
        {
            var path = Path.Combine(root, "src", project, project + ".csproj");
            var content = File.ReadAllText(path);
            Assert.DoesNotContain("IConta", content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("iConta", content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Consumer", content, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string RepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "FiscalOS.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }
}
