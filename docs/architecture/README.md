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

## Current Status

- **181 tests passing.**
- **Working tree clean.**
- **Legislation ingestion planning opened, not implemented** (ARCH-0005);
  out-of-scope items (parser, NLP, AI extraction, rule generator, database, API,
  runtime changes) remain deferred.
