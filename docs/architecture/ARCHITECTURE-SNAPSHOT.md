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
- Runtime Repository Implementation

## Audit
- Model
- Repository Contract
- Pipeline Contract
- Engine Contract
- Runtime Engine Implementation
- Runtime Repository Implementation

## Provenance
- Model
- Repository Contract
- Pipeline Contract
- Engine Contract
- Runtime Engine Implementation
- Runtime Repository Implementation

## Evidence Package
- Model
- Repository Contract
- Pipeline Contract
- Engine Contract
- Runtime Engine Implementation
- Runtime Repository Implementation

## Runtime Composition
- End-to-end LegalReference -> ResolutionEvidencePackage facade

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
- Architectural debt remains: evidence package composition exists as a runtime
  helper rather than a formal composition contract.
- FOS-0061 moved audit result-to-entry mapping into a dedicated runtime audit
  stage while preserving Domain contracts and immutable models.
- FOS-0061 documented ambiguous-result query derivation through executable
  tests; the current convention remains first-candidate based.
- FOS-0061 strengthened ResolutionEvidencePackageComposer failure-mode coverage.
- Evidence package composition remains a runtime helper; no composition contract
  has been introduced yet.
- FOS-0062 added deterministic in-memory runtime repositories behind the
  existing repository contracts for Resolution, Audit, Provenance and Evidence
  Package.
- FOS-0063 added an end-to-end runtime composition facade that accepts
  LegalReference queries and returns ResolutionEvidencePackage outputs.
- Runtime composition wires the existing engines, pipelines, stages,
  repositories and ResolutionEvidencePackageComposer.
- Dependency direction remains Runtime -> Domain.
- No durable persistence, ingestion, graph integration, AI/NLP, classification
  coupling or richer resolution/search/source algorithm has been introduced.

## Next
- FOS-0064 Legal Reference Resolution Runtime Composition Review
