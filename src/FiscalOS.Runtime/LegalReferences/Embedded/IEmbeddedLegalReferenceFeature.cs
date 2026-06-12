namespace FiscalOS.Runtime.LegalReferences.Embedded;

public interface IEmbeddedLegalReferenceFeature
{
    Task<EmbeddedLegalReferenceResponse> ResolveAsync(
        EmbeddedLegalReferenceRequest request,
        CancellationToken cancellationToken = default);
}
