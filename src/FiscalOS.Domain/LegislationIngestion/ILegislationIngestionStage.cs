namespace FiscalOS.Domain.LegislationIngestion;

public interface ILegislationIngestionStage
{
    IngestionStage Stage { get; }

    Task<IngestionContext> ExecuteAsync(IngestionContext context, CancellationToken cancellationToken = default);
}
