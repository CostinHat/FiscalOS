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

## Audit
- Model
- Repository Contract
- Pipeline Contract
- Engine Contract

## Provenance
- Model
- Repository Contract
- Pipeline Contract
- Engine Contract

## Evidence Package
- Model
- Repository Contract
- Pipeline Contract
- Engine Contract

## Status
Resolution, Audit, Provenance and Evidence Package are now complete as separate
domain/contract verticals, each following Model -> Repository Contract ->
Pipeline Contract -> Engine Contract.

Evidence Package composes the ResolutionResult, ResolutionAuditTrail and
ResolutionProvenance concerns without replacing or merging those verticals.
All four verticals remain contract-only at this layer: no production
implementation, runtime behavior or persistence behavior is prescribed by the
domain contracts.

## Next
- FOS-0057 Legal Reference Resolution Engine Implementation
