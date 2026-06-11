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

## Legislation Ingestion
- Foundation Model
- Raw Document Repository Contract
- Pipeline Contract
- Source Acquisition Contract
- Runtime Source Implementation
- Runtime Raw Document Repository Implementation
- Runtime Pipeline Implementation
- Runtime Composition

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
- FOS-0064 reviewed runtime composition and accepted it as-is.
- Remaining debt: no composition builder or DI registration exists yet.
- Remaining debt: no formal Evidence Package composition contract exists yet.
- Remaining debt: no durable persistence exists yet.
- Remaining debt: no richer legal reference matching, search or source algorithm
  exists yet.
- No durable persistence, ingestion, graph integration, AI/NLP, classification
  coupling or richer resolution/search/source algorithm has been introduced.

FOS-0065 added the first runtime implementation behind the Legislation Ingestion
contracts:
- In-memory legislation source.
- In-memory raw legislation document repository.
- Acquisition, storage and deterministic failure stages.
- Sequential ingestion pipeline with succeeded and failed result handling.
- Runtime-only, in-memory and contract-backed behavior.
- No external network acquisition, filesystem/database persistence, graph
  integration, AI/NLP, classification coupling or Legal Reference Resolution
  integration has been introduced.
- FOS-0066 reviewed the Legislation Ingestion runtime and accepted FOS-0065
  as-is.
- Remaining debt: no ingestion runtime composition facade exists yet.
- Remaining debt: no ingestion builder or DI registration exists yet.
- Remaining debt: no stage implementations exist yet for discovery, versioning,
  normalization, citation detection, candidate extraction, human review or rule
  binding.
- Remaining debt: no batch lifecycle store, document validation stage,
  duplicate/version policy or ingestion summary output exists yet.
- FOS-0067 added a Runtime-only ingestion composition facade that accepts an
  IngestionBatchId and returns an IngestionResult.
- Ingestion runtime composition wires the existing source, raw document
  repository, acquisition/storage stages and pipeline.
- Ingestion runtime composition remains in-memory, deterministic and
  contract-backed.
- No external network acquisition, durable persistence, filesystem/database
  storage, graph integration, AI/NLP, classification coupling or Legal Reference
  Resolution integration has been introduced.
- FOS-0068 reviewed Legislation Ingestion runtime composition and accepted it
  as-is.
- Remaining debt: no ingestion builder or DI registration exists yet.
- Remaining debt: no document validation stage exists yet.
- Remaining debt: no stage implementations exist yet for discovery, versioning,
  normalization, citation detection, candidate extraction, human review or rule
  binding.
- FOS-0069 added a Runtime-only validation stage that inspects
  IngestionContext.Documents before storage.
- Valid documents add a success trace.
- Invalid content fails through the existing runtime pipeline failure behavior.
- Runtime validation preserves Domain contracts and Runtime -> Domain
  dependency direction.
- No durable persistence, network acquisition, AI/NLP, graph integration,
  Legal Reference integration or classification coupling has been introduced.
- FOS-0070 added a Runtime-only discovery stage that emits a deterministic
  success trace before acquisition.
- Runtime discovery does not mutate IngestionContext.Documents.
- Runtime discovery preserves Domain contracts and Runtime -> Domain
  dependency direction.
- No durable persistence, network acquisition, AI/NLP, graph integration,
  Legal Reference integration or classification coupling has been introduced.
- FOS-0071 was skipped/reframed because the proposed acquisition scope is
  already implemented by AcquireLegislationDocumentsStage and the current
  runtime wiring.
- No acquisition hardening review has been recorded yet.
- FOS-0072 added a Runtime-only versioning stage that runs after validation and
  before storage.
- Runtime order is Discovery -> Acquisition -> Validation -> Versioning ->
  Storage.
- All ingestion runtime stages remain Runtime-only, in-memory, and
  contract-backed.
- Runtime versioning does not mutate document content.
- No durable persistence, network acquisition, AI/NLP, graph integration,
  Legal Reference integration or classification coupling has been introduced.
- Post-FOS-0073 roadmap review is complete.
- The ingestion runtime vertical is considered complete for the current
  architecture milestone line.
- Additional ingestion hardening milestones are lower priority than starting
  the next major vertical.
- FOS-0071 and FOS-0073 were skipped because the proposed functionality already
  existed in the runtime implementation.
- FOS-0074 added a ServiceCollection-based DI registration module for the
  Legal Reference resolution runtime graph.
- FOS-0074 was accepted as-is.
- Runtime composition remains Runtime-only and Domain-backed.
- FOS-0075 wired LegalReferenceResolutionRuntime into FiscalOS.Api through API
  composition.
- FOS-0075 was accepted as-is.
- The API host now resolves the Legal Reference runtime graph through the
  shared composition path.
- FOS-0076 closed the host-exposure line.
- The team decided not to expose a Legal Reference endpoint now.
- The API host remains a composition boundary rather than a Legal Reference
  feature surface.
- FOS-0077 reviewed the FiscalOS.Api composition boundary and accepted it
  as-is.
- The API host is coherent as a thin composition boundary with a shared
  registration helper and dedicated endpoint mapping.
- FOS-0078 kept the current endpoint surface.
- The API surface remains a single root route plus the existing microenterprise
  endpoint.
- No Legal Reference endpoint was introduced.
- FOS-0079 kept FiscalOS.Api as a thin host for now.
- The future strategy is modular endpoint groups only when a public endpoint
  is justified.
- FOS-0080 deferred endpoint modules until additional public API surfaces
  exist.
- The current endpoint organization remains sufficient for the current public
  surface.
- FOS-0082 separated platform capabilities from product capabilities.
- Legal Reference Resolution remains the strongest future product exposure
  candidate.
- Legislation Ingestion remains an internal platform capability.
- FOS-0083 confirmed FiscalOS should primarily power iConta workflows rather
  than be marketed independently at this stage.
- FOS-0083 confirmed Legal Reference Resolution has strong accountant workflow
  fit.
- FOS-0083 confirmed Legislation Ingestion remains backend/internal
  infrastructure.
- FOS-0084 recommends Legal Reference Resolution as an embedded iConta
  feature, not a standalone FiscalOS feature at this stage.
- FOS-0084 keeps the internal pipeline, audit, provenance, evidence,
  repositories, DI and ingestion internals hidden.
- FOS-0084 flags the risk of presenting legal basis lookup as full legal
  advice or guaranteed compliance.
- FOS-0085 recommends implementing Legal Reference Resolution as an embedded
  iConta workflow action.
- FOS-0085 defines the workflow shape as ask -> resolve -> cite -> explain.
- FOS-0085 reiterates that the feature is decision support, not legal advice
  or guaranteed compliance.
- FOS-0086 defines the embedded feature version-1 shape:
  request = LegalReference query + correlationId.
- FOS-0086 defines the embedded feature version-1 shape:
  response = status + answer + citation + explanation + correlationId +
  optional traceability summary.
- FOS-0086 keeps internal audit, provenance, evidence, repository and DI
  details hidden.

## Next
- FOS-0087 Embedded Feature UX Copy Review
