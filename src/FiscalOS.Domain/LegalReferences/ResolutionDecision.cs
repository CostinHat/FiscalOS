using FiscalOS.LegalCore;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// The disposition recorded for a resolution: its <see cref="ResolutionStatus"/>
/// and, when resolved, the <see cref="FullyQualifiedLegalReference"/> that was
/// selected. A resolved decision must select a reference; any other status must
/// not. Pure audit data; no resolution logic.
/// </summary>
public sealed record ResolutionDecision
{
    public ResolutionStatus Status { get; }

    public FullyQualifiedLegalReference? SelectedReference { get; }

    public ResolutionDecision(ResolutionStatus status, FullyQualifiedLegalReference? selectedReference = null)
    {
        if (status == ResolutionStatus.Resolved && selectedReference is null)
        {
            throw new ArgumentException("A resolved decision must select a reference.", nameof(selectedReference));
        }

        if (status != ResolutionStatus.Resolved && selectedReference is not null)
        {
            throw new ArgumentException("Only a resolved decision can select a reference.", nameof(selectedReference));
        }

        Status = status;
        SelectedReference = selectedReference;
    }
}
