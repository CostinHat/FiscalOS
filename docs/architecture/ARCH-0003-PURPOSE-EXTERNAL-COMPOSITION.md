# ARCH-0003 — Purpose stays an external composition layer

Status: Accepted
Date: 2026-06-02

Related: [ARCH-0002-REALITY-CHECK.md](ARCH-0002-REALITY-CHECK.md),
[TRACE-0001-ROADMAP-RECONCILIATION.md](TRACE-0001-ROADMAP-RECONCILIATION.md)

## Decision

Purpose reasoning remains an **external composition layer**, assembled by a
caller from the decision the engine already returns:

```
ClassificationDecision + PurposeGraph -> PurposeAwareExplanation
```

The classification core does **not** take, carry, or surface a `PurposeGraph`.

## Context

`PurposeGraph` is currently constructed only in tests; it is not produced or
consumed anywhere in the runtime path (`ClassifyAsync` and `DecisionExplanation`
reference neither it nor `PurposeAwareExplanation`). The question was whether to
integrate it by adding an optional `PurposeGraph` parameter to
`ClassificationEngine.ClassifyAsync`.

## Rationale

Adding `PurposeGraph` to `ClassifyAsync` was rejected as core-API pollution:

- Purpose does not affect the classification outcome — it is explanatory
  enrichment, orthogonal to "classify a subject -> decision". The parameter
  would be threaded through unused (tramp data).
- It would force the graph onto `ClassificationDecision` / `DecisionExplanation`,
  bloating the validated core types and re-opening the purpose slot FOS-0045
  deliberately left closed.
- It would couple the validated core to the experimental, unintegrated purpose
  layer, undoing the separation preserved by ARCH-0001 / ARCH-0002.

External composition is preferred because:

- The engine stays pure: `ClassifyAsync(subject) -> ClassificationDecision`.
- The seam already exists: `PurposeAwareExplanation` composes
  `(DecisionExplanation, PurposeGraph)` with zero core change.
- Separation is preserved — law / conflict / provenance in the core; purpose as
  an orthogonal, opt-in overlay assembled at the edge.

## Consequences

- No change to `ClassificationEngine.ClassifyAsync`, `ClassificationDecision`,
  `DecisionExplanation`, `ConflictResolver`, or `DecisionLegalBasis`.
- Purpose output (`PurposeAwareExplanation.GoverningPurposeChains()`,
  purpose-aware narrative) is reachable only by composing the returned decision
  with a `PurposeGraph` supplied by the caller.
- The remaining gap — a production consumer that holds both a decision and a
  `PurposeGraph` — is a delivery-layer concern (no API/DI today) and is the
  correct place for the composition. It is intentionally out of scope here.
- No `PurposeGraph` factory, registry, service, or legislation-ingestion change
  is implied by this decision.

## Status of purpose integration

Stopped. Purpose remains experimental and unintegrated by design, surfaced only
through external composition.
