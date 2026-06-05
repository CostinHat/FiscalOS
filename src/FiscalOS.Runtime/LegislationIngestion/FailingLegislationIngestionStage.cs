using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class FailingLegislationIngestionStage : ILegislationIngestionStage
{
    private readonly string _reason;

    public FailingLegislationIngestionStage(
        IngestionStage stage,
        string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Failure reason cannot be empty.", nameof(reason));
        }

        Stage = stage;
        _reason = reason.Trim();
    }

    public IngestionStage Stage { get; }

    public Task<IngestionContext> ExecuteAsync(
        IngestionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        throw new InvalidOperationException(_reason);
    }
}
