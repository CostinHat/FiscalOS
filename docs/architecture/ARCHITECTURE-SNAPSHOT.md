# FiscalOS Architecture Snapshot

## Principles
- Fact != Legal != Derived != Decision
- Every decision must be auditable
- Every conclusion must have provenance

## Atom Model
- Fact
- Legal
- Derived
- Decision
- Practice
- Authority

## Graphs
- LegalGraph
- PracticeGraph
- PurposeGraph
- AuditGraph

## Resolution
- Model
- Repository Contract
- Pipeline Contract
- Engine Contract
- Runtime Engine Implementation

## Audit
- Model
- Repository Contract
- Pipeline Contract
- Engine Contract
- Runtime Engine Implementation

## Provenance
- Model
- Repository Contract
- Pipeline Contract
- Engine Contract
- Runtime Engine Implementation

## Evidence Package
- Model
- Repository Contract
- Pipeline Contract
- Engine Contract
- Runtime Engine Implementation

## Status
Resolution, Audit, Provenance and Evidence Package are now complete as separate
domain/contract verticals, each following Model -> Repository Contract ->
Pipeline Contract -> Engine Contract.

Evidence Package composes the ResolutionResult, ResolutionAuditTrail and
ResolutionProvenance concerns without replacing or merging those verticals.

FOS-0057 through FOS-0060 added first runtime implementations for all four
verticals:
- Resolution: runtime facade over a resolution pipeline, with a repository-backed
  stage for stored outcomes and unresolved misses.
- Audit: runtime facade over an audit pipeline, producing audit trails from
  resolution outcomes.
- Provenance: runtime facade over a provenance pipeline, producing ordered
  provenance chains from provenance records.
- Evidence Package: runtime facade over an evidence package pipeline, processing
  packages composed from ResolutionResult, ResolutionAuditTrail and
  ResolutionProvenance.

Architecture review conclusions:
- Accept FOS-0057 through FOS-0060 as-is for this milestone.
- Dependency direction is correct: Runtime depends on Domain contracts; Domain
  does not depend on Runtime.
- Separation of concerns is mostly preserved; repository-backed persistence is
  isolated behind repository contracts and pipeline stages.
- Architectural debt remains: audit result-to-entry mapping lives in the audit
  engine rather than a dedicated audit stage.
- Architectural debt remains: evidence package composition exists as a runtime
  helper rather than a formal composition contract.
- Ambiguous-result query derivation currently uses the first candidate and should
  be documented or strengthened before richer ambiguity handling is added.

## Next
- FOS-0061 Legal Reference Resolution Runtime Architecture Hardening
