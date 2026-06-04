namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// A single step in a legal reference pipeline. A stage transforms a
/// <see cref="LegalReferenceContext"/> and returns the updated context; it is a
/// pure structural transformation contract with no implementation prescribed.
/// </summary>
public interface ILegalReferenceStage
{
    string Name { get; }

    Task<LegalReferenceContext> ExecuteAsync(LegalReferenceContext context, CancellationToken cancellationToken = default);
}
