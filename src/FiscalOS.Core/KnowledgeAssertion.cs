namespace FiscalOS.Core;

public sealed record KnowledgeAssertion(string ConceptCode, object Value, IReadOnlyCollection<ObservedFact> Evidence);
