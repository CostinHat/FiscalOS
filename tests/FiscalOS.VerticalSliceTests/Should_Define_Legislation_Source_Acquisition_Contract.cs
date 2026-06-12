using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegislationIngestion;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legislation_Source_Acquisition_Contract
{
    // Test-only double used to validate the contract shape and semantics.
    // Not a production implementation (no HTTP client or external integration),
    // per FOS-0035 scope.
    private sealed class StubLegislationSource : ILegislationSource
    {
        private readonly IReadOnlyList<RawLegislationDocument> _documents;

        public StubLegislationSource(params RawLegislationDocument[] documents)
        {
            Id = new LegislationSourceId("stub-legislation-source");
            Metadata = new LegislationSourceMetadata(Id, "Stub legislation source", "stub");
            _documents = documents;
        }

        public LegislationSourceId Id { get; }

        public LegislationSourceMetadata Metadata { get; }

        public Task<IReadOnlyList<RawLegislationDocument>> FetchAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_documents);
        }
    }

    private static RawLegislationDocument Document(string id) =>
        new(new LegislationDocumentId(id),
            new LegislationSourceReference("Monitorul Oficial 1/2026"),
            "raw legislative text");

    [Fact]
    public async Task Fetch_returns_the_source_documents()
    {
        ILegislationSource source = new StubLegislationSource(Document("DOC-1"), Document("DOC-2"));

        var documents = await source.FetchAsync();

        Assert.Equal(2, documents.Count);
        Assert.Equal("DOC-1", documents[0].Id.Value);
        Assert.Equal("DOC-2", documents[1].Id.Value);
    }

    [Fact]
    public async Task Source_with_no_documents_returns_empty()
    {
        ILegislationSource source = new StubLegislationSource();

        var documents = await source.FetchAsync();

        Assert.Empty(documents);
    }

    [Fact]
    public void Source_exposes_stable_identity()
    {
        ILegislationSource source = new StubLegislationSource(Document("DOC-1"));

        Assert.Equal("stub-legislation-source", source.Id.Value);
        Assert.Same(source.Id, source.Metadata.Id);
    }

    [Fact]
    public void Source_exposes_metadata()
    {
        ILegislationSource source = new StubLegislationSource(Document("DOC-1"));

        Assert.Equal("Stub legislation source", source.Metadata.DisplayName);
        Assert.Equal("stub", source.Metadata.SourceType);
    }

    [Fact]
    public async Task Source_identity_is_distinct_from_document_source_reference()
    {
        ILegislationSource source = new StubLegislationSource(Document("DOC-1"));

        var documents = await source.FetchAsync();

        Assert.Equal("stub-legislation-source", source.Id.Value);
        Assert.Equal("Monitorul Oficial 1/2026", documents[0].Source.Value);
        Assert.NotEqual(source.Id.Value, documents[0].Source.Value);
    }
}
