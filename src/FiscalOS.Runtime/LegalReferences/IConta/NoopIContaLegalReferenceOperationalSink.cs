namespace FiscalOS.Runtime.LegalReferences.IConta;

public sealed class NoopIContaLegalReferenceOperationalSink : IIContaLegalReferenceOperationalSink
{
    public void Record(IContaLegalReferenceOperationalEvent operationalEvent)
    {
        ArgumentNullException.ThrowIfNull(operationalEvent);
    }
}
