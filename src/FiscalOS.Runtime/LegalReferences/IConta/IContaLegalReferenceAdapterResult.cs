namespace FiscalOS.Runtime.LegalReferences.IConta;

public sealed record IContaLegalReferenceAdapterResult(
    IContaLegalReferenceResponse? Response,
    IContaLegalReferenceApplicationError? Error)
{
    public bool IsSuccess => Response is not null && Error is null;

    public static IContaLegalReferenceAdapterResult Success(IContaLegalReferenceResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new IContaLegalReferenceAdapterResult(response, Error: null);
    }

    public static IContaLegalReferenceAdapterResult Failure(IContaLegalReferenceApplicationError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new IContaLegalReferenceAdapterResult(Response: null, error);
    }
}
