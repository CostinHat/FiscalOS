using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class ValidateRawLegislationDocumentsStage : ILegislationIngestionStage
{
    private readonly Func<DateTimeOffset> _timestampProvider;

    public ValidateRawLegislationDocumentsStage()
        : this(() => DateTimeOffset.UtcNow)
    {
    }

    public ValidateRawLegislationDocumentsStage(Func<DateTimeOffset> timestampProvider)
    {
        ArgumentNullException.ThrowIfNull(timestampProvider);

        _timestampProvider = timestampProvider;
    }

    public IngestionStage Stage => IngestionStage.Normalization;

    public Task<IngestionContext> ExecuteAsync(
        IngestionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var document in context.Documents)
        {
            Validate(document);
        }

        var trace = context.Trace.Concat(new[]
        {
            new IngestionTraceEntry(
                Stage,
                IngestionStatus.Succeeded,
                _timestampProvider(),
                $"Validated {context.Documents.Count} raw legislation document(s)."),
        }).ToList();

        return Task.FromResult(context with { Trace = trace });
    }

    private static void Validate(RawLegislationDocument document)
    {
        if (document is null)
        {
            throw new InvalidOperationException("Raw legislation document is required.");
        }

        if (document.Id is null)
        {
            throw new InvalidOperationException("Raw legislation document identifier is required.");
        }

        if (document.Source is null)
        {
            throw new InvalidOperationException($"Raw legislation document '{document.Id}' source identifier is required.");
        }

        if (document.Content is null)
        {
            throw new InvalidOperationException($"Raw legislation document '{document.Id}' content is required.");
        }

        if (document.Content.Length == 0)
        {
            throw new InvalidOperationException($"Raw legislation document '{document.Id}' content cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(document.Content))
        {
            throw new InvalidOperationException($"Raw legislation document '{document.Id}' content cannot be whitespace.");
        }
    }
}
