using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegislationIngestion;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Raw_Legislation_Document_Repository_Contract
{
    // Minimal in-memory test double used only to validate the contract shape and
    // semantics. It is not a production implementation (no filesystem, database,
    // blob storage, or DI), per FOS-0033 scope.
    private sealed class InMemoryRawLegislationDocumentRepository : IRawLegislationDocumentRepository
    {
        private readonly Dictionary<string, RawLegislationDocument> _store = new();

        public Task StoreAsync(RawLegislationDocument document, CancellationToken cancellationToken = default)
        {
            _store[document.Id.Value] = document;
            return Task.CompletedTask;
        }

        public Task<RawLegislationDocument?> GetAsync(LegislationDocumentId id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.TryGetValue(id.Value, out var document) ? document : null);
        }
    }

    private static RawLegislationDocument Document(string id) =>
        new(new LegislationDocumentId(id),
            new LegislationSourceReference("Monitorul Oficial 1/2026"),
            "raw legislative text");

    [Fact]
    public async Task Stored_document_can_be_retrieved_by_id()
    {
        IRawLegislationDocumentRepository repository = new InMemoryRawLegislationDocumentRepository();
        var document = Document("DOC-1");

        await repository.StoreAsync(document);
        var retrieved = await repository.GetAsync(new LegislationDocumentId("DOC-1"));

        Assert.Equal(document, retrieved);
    }

    [Fact]
    public async Task Unknown_id_returns_null()
    {
        IRawLegislationDocumentRepository repository = new InMemoryRawLegislationDocumentRepository();

        var retrieved = await repository.GetAsync(new LegislationDocumentId("MISSING"));

        Assert.Null(retrieved);
    }
}
