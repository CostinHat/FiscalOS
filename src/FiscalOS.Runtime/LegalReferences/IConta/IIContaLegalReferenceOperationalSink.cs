namespace FiscalOS.Runtime.LegalReferences.IConta;

public interface IIContaLegalReferenceOperationalSink
{
    void Record(IContaLegalReferenceOperationalEvent operationalEvent);
}
