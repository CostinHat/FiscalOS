# ARCH-0005 — Legislation Ingestion Planning

Status: Planning
Date: 2026-06-03

Related: [ARCH-0002-REALITY-CHECK.md](ARCH-0002-REALITY-CHECK.md),
[ARCH-0004-MICROENTERPRISE-LAW-PATH.md](ARCH-0004-MICROENTERPRISE-LAW-PATH.md),
[TRACE-0001-ROADMAP-RECONCILIATION.md](TRACE-0001-ROADMAP-RECONCILIATION.md)

This is a **planning** document only. It records that ingestion planning is now
unblocked and scopes the design space. No implementation is proposed or implied.

## Why ingestion planning is now unblocked

The prerequisite rule-shape variety (ARCH-0004) is present in the live
`ClassifyAsync` path:

- **Microenterprise** covers the **single-citation** path (one governing
  `LegalCitation`, trivially resolved).
- **VAT** covers the **competing-citation** path (two citations resolved by the
  existing `ConflictResolver` via lex superior).
- **`LegalCitation`** now carries the full field set ingestion must reproduce:
  authority (via `SourceType` → `SourceAuthority`), specificity
  (`SpecificityLevel`), chronology (`EffectiveDate`), and jurisdiction
  (`JurisdictionId`).

Together these establish the output model ingestion must target: produce
`LegalCitation` values — singly or as a competing set — with all fields
populated. The deciding conflict principle (superior / specialis / posterior)
is the resolver's concern and is already validated, so it does not gate
ingestion design.

## Planning scope

- **Versioned `LegalSource`** — a representation of legal sources that carries
  version/effective-date information, so a given threshold or citation can be
  tied to the source version in force.
- **Legal fragments** — the unit between a source and a citation (e.g. an
  article/paragraph), as the granular thing a citation points at.
- **Curated / manual extraction boundary** — define what is authored by hand
  (today: `*Regime` constants and `LegalCitation` values) and how that curated
  layer is structured so it can later be partially or fully populated by
  automation.
- **Future automated extraction boundary** — define the seam where automated
  extraction *could* later plug in, without committing to building it. The
  curated and automated paths should produce the same downstream shape.
- **Mapping to existing `LegalCitation`** — how ingested fragments/sources
  resolve into the current `LegalCitation(SourceType, SourceReference, Article,
  Specificity, EffectiveDate, Jurisdiction)` shape unchanged.
- **Mapping to curated regime definitions** — how ingested data relates to the
  proven `*Regime` pattern (`MicroenterpriseRegime`, `VatRegime`): whether
  regimes are populated from ingestion, and how the threshold-↔-citation binding
  established in the LAW series is preserved.

## Explicitly out of scope

- Parser implementation
- NLP
- AI extraction
- Rule generator
- Database
- API
- Runtime changes

## Open questions

- **How do threshold values bind to legal versions?** A threshold value (e.g.
  the microenterprise `500_000` or VAT `300_000`) corresponds to a specific
  version of the cited norm; how is value-↔-version coherence represented and
  maintained?
- **Should `Regime` objects be generalized?** `MicroenterpriseRegime` and
  `VatRegime` share a near-identical shape (threshold(s) + citation(s)); should
  this be generalized, and if so does that happen before or as part of ingestion?
- **How are citations versioned?** A `LegalCitation` currently carries one
  `EffectiveDate`; how is the versioning of a cited provision over time
  represented as sources are amended?
- **How are conflicts preserved from source to decision?** Competing citations
  (as in VAT) must travel intact from ingested sources through
  `DecisionLegalBasis` to the decision; how is that competition preserved and
  represented end to end?

## Status

Ingestion planning: unblocked and open. No implementation begun. Out-of-scope
items above remain deferred; open questions are to be resolved during planning.
