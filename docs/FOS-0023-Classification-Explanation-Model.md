\# FOS-0023 Classification Explanation Model

> **Historical / superseded for runtime ownership.** This document does not
> define the current runtime explanation surface. The current canonical
> composition is `ClassificationDecision -> DecisionExplanation ->
> (DecisionLegalBasis, AuditGraph)`. `ExplanationGraph` is a dormant generic
> Core projection shape, not a live decision explanation contract.



\## Purpose



Definește modelul de explicație care justifică rezultatul clasificării.



\## Goal



Permite trasabilitate completă între:



\- Facts

\- Rules

\- Conclusions



\## Historical Core Concepts (superseded for runtime ownership)



\### ExplanationGraph



Reprezintă explicația completă.



\### ExplanationNode



Tipuri:



\- Fact

\- Rule

\- Conclusion



\### ExplanationEdge



Leagă două noduri.



\## Example



Revenue = 320000

EmployeeCount = 2



↓



Rule MIC-001



↓



Category = Microenterprise



\## Explanation Result



\### Summary



Explicație scurtă pentru utilizator.



\### Detailed Explanation



Explicație completă pentru audit.



\## Historical Relationships (superseded for runtime ownership)

The relationships below are historical and do not define the current runtime
composition. Current ownership is `ClassificationDecision ->
DecisionExplanation -> (DecisionLegalBasis, AuditGraph)`.



ExplanationGraph

→ RuleEvaluationResult



ExplanationGraph

→ ClassificationResult



ExplanationGraph

→ EvaluationContext



\## Audit Support



Explicația trebuie să permită reconstrucția completă a rezultatului.



\## Future Extensions



\- Visualization

\- Graph Export

\- Natural Language Explanation



\## Acceptance Criteria



\- Explanation model definit

\- Node model definit

\- Edge model definit

\- Audit traceability definită

