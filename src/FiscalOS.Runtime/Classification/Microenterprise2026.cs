using FiscalOS.LegalCore;

namespace FiscalOS.Runtime.Classification;

public enum FactStatus { Unknown, True, False }
public enum ExcludedActivityStatus { Unknown, ConfirmedNotExcluded, ConfirmedExcluded }
public enum Microenterprise2026Outcome { Eligible, NotEligible, InsufficientInformation, Unsupported }

public sealed record Microenterprise2026Facts(
    decimal? EntityRevenueLei, IReadOnlyList<decimal>? LinkedRevenueLei,
    decimal? EurExchangeRate, FactStatus RomanianLegalEntity, FactStatus CapitalEligible,
    FactStatus NotInDissolutionLiquidation, FactStatus HasEmployee,
    FactStatus SingleMicroDesignation, FactStatus FinancialStatementsTimely,
    ExcludedActivityStatus ExcludedActivity);

public sealed record Microenterprise2026Knowledge(
    decimal EurLimit, DateOnly FactsAsOf, int FiscalYear, DateOnly KnowledgeDate,
    string CanonicalAct, string PrimaryProvision, string ModifyingAct,
    string EvidenceSource, string EvidenceUrl, bool Curated);

public sealed record Microenterprise2026Response(
    Microenterprise2026Outcome Outcome, IReadOnlyDictionary<string, FactStatus> Conditions,
    string Explanation, Microenterprise2026Knowledge Knowledge);

public sealed class Microenterprise2026Application
{
    public Microenterprise2026Application(Microenterprise2026Knowledge knowledge)
        => Knowledge = knowledge ?? throw new ArgumentNullException(nameof(knowledge));
    public Microenterprise2026Knowledge Knowledge { get; }

    public Microenterprise2026Response Evaluate(Microenterprise2026Facts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        var c = new Dictionary<string, FactStatus>
        {
            ["C1_REVENUE"] = Revenue(facts), ["C2_CAPITAL"] = facts.CapitalEligible,
            ["C3_DISSOLUTION"] = facts.NotInDissolutionLiquidation, ["C4_EMPLOYEE"] = facts.HasEmployee,
            ["C5_SINGLE_DESIGNATION"] = facts.SingleMicroDesignation, ["C6_FINANCIAL_STATEMENTS"] = facts.FinancialStatementsTimely,
            ["ROMANIAN_ENTITY"] = facts.RomanianLegalEntity,
        };
        if (facts.ExcludedActivity == ExcludedActivityStatus.ConfirmedExcluded) return Result(Microenterprise2026Outcome.NotEligible, c, "Activitatea este confirmată ca exclusă de o regulă juridică suportată.");
        if (facts.ExcludedActivity == ExcludedActivityStatus.Unknown) return Result(Microenterprise2026Outcome.Unsupported, c, "Lista completă a excluderilor art. 47 alin. (3) nu este în corpusul VS02; statutul activității este necunoscut.");
        if (c.Values.Any(v => v == FactStatus.False)) return Result(Microenterprise2026Outcome.NotEligible, c, "Cel puțin o condiție cumulativă este demonstrat nesatisfăcută.");
        if (c.Values.Any(v => v == FactStatus.Unknown)) return Result(Microenterprise2026Outcome.InsufficientInformation, c, "Lipsesc fapte necesare pentru o concluzie pozitivă.");
        return Result(Microenterprise2026Outcome.Eligible, c, "Toate condițiile VS02 suportate sunt satisfăcute la 31.12.2025 pentru anul fiscal 2026.");
    }

    private FactStatus Revenue(Microenterprise2026Facts f)
    {
        if (f.EntityRevenueLei is null || f.EurExchangeRate is null || f.EurExchangeRate <= 0 || f.LinkedRevenueLei is null) return FactStatus.Unknown;
        var total = f.EntityRevenueLei.Value + f.LinkedRevenueLei.Sum();
        return total <= Knowledge.EurLimit * f.EurExchangeRate.Value ? FactStatus.True : FactStatus.False;
    }
    private Microenterprise2026Response Result(Microenterprise2026Outcome o, IReadOnlyDictionary<string, FactStatus> c, string e) => new(o, c, $"{e} Legal basis: {Knowledge.CanonicalAct}, {Knowledge.PrimaryProvision}; valid for facts as of {Knowledge.FactsAsOf:yyyy-MM-dd}, knowledge time {Knowledge.KnowledgeDate:yyyy-MM-dd}; source {Knowledge.EvidenceSource} ({Knowledge.EvidenceUrl}); curated={Knowledge.Curated}.", Knowledge);
}
