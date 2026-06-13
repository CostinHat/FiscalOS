using FiscalOS.LegalCore;

namespace FiscalOS.LegalKnowledge;

public static class SourceAuthority
{
    public static SourceAuthorityLevel LevelFor(LegalSourceType sourceType) =>
        sourceType switch
        {
            LegalSourceType.Law                => SourceAuthorityLevel.Statutory,
            LegalSourceType.EmergencyOrdinance => SourceAuthorityLevel.Statutory,
            LegalSourceType.FiscalCode         => SourceAuthorityLevel.Statutory,
            LegalSourceType.GovernmentDecision => SourceAuthorityLevel.Government,
            LegalSourceType.MethodologicalNorm => SourceAuthorityLevel.Government,
            LegalSourceType.ANAFOrder          => SourceAuthorityLevel.Administrative,
            LegalSourceType.Other              => SourceAuthorityLevel.Unknown,
            _ => throw new ArgumentOutOfRangeException(nameof(sourceType), sourceType, null)
        };
}
