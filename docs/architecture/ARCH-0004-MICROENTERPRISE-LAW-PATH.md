# ARCH-0004 — Microenterprise Law → Citation → Rule path

Status: Accepted
Date: 2026-06-03

Related: [ARCH-0002-REALITY-CHECK.md](ARCH-0002-REALITY-CHECK.md),
[ARCH-0003-PURPOSE-EXTERNAL-COMPOSITION.md](ARCH-0003-PURPOSE-EXTERNAL-COMPOSITION.md),
[TRACE-0001-ROADMAP-RECONCILIATION.md](TRACE-0001-ROADMAP-RECONCILIATION.md)

## Decision

The microenterprise **Law → Citation → Rule** path is **locally complete under
hand-curation**. No further `LAW-000X` slice is justified for the microenterprise
path until a legislation ingestion capability exists.

"Locally complete" means: within the microenterprise path, every sub-gap of
Law → Citation → Rule that can be closed *without* deriving anything from
legislation has been closed, and the legal basis is traceable, single-sourced,
and surfaced end to end. It does **not** mean "law-derived" — the data is still
hand-curated.

## LAW-0001..0004 summary

- **LAW-0001 — Curated citation for production microenterprise rule.**
  `MicroenterpriseClassificationRule` emits a curated `LegalCitation`
  (FiscalCode / Legea 227/2015 / Art. 47 / RO) when eligible, flowing through
  `RuleEvaluationResult.Citations -> DecisionLegalBasis -> DecisionExplanation`.
- **LAW-0002 — Bind microenterprise threshold to curated citation.**
  Introduced `MicroenterpriseRegime`, a single curated definition holding the
  revenue threshold, minimum employee count, and the governing citation. The
  eligibility logic reads the thresholds from it and the rule emits its citation,
  so parameter and legal source cannot drift.
- **LAW-0003 — Move citation emission to the eligibility evaluation.**
  `MicroenterpriseEligibilityRule.EvaluateWithCitation` surfaces the citation
  from the same layer that applies the threshold (via `MicroenterpriseEvaluation`);
  `MicroenterpriseClassificationRule` forwards it instead of re-attaching its own.
- **LAW-0004 — Cite governing norm in the eligibility explanation.**
  The eligible-branch explanation text names the governing norm
  (`per Legea 227/2015 Art. 47.`), sourced from `MicroenterpriseRegime.Citation`,
  so prose and structured citation are single-sourced and cannot drift.

## What is complete

- Single source of truth: `MicroenterpriseRegime` binds threshold value(s) and
  citation.
- Citation produced where the law is applied (eligibility evaluation), forwarded
  by the wrapper — no cross-layer re-attachment.
- Structured flow intact: citation -> `RuleEvaluationResult.Citations` ->
  `DecisionLegalBasis` -> `DecisionExplanation`, with jurisdiction attached
  (M2R-0002) and rendered in the knowledge projection (M2R-0003).
- Human-readable basis: the eligibility explanation names the governing norm,
  single-sourced from the regime.

## What remains deferred (not a LAW slice)

- **Legislation ingestion / law-sourcing** — deriving the threshold and citation
  from actual legal text. Root of gap #1; out of scope here.
- **Threshold-value <-> norm-version coherence** — the `500_000` value
  corresponds to a specific version of Art. 47; binding value to the cited norm's
  version belongs with versioned ingestion, not a standalone curated slice.
- **Denial-branch citation** — inert: the engine only consumes the winning rule's
  citations, so citing the norm on rejection changes nothing.
- **Per-parameter attribution** (which article justifies revenue vs employee
  count) — speculative; both currently derive from Art. 47.

## Next options

Two paths forward, neither a further microenterprise LAW slice:

1. **Start legislation ingestion** — build the capability that derives thresholds
   and citations from actual legal sources (addresses gap #1's root and the
   deferred versioning concern).
2. **Apply the LAW pattern to another rule** — replicate the now-proven
   curated-regime / cited-evaluation pattern (LAW-0001..0004) on a different
   classification rule.

## Status

Microenterprise Law -> Citation -> Rule: locally complete under hand-curation.
Further microenterprise LAW slices: stopped until ingestion exists.
