namespace FiscalOS.Domain.LegislationIngestion;

public interface ILegislationIngestionPipeline
{
    Task<IngestionResult> RunAsync(IngestionContext context, CancellationToken cancellationToken = default);
}
