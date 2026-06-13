using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class DecideRawDocumentIdentityStage : ILegislationIngestionStage
{
    private readonly LegislationSourceId _sourceId;
    private readonly IRawDocumentIdentityDecisionPolicy _decisionPolicy;
    private readonly Func<DateTimeOffset> _timestampProvider;

    public DecideRawDocumentIdentityStage(LegislationSourceId sourceId)
        : this(
            sourceId,
            new DeterministicRawDocumentIdentityDecisionPolicy(),
            () => DateTimeOffset.UtcNow)
    {
    }

    public DecideRawDocumentIdentityStage(
        LegislationSourceId sourceId,
        IRawDocumentIdentityDecisionPolicy decisionPolicy,
        Func<DateTimeOffset> timestampProvider)
    {
        ArgumentNullException.ThrowIfNull(sourceId);
        ArgumentNullException.ThrowIfNull(decisionPolicy);
        ArgumentNullException.ThrowIfNull(timestampProvider);

        _sourceId = sourceId;
        _decisionPolicy = decisionPolicy;
        _timestampProvider = timestampProvider;
    }

    public IngestionStage Stage => IngestionStage.RawDocumentIdentity;

    public Task<IngestionContext> ExecuteAsync(
        IngestionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var decisions = context.Documents.Select(document =>
            _decisionPolicy.Decide(document, _sourceId, _timestampProvider())).ToList();
        var trace = context.Trace.Concat(new[]
        {
            new IngestionTraceEntry(
                Stage,
                IngestionStatus.Succeeded,
                _timestampProvider(),
                $"Decided identity for {context.Documents.Count} raw legislation document(s)."),
        }).ToList();

        return Task.FromResult(context with
        {
            RawDocumentIdentityDecisions = context.RawDocumentIdentityDecisions.Concat(decisions).ToList(),
            Trace = trace,
        });
    }
}
