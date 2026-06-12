using System.Collections.Generic;

namespace FiscalOS.Domain.LegislationIngestion;

public interface ILegislationSource
{
    LegislationSourceId Id { get; }

    LegislationSourceMetadata Metadata { get; }

    Task<IReadOnlyList<RawLegislationDocument>> FetchAsync(CancellationToken cancellationToken = default);
}
