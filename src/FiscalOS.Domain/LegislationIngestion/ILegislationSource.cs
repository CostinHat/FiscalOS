using System.Collections.Generic;

namespace FiscalOS.Domain.LegislationIngestion;

public interface ILegislationSource
{
    Task<IReadOnlyList<RawLegislationDocument>> FetchAsync(CancellationToken cancellationToken = default);
}
