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
- Traceability composition is a public-safe downstream projection from internal
  resolution/evidence output.
- Traceability summaries are composed at the Embedded Feature / iConta boundary,
  not inside the Runtime resolution core.
- Runtime remains responsible for internal outcome determination and evidence
  package composition.
- Public-safe traceability projection model:
  ResolutionEvidencePackage + correlationId + requested-reference ->
  TraceabilitySummary.
- Public-safe TraceabilitySummary may include correlationId,
  requested-reference identity, status, resolved citation when available, source
  summary, linkage text and limitations.
- correlationId links the request and response but is not sufficient by itself
  for legal traceability.
- requested-reference identity is required in public traceability summaries.
- Ambiguous traceability has no selected citation and must remain anchored to
  the original requested reference.
- Unresolved traceability has no citation and must not imply that the law does
  not exist.
- Provenance chains, audit trails, evidence package internals, repositories, DI,
  pipeline/stage names and graph internals remain hidden from public
  traceability summaries.
- Known debt: ambiguous-result first-candidate linkage remains an internal
  convention and must not become public traceability behavior.
- FOS-0211 accepted the traceability architecture track as internally
  consistent and compatible with Embedded Feature v1.
- FOS-0212 accepted the TraceabilitySummary v1 shape:
  correlationId, requested-reference identity, status, optional resolved
  citation, optional source summary, linkage text and limitations.
- FOS-0213 accepted public traceability wording with constraints: factual,
  accountant-friendly, citation-backed and non-advisory.
- FOS-0214 found the traceability track ready for implementation planning.
- FOS-0215 limited the first delivery to public-safe traceability projection.
- FOS-0216 added public-safe traceability DTOs and
  LegalReferenceTraceabilityProjector.
- Runtime traceability projection accepts ResolutionEvidencePackage,
  correlationId and requested-reference input, and returns TraceabilitySummary.
- Projection preserves resolved, ambiguous and unresolved statuses.
- Projection emits a citation only for resolved outcomes.
- Ambiguous and unresolved projection outputs include no selected citation.
- Public traceability DTOs do not expose provenance, audit, evidence package,
  repository, DI, pipeline or graph internals.
- Ambiguous projection uses the original requested reference rather than
  first-candidate linkage.
- FOS-0217 accepted the traceability projection implementation as-is.
- FOS-0218 accepted Runtime placement for the first projection slice while
  preserving Embedded Feature / iConta as the long-term public composition
  owner.
- FOS-0219 defined embedded integration as request correlationId plus original
  LegalReference flowing into optional TraceabilitySummary projection.
- FOS-0220 accepted the embedded request/response contract shape for
  traceability integration.
- FOS-0221 planned the embedded boundary as DTOs plus a narrow facade over
  LegalReferenceResolutionRuntime and LegalReferenceTraceabilityProjector.
- FOS-0222 added EmbeddedLegalReferenceRequest,
  EmbeddedLegalReferenceResponse and EmbeddedLegalReferenceFeature.
- EmbeddedLegalReferenceFeature validates and trims correlationId, preserves the
  original requested LegalReference, resolves through LegalReferenceResolutionRuntime
  and optionally projects TraceabilitySummary.
- Embedded responses expose status, answer, citation, explanation,
  correlationId and optional traceabilitySummary.
- TraceabilitySummary is included only when IncludeTraceability is true.
- Resolved embedded responses include citation; ambiguous and unresolved
  embedded responses expose no selected citation.
- Embedded response DTOs keep provenance, audit, evidence package, repository,
  DI, pipeline and graph internals hidden.
- FOS-0223 accepted the embedded traceability boundary implementation as-is.
- FOS-0224 accepted singleton DI registration for
  LegalReferenceTraceabilityProjector and EmbeddedLegalReferenceFeature.
- FOS-0225 registered LegalReferenceTraceabilityProjector and
  EmbeddedLegalReferenceFeature in the existing runtime composition path.
- DI registration does not add a Legal Reference API endpoint or expand product
  integration.
- EmbeddedLegalReferenceFeature depends only on LegalReferenceResolutionRuntime
  and LegalReferenceTraceabilityProjector.
- Direct repository, pipeline, audit, provenance, evidence package, graph,
  persistence, ingestion or AI/NLP dependencies were not introduced into the
  embedded facade.
- FOS-0226 accepted the embedded traceability DI registration as-is.
- FOS-0227 deferred public endpoint exposure.
- Embedded traceability remains DI-wired but unexposed.
- The embedded boundary remains the active integration surface for traceability.
- FOS-0228 concluded future API exposure requires API-owned request and
  response DTOs.
- Internal Runtime and Domain DTOs must not be reused directly as public API
  contracts.
- Future API exposure must preserve public-safety constraints: no provenance,
  audit, evidence, repository, DI, pipeline or graph internals in public
  contracts.
- Required preconditions before endpoint implementation:
  API contract review, validation review, error behavior review, route review
  and versioning review.
- FOS-0230 accepted the endpoint exposure documentation update.
- FOS-0231 defined future public API contract requirements for Legal Reference
  traceability.
- Future public API exposure must use API-owned request and response DTOs.
- Internal Runtime and Domain DTOs must not be reused directly as public API
  contracts.
- FOS-0232 defined validation and public error behavior for a future endpoint.
- Validation must occur before Runtime or Domain object construction.
- correlationId, reference.segments and explicit includeTraceability are
  required for a future public request.
- Unresolved and ambiguous results are successful domain outcomes, not HTTP
  errors.
- Public error responses must use public-safe error codes and messages.
- FOS-0233 defined route and versioning policy for future endpoint exposure.
- Recommended future route: POST /v1/legal-references/resolve.
- API owns route, versioning, DTOs, validation and error behavior.
- API versioning is independent from Runtime and Domain versioning.
- Additive changes are allowed within v1 only when backward-compatible.
- Breaking public contract changes require a new API version.
- Internal architecture terms must not appear in routes, schemas, errors or
  public documentation.
- Hidden-internal boundary for API exposure remains: no provenance, audit,
  evidence, repository, DI, pipeline or graph internals in public contracts.
- FOS-0235 accepted the API contract documentation update.
- FOS-0236 concluded endpoint exposure remains deferred.
- Technical API contract readiness is not sufficient for endpoint
  implementation without product, authorization and operational decisions.
- FOS-0237 recommended staged product-surface exposure:
  internal iConta capability first, partner-facing API later and public FiscalOS
  API last.
- Current target surface is internal iConta capability.
- Partner-facing and public API exposure remain deferred.
- FOS-0238 concluded authorization remains outside Runtime and Domain.
- Internal iConta usage requires authenticated and authorized workflow context.
- Tenant, account and workflow scoping is required before invoking the embedded
  feature.
- correlationId is linkage metadata, not authorization.
- FOS-0239 concluded operational ownership remains outside Runtime and Domain.
- Internal iConta usage requires defined ownership, logging, monitoring,
  support and incident policies.
- Operational surfaces may use public-safe metadata only.
- Public-safety constraints for operational surfaces remain: provenance, audit,
  evidence, repository, DI, pipeline and graph internals stay hidden.
- Hidden-internal boundary requirements apply regardless of authorization level
  or operational role.
- FOS-0241 accepted the exposure and operations documentation update.
- FOS-0242 approved internal iConta rollout planning but not rollout execution.
- Embedded capability is technically ready for internal planning.
- Internal rollout execution remains blocked pending authorization and
  operational policy acceptance.
- FOS-0243 defined the internal rollout plan.
- Adapter boundary belongs outside Runtime.
- Adapter responsibilities include translating iConta workflow requests into
  LegalReference input, supplying correlationId, setting IncludeTraceability,
  invoking EmbeddedLegalReferenceFeature and mapping the response to iConta UI
  or application models.
- Authorization handoff must occur before invoking EmbeddedLegalReferenceFeature.
- Required authorization context includes authenticated user/session,
  tenant/account/company/workspace and workflow/action context.
- Logging expectations include correlationId, tenant/account/workflow metadata
  when allowed, status, IncludeTraceability, success/failure classification and
  adapter-boundary latency.
- Monitoring expectations include request count, status distribution,
  authorization denial count, validation failure count, latency,
  traceability-requested count and unexpected exception count.
- Support ownership remains split: iConta owns workflow/user-facing issues,
  iConta operations owns rollout health, FiscalOS maintainers own Runtime
  defects and product/legal review owns wording or disclaimer concerns.
- Incident-management expectations include handling authorization/scoping
  failures, cross-tenant visibility concerns, misleading citation display,
  elevated unresolved/ambiguous rates, runtime failures, logging/privacy issues
  and public-safety wording issues.
- Rollout acceptance requires adapter design, authorization handoff,
  tenant/account/workflow scoping, logging policy, monitoring metrics, support
  owner, incident escalation path, accepted user-facing wording and continued
  endpoint deferment.
- Execution blockers remain: no adapter implementation plan, no authorization
  handoff design, no logging/monitoring policy, no support/incident owner and
  no accepted iConta user-facing wording.
- Endpoint exposure remains deferred.
- FOS-0245 accepted the internal rollout documentation update.
- FOS-0246 defined the iConta adapter contract.
- Adapter ownership remains outside Runtime and Domain.
- Input mapping responsibilities: translate iConta workflow context into
  EmbeddedLegalReferenceRequest, convert workflow legal-reference input into
  LegalReference, preserve requested-reference identity, supply correlationId
  and set IncludeTraceability.
- Output mapping responsibilities: map EmbeddedLegalReferenceResponse into
  iConta-owned presentation models, preserve status, show citation only when
  resolved, preserve ambiguous/unresolved no-citation behavior and avoid
  exposing Runtime DTOs directly to UI or API surfaces.
- Authorization handoff must occur before EmbeddedLegalReferenceFeature
  invocation and must include authenticated user/session,
  tenant/account/company/workspace, workflow/action context and permission
  decision.
- correlationId handling remains the adapter responsibility; it is linkage
  metadata only and not authorization.
- Traceability inclusion remains the adapter responsibility through
  IncludeTraceability.
- Logging and monitoring metadata are owned by the adapter/application boundary.
- Failure and validation handling belongs at the adapter boundary: missing or
  invalid workflow references, missing or blank correlationId, authorization
  denial, embedded feature exceptions and Runtime failures must be mapped to
  public-safe application errors.
- Runtime remains unaware of workflow, tenant, account, user, authorization,
  logging, monitoring and support concerns.
- Hidden-internal boundary is preserved: provenance, audit, evidence,
  repository, DI, pipeline and graph internals remain hidden.
- FOS-0248 accepted the adapter contract documentation update.
- FOS-0249 planned the iConta adapter implementation slice.
- FOS-0250 added IContaLegalReferenceAdapter and iConta-owned adapter DTOs.
- iConta adapter DTOs model request segments, authorization context,
  operational metadata, presentation response data, public-safe application
  errors and operational events.
- IContaLegalReferenceAdapter validates input and authorization before invoking
  EmbeddedLegalReferenceFeature.
- Unauthorized and invalid requests do not invoke EmbeddedLegalReferenceFeature.
- IContaLegalReferenceAdapter maps iConta workflow input to
  EmbeddedLegalReferenceRequest and maps EmbeddedLegalReferenceResponse to
  iConta-owned presentation models.
- IContaLegalReferenceAdapter preserves requested-reference identity and treats
  correlationId as linkage metadata only.
- IncludeTraceability controls optional traceability projection through the
  embedded feature.
- Operational events contain public-safe metadata only.
- Runtime failures are mapped to public-safe application errors.
- No endpoint, persistence, ingestion, AI/NLP integration, graph integration or
  runtime redesign was introduced.
- FOS-0251 accepted the FOS-0250 iConta adapter implementation.
- FOS-0252 accepted current adapter placement under Runtime as a temporary
  implementation location only.
- Adapter placement rationale: the adapter needed an initial compiled location
  while the application/iConta composition boundary is still not represented as a
  separate project or module.
- Runtime placement is technical debt and is not the target architecture.
- Long-term ownership belongs to an application/iConta composition boundary
  that owns workflow mapping, authorization handoff, operational sink wiring and
  EmbeddedLegalReferenceFeature consumption.
- Runtime DI registration for IContaLegalReferenceAdapter is intentionally
  deferred.
- Future DI registration should occur in application-level composition and own
  IContaLegalReferenceAdapter, an operational sink implementation and the
  embedded feature dependency.
- Future migration should move iConta adapter DTOs, adapter behavior and
  operational hooks out of Runtime once the application/iConta boundary exists.
- Hidden-internal boundary remains preserved: provenance, audit, evidence,
  repository, DI, pipeline and graph internals remain hidden.
- Endpoint exposure remains deferred.
- FOS-0255 accepted current non-registration of IContaLegalReferenceAdapter.
- Deferred adapter DI strategy: IContaLegalReferenceAdapter must not be
  registered in Runtime DI and must not be registered in public API composition
  yet.
- Application/iConta composition owns future adapter DI registration.
- Operational sink registration is a prerequisite for adapter DI registration.
- Runtime DI boundary remains limited to Runtime-owned services and the embedded
  feature currently needed for internal service-level composition.
- Public API composition remains deferred and must not become the adapter
  registration owner before endpoint exposure is explicitly approved.
- Placement debt remains: the adapter is temporarily compiled under Runtime and
  should migrate to the application/iConta boundary when that boundary exists.
- FOS-0258 accepted the operational sink planning review.
- Operational sink purpose is operational visibility for logging, monitoring,
  support triage and incident response only.
- Operational sink ownership belongs to the future application/iConta
  composition boundary and remains outside Runtime and Domain ownership.
- Public-safe metadata rules: operational events may include event name,
  correlationId, allowed tenant/account/workflow identifiers, status,
  IncludeTraceability and coarse outcome only.
- Operational events must not include request payloads, internal exceptions,
  evidence packages, provenance chains, audit trails, repository names, pipeline
  state or graph details.
- Logging and monitoring responsibilities belong to the application/iConta
  operational boundary, not Runtime or Domain.
- Incident-support boundaries remain outside Runtime and must handle
  authorization/scoping issues, cross-tenant visibility concerns, elevated
  unresolved or ambiguous rates, runtime failure spikes, logging/privacy issues
  and public-safety wording concerns.
- Adapter-to-sink interaction is one-way event emission only; the sink must not
  call back into Runtime, alter outcomes, trigger traceability projection,
  retry resolution, enrich legal results or participate in resolution behavior.
- Concrete sink implementation and sink registration remain deferred.
- Registration prerequisites include accepted logging, monitoring, privacy,
  support and incident policies.
- Hidden-internal boundary remains preserved: provenance, audit, evidence,
  repository, DI, pipeline and graph internals remain hidden.

## Next
- FOS-0260 Legal Reference Traceability Operational Sink Documentation Acceptance Review
