namespace FiscalOS.Core.Rules;

public interface IRule
{
    EvaluationResult Evaluate(IEnumerable<ObservedFact> facts);
}