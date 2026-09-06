# FOS-0463 Ingestion-Legal Knowledge Handoff Boundary Review

Status: reviewed.

Related: [AR-02.md](AR-02.md),
[ingestion-to-resolution-boundaries.md](ingestion-to-resolution-boundaries.md),
[repository-and-consumer-boundaries.md](repository-and-consumer-boundaries.md),
[FOS-0462-RESOLUTION-CLASSIFICATION-COLLABORATION-BOUNDARY.md](FOS-0462-RESOLUTION-CLASSIFICATION-COLLABORATION-BOUNDARY.md)

## Objective

Define the explicit handoff from the current narrow, deterministic, in-memory
Ingestion capability to Legal Knowledge. The boundary preserves AR-02: Legal
Knowledge is distinct from Ingestion; Ingestion remains separate from
Resolution and Classification; and Assurance/Traceability is transversal while
its contracts remain independently owned.

This review is documentation only. It does not add a handoff stage, persistence,
or runtime coupling.

## Decision

Ingestion may hand off a successful raw-document package to an explicit Legal
Knowledge curation or consumer boundary only after acquisition, normalization,
raw-document identity decision, and associated ingestion traceability have been
completed. The handoff is candidate material, not automatically accepted
curated legal knowledge.

```text
Ingestion success and traceability
  -> explicit curation / Legal Knowledge consumer boundary
  -> curated Legal Knowledge, if separately accepted
```

The current Runtime ingestion foundation remains narrow, deterministic, and
in-memory. Its existing stage named `CuratedPromotion` does not authorize or
implement automatic Legal Knowledge curation, rule generation, or fiscal/legal
conclusions.

## Ingestion Ownership

Ingestion owns:

- source selection, acquisition orchestration, and source metadata snapshots;
- raw document content, normalization, fingerprinting, and identity decisions;
- raw document and version identifiers, source linkage, configuration snapshots,
  batch context, replay and idempotency context;
- ingestion status, failures, retries, audit events, and provenance records;
- repository-neutral raw-document handoff context.

Ingestion may provide an explicit downstream Legal Knowledge boundary with:

- raw document content or a repository-neutral content reference;
- raw document and version identity, identity-decision outcome, fingerprint,
  content hash, and hash algorithm;
- source identity and source metadata snapshot references;
- configuration snapshot, batch, replay, and policy context;
- ingestion audit and provenance links, including correlation and causation
  identifiers where available;
- explicit success or failure status.

Ingestion must not perform Legal Reference Resolution, Classification, legal
interpretation, fiscal/legal conclusions, or automatic fiscal-rule generation.
It must not call Resolution engines, repositories, or pipelines, and it must
not call `ClassificationEngine`.

## Handoff Gate

Only successful ingestion output with its identity and traceability context may
be presented as candidate material for curation. An ingestion failure, skipped
candidate, or incomplete identity/traceability record is not accepted Legal
Knowledge and must remain visible as an ingestion outcome.

Legal Knowledge must not silently reinterpret an ingestion failure as accepted
curated material. Acceptance or rejection of candidate material is a separate,
explicit curation decision; it is not implied by acquisition, storage, or the
current in-memory runtime pipeline.

## Legal Knowledge Ownership After Handoff

Legal Knowledge owns curated legal knowledge and its own interpretation-free
knowledge vocabulary, including curated legal sources, fragments, citations,
authority/conflict helpers, and rule-adjacent concepts where separately
accepted. It may retain the supplied Ingestion identity and traceability links
as provenance context.

Legal Knowledge does not take ownership of Ingestion source identity, raw
document identity decisions, batch execution, ingestion failures, ingestion
audit records, or ingestion provenance records. It does not automatically
derive fiscal rules or fiscal/legal conclusions from handed-off material.

Generic Legal Core remains limited to shared structural legal primitives. It
does not own Ingestion execution, curation, Resolution, Classification, or
Assurance/Traceability operations.

## Resolution and Classification Boundaries

Legal Reference Resolution remains downstream of stable material and owns legal
reference normalization, policies, attempts, resolved/ambiguous/unresolved
outcomes, corrections, and Resolution-specific audit/provenance/evidence
contracts. This handoff does not authorize direct Ingestion coupling to
Resolution engines, repositories, or pipelines.

Classification remains a separate first-class capability. It owns rules,
evaluation, classification decisions, `DecisionExplanation`,
`DecisionLegalBasis`, and `AuditGraph`. This handoff does not authorize direct
Ingestion coupling to `ClassificationEngine`, automatic rule binding, or
classification from raw Ingestion material.

The FOS-0462 Resolution-to-Classification boundary remains unchanged.

## Assurance and Traceability

Audit, Provenance, and Evidence remain separate, independently traceable
contracts within transversal Assurance/Traceability:

- Ingestion retains its audit and provenance records for source, raw-document,
  batch, and identity history.
- Legal Knowledge may correlate curated material to Ingestion lineage without
  replacing or flattening Ingestion records.
- Resolution retains `ResolutionResult`, `ResolutionAuditTrail`,
  `ResolutionProvenance`, and `ResolutionEvidencePackage` ownership.
- Classification retains `DecisionLegalBasis`, `AuditGraph`, decision, and
  explanation ownership.

No handoff creates a merged Ingestion, Legal Knowledge, Resolution,
Classification, audit, provenance, evidence, or decision contract.

## Dependency and Composition Boundary

Any future handoff implementation must be initiated through explicit Runtime or
application composition while preserving Runtime -> Domain dependency direction.
Domain contracts must not depend on Runtime. Legal Knowledge, Ingestion,
Resolution, and Classification must not acquire dependencies on each other's
internal engines, repositories, or pipelines through this boundary.

## Non-Goals

- No durable persistence or public API exposure.
- No AI/NLP, graph infrastructure, graph traversal, visualization, or automatic
  rule generation.
- No Ingestion, Legal Knowledge, Resolution, or Classification runtime redesign.
- No automatic curation, promotion, rule binding, or fiscal/legal conclusion.
- No changes to production code, runtime APIs, or tests.

## Review Result

The handoff is coherent with AR-02. Ingestion remains a narrow deterministic
foundation that supplies traceable candidate material; Legal Knowledge retains
separate curation ownership; Resolution and Classification remain separate
downstream capabilities; and Assurance/Traceability remains transversal.
