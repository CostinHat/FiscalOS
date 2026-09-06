# FiscalOS Architecture Index

A consolidated index of the current FiscalOS architecture work. Each entry links
to its document (where one exists) or names the commit that introduced it.

## Architecture Decisions

- **ARCH-0001** (`acad79a`) — Rebase knowledge projection onto
  `DecisionExplanation` (remediation of the V1-0001 slice). Committed directly;
  no standalone document.
- [ARCH-0002-REALITY-CHECK.md](ARCH-0002-REALITY-CHECK.md) — Reality check:
  what is integrated & validated, experimental, and not implemented.
- [ARCH-0003-PURPOSE-EXTERNAL-COMPOSITION.md](ARCH-0003-PURPOSE-EXTERNAL-COMPOSITION.md)
  — Purpose stays an external composition layer
  (`ClassificationDecision + PurposeGraph -> PurposeAwareExplanation`).
- [ARCH-0004-MICROENTERPRISE-LAW-PATH.md](ARCH-0004-MICROENTERPRISE-LAW-PATH.md)
  — Microenterprise Law → Citation → Rule path is locally complete under
  hand-curation.
- [ARCH-0005-LEGISLATION-INGESTION-PLANNING.md](ARCH-0005-LEGISLATION-INGESTION-PLANNING.md)
  — Ingestion planning unblocked and scoped (planning only).
- **ARCH-0006** (`d9eb866`) — Legislation ingestion architecture: source discovery, acquisition, versioning, normalization, citation detection, candidate extraction, human review, curated promotion, and rule binding. Planning only; no runtime ingestion implementation.
- **ARCH-0007** (`af58c41`) — Terminology reconciliation between implemented concepts, planning concepts, and future ingestion concepts.
- **ARCH-0008** (`a758961`) — LegalAtom remains a planning term and is not promoted to a first-class implemented model at this stage.
- **ARCH-0009** (`f2be656`) — PracticeGraph remains a planning concept, independent from LegalGraph, and is not promoted to a runtime model at this stage.
- **ARCH-0010** (`539c1cd`) — LegalGraph remains a planning concept and is not promoted to a first-class implemented runtime model at this stage.

- [ARCH-0011-Legal-Reference-Addressing-Model.md](ARCH-0011-Legal-Reference-Addressing-Model.md)
  -- LegalReference represents structural legal addressing, not legal content,
  interpretation, or fiscal conclusions.
- [ARCH-0012-GENERIC-LEGAL-CORE-BOUNDARY.md](ARCH-0012-GENERIC-LEGAL-CORE-BOUNDARY.md)
  -- Generic Legal Core owns shared structural legal primitives while excluding
  ingestion runtime, resolution execution, graph behavior, AI/NLP, and rule
  generation.
- [FOS-0462-RESOLUTION-CLASSIFICATION-COLLABORATION-BOUNDARY.md](FOS-0462-RESOLUTION-CLASSIFICATION-COLLABORATION-BOUNDARY.md)
  -- Resolution may provide separately traceable structural reference outputs
  to Classification-adjacent composition without merging the capabilities or
  their contracts.
- [FOS-0463-INGESTION-LEGAL-KNOWLEDGE-HANDOFF-BOUNDARY.md](FOS-0463-INGESTION-LEGAL-KNOWLEDGE-HANDOFF-BOUNDARY.md)
  -- Ingestion may hand off traceable successful raw-document candidate material
  to explicit Legal Knowledge curation without automatic promotion or coupling
  to Resolution or Classification.
- [AR-02.md](AR-02.md) -- Historical accepted re-baseline of FiscalOS internal
  capability boundaries; its strategic product-positioning statements are
  superseded only as recorded by AR-03.
- [AR-03.md](AR-03.md) -- Accepted independent-product, consumer-agnostic and
  API-first strategic baseline; no production persistence, Public API or
  human-facing application is implied as currently implemented.
- [AR-04.md](AR-04.md) -- Accepted readiness constraints for Vertical Slice 01:
  semantic bi-temporality, canonical/consumer-scoped data boundary and
  executable Consumer Independence enforcement.

## Traceability

- [TRACE-0001-ROADMAP-RECONCILIATION.md](TRACE-0001-ROADMAP-RECONCILIATION.md)
  — Reconciles ticket namespaces, milestone names, commit history, and actual
  implementation. Records the mislabeled commit `b58044c`
  (`M2-0001 "Add jurisdiction model"` actually contains FOS-0050).

## Jurisdiction Runtime Thread

- **M2R-0001** (`93f6f31`) — `Jurisdiction` / `JurisdictionId` foundations.
- **M2R-0002** (`6ebe7b4`) — Attach `JurisdictionId` to `LegalCitation`; carried
  through `DecisionLegalBasis` into `DecisionExplanation`.
- **M2R-0003** (`d69f5bf`) — Render jurisdiction on the knowledge-projection
  `LegalBasis` node label.

## Law → Citation → Rule

- **LAW-0001** (`6e0cdb0`) — Curated citation on the production microenterprise
  rule.
- **LAW-0002** (`2ecce40`) — Bind threshold ↔ citation in a single curated
  definition (`MicroenterpriseRegime`).
- **LAW-0003** (`7f0ea5c`) — Move citation emission to the eligibility evaluation
  layer (`MicroenterpriseEvaluation`).
- **LAW-0004** (`0d63d33`) — Cite the governing norm in the eligibility
  explanation text, single-sourced from the regime.
- **LAW-0005** (`846a2b0`) — VAT payer rule with two competing citations,
  resolved by the existing `ConflictResolver` (lex superior) in the live path.

See [ARCH-0004](ARCH-0004-MICROENTERPRISE-LAW-PATH.md) for the microenterprise
path summary and the single-citation vs competing-citation coverage.

## Delivery

- **DEL-0001** (`3ab5daa`) — Console sample exercising the validated
  `ClassificationEngine` path end to end (no DI, no database).

## Recent Slices

- **FOS-0030** (`f5c09ca`) — PurposeReference supports optional Note metadata.
- **FOS-0031** (`85e25e9`) — AuditEvent record introduced as a standalone audit event model.

## Current Status

- AR-03 is the accepted strategic baseline: FiscalOS is an independent,
  consumer-agnostic, API-first fiscal/legal knowledge product and platform.
- The current implementation remains a narrow deterministic in-memory
  foundation; durable persistence, authoritative production source acquisition,
  Public API exposure and a human-facing interface are not implemented yet.
- AR-02 internal capability boundaries remain valid: small generic Legal Core;
  distinct Ingestion, Legal Knowledge, Resolution and Classification; Runtime →
  Domain; and transversal Assurance/Traceability.
- The next roadmap step operationalizes AR-04 and selects a narrow, measurable
  production-grade Vertical Slice 01. The source, supported question,
  persistence/API technologies, deployment model and commercial packaging remain
  open.
- `LegalGraph` and `LegalAtom` in ARCH-0006 remain planning terms, not
  implemented code models. Current implemented types include `LegalCitation`,
  curated regime/rule models, `PurposeGraph`, `AuditGraph`, and
  `KnowledgeProjectionResult`.
