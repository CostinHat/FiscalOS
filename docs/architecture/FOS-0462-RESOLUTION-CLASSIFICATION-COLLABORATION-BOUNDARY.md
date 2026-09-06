# FOS-0462 Resolution-Classification Collaboration Boundary Review and Acceptance

Status: accepted.

Related: [AR-02.md](AR-02.md),
[ingestion-to-resolution-boundaries.md](ingestion-to-resolution-boundaries.md),
[repository-and-consumer-boundaries.md](repository-and-consumer-boundaries.md)

## Objective

Define the permitted architecture-contract handoff from Legal Reference
Resolution to Classification while preserving AR-02: Resolution and
Classification are separate first-class capabilities with independent
responsibilities, contracts, and traceability.

This review is documentation only. It does not add a runtime coupling.

## Decision

The permitted handoff is explicit, read-only downstream consumption of a
Resolution output by a Classification-adjacent composition or consumer:

```text
Legal Reference Resolution output
  -> optional Classification context / downstream composition
```

The handoff does not make Resolution a classification rule source, a legal-basis
resolver, or an owner of Classification outcomes. Classification does not become
a Resolution policy, candidate-selection, or correction mechanism.

## Resolution Responsibilities and Permitted Outputs

Legal Reference Resolution owns:

- structural legal-reference query normalization;
- resolution policy and deterministic attempts;
- `ResolutionResult` status, candidates, resolved reference when exactly one
  candidate exists, and unresolved query;
- resolved, ambiguous, and unresolved outcomes;
- additive corrections and supersession;
- `ResolutionAuditTrail` and `ResolutionProvenance`;
- `ResolutionEvidencePackage`, which composes those independent contracts
  without merging them.

Resolution may provide to a downstream Classification context or consumer:

- `ResolutionResult`, including its status and structural
  `FullyQualifiedLegalReference` candidates;
- `ResolutionEvidencePackage`, while retaining its `Result`, `AuditTrail`, and
  `Provenance` as separate values;
- explicit correlation to the provided output or evidence package.

Resolution must not provide or derive a fiscal category, Classification result,
classification rule, `DecisionLegalBasis`, fiscal conclusion, or generated rule.

## Classification Responsibilities and Permitted Consumption

Classification owns:

- classification rules and rule evaluation;
- `ClassificationResult`, winning-rule selection, and `ClassificationDecision`;
- `DecisionExplanation` and its composition of the separate
  `DecisionLegalBasis` and `AuditGraph` contracts;
- its own legal-basis resolution from classification-rule citations.

Classification may consume a supplied Resolution output only as structural
reference context and traceable correlation data. It may expose that supplied
context through a downstream explanation or projection only when the source
Resolution output and its separate assurance contracts remain identifiable.

Classification must not:

- treat `ResolutionResult` as `DecisionLegalBasis` or as a `LegalCitation`;
- select a candidate from an ambiguous Resolution result;
- treat an unresolved result as a resolved reference or as absence of legal
  basis;
- convert Resolution status or candidates into a fiscal category, rule outcome,
  or fiscal/legal conclusion;
- alter Resolution attempts, policies, audit trails, provenance, corrections,
  or supersession.

## Outcome and Traceability Rules

`Resolved`, `Ambiguous`, and `Unresolved` are first-class Resolution outcomes.
Their meanings are preserved across the handoff. If Classification receives an
ambiguous or unresolved output, it may report that context faithfully but cannot
silently replace it with a candidate, legal basis, rule result, or classification
conclusion.

Traceability remains independently owned:

- `ResolutionResult` remains the Resolution outcome contract.
- `DecisionLegalBasis` remains the Classification legal-basis contract.
- `AuditGraph` remains the Classification audit/traceability contract.
- `ResolutionAuditTrail` and `ResolutionProvenance` remain Resolution contracts.
- `ResolutionEvidencePackage` remains a composition of Resolution contracts.
- Classification outcome and explanation contracts remain Classification
  contracts.

A consumer may correlate these values, but it must not flatten them into a new
canonical record, overwrite their identities, or make one contract a substitute
for another.

## Dependency and Composition Boundary

Any future collaboration must be initiated by explicit Runtime or application
composition. Domain contracts remain independent of Runtime, and neither
capability gains a dependency on the other's execution engine, repository, or
internal policy implementation.

This review does not authorize direct calls from `ClassificationEngine` to a
Legal Reference Resolution engine, repository, or pipeline.

## Non-Goals

- No runtime redesign or runtime API change.
- No production code or test change.
- No persistence, public API, ingestion, AI/NLP, graph infrastructure,
  traversal, visualization, or rule generation.
- No resolution-to-classification rule binding.
- No merged result, legal-basis, audit, provenance, evidence, or classification
  outcome contract.

## Review Result

The boundary is coherent with AR-02. Resolution may provide structural,
status-preserving, independently traceable output to Classification-adjacent
composition, while Classification remains independently responsible for
deterministic fiscal/legal decisions and explanations.

## Acceptance

FOS-0462 is accepted.

- Resolution and Classification remain separate first-class capabilities.
- The handoff is one-way, explicit, and read-only.
- `ResolutionResult` and `ResolutionEvidencePackage` remain context only.
- Classification cannot select ambiguous candidates or convert unresolved
  outcomes into legal basis, rule results, or conclusions.
- Resolution and Classification contract ownership remains separate.
- Assurance/Traceability remains transversal and independently traceable.
- No runtime coupling, production code, runtime API, persistence, public API,
  AI/NLP, graph infrastructure, ingestion, or rule generation was introduced.
