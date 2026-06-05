using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class LegalReferenceResolutionAuditEngine : ILegalReferenceResolutionAuditEngine
{
    private readonly ILegalReferenceResolutionAuditPipeline _pipeline;
    private readonly Func<DateTimeOffset> _timestampProvider;

    public LegalReferenceResolutionAuditEngine(ILegalReferenceResolutionAuditPipeline pipeline)
        : this(pipeline, () => DateTimeOffset.UtcNow)
    {
    }

    public LegalReferenceResolutionAuditEngine(
        ILegalReferenceResolutionAuditPipeline pipeline,
        Func<DateTimeOffset> timestampProvider)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(timestampProvider);

        _pipeline = pipeline;
        _timestampProvider = timestampProvider;
    }

    public async Task<ResolutionAuditTrail> AuditAsync(
        IReadOnlyList<ResolutionResult> results,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(results);

        var context = new LegalReferenceResolutionAuditContext(Array.Empty<ResolutionAuditEntry>());
        var mappingStage = new ResolutionResultAuditEntryStage(results, _timestampProvider);
        var mapped = await mappingStage.ExecuteAsync(context, cancellationToken);
        var processed = await _pipeline.RunAsync(mapped, cancellationToken);

        return new ResolutionAuditTrail(processed.Entries);
    }
}
