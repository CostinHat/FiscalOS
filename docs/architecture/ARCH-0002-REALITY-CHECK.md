# ARCH-0002 — Reality Check

Status: Active
Date: 2026-06-02

Purpose: record what the FiscalOS classification/explanation stack **actually**
does in running, tested code versus what is experimental or merely notional,
as of the post-ARCH-0001 state. Assessed from the current implementation only —
no roadmap aspirations included.

> Note on scope: the repository defines milestones MILESTONE-0001/0002/0003
> only; there is no `M3`/`M4` milestone definition in the codebase. The
> categories below classify the advanced conceptual layers by whether tested
> code exercises them, not by a milestone plan.

---

## 1. Integrated & Validated

These are exercised end-to-end in the live `ClassificationEngine.ClassifyAsync`
path and covered by passing tests.

- **Legal-authority model** — `SourceAuthorityLevel`, `SourceAuthority`
  (LegalSourceType → level), `SourceHierarchy`, and the citation resolvers
  `CitationAuthority` / `CitationSpecificity` / `CitationChronology`.
- **Conflict resolution** — `LexSuperior`, `LexSpecialis`, `LexPosterior`, and
  their composition `ConflictResolver`, invoked via `DecisionLegalBasis.Resolve`.
- **Legal basis** — `DecisionLegalBasis` (conflict-resolved governing law)
  attached to every decision.
- **Provenance** — `AuditGraph`, built by `DecisionAuditGraphBuilder`, attached
  via `DecisionExplanation.AuditGraph`. Single canonical location.
- **Canonical explanation surface** — `DecisionExplanation`, with
  `ExplanationNarrative` / `ExplanationNarrativeProjector`.

These form the working core: authority → conflict resolution → legal basis →
provenance → explanation.

---

## 2. Experimental

Real, tested capabilities that are **not wired into** the engine or decision —
they only run when a caller (currently only tests) constructs their inputs by
hand.

- **Purpose reasoning** — `PurposeGraph`, `PurposeAwareExplanation`,
  `GoverningPurposes()`, `GoverningPurposeChains()`, and the purpose-aware
  narrative. Tests build the `PurposeGraph` as a fixture; nothing in the real
  path produces one. Purpose is correctly modelled as nodes (the `Serves`
  chains are intrinsically graph-shaped); the only metadata element is the
  `LegalCitation` anchoring a `Norm` node.
- **Knowledge projection** — `IKnowledgeProjector` / `DefaultKnowledgeProjector`
  / `KnowledgeProjectionResult`. After ARCH-0001 it projects from the canonical
  `ClassificationDecision` / `DecisionExplanation`, validates its graph
  (unique node ids, edge endpoints exist), and reuses the canonical narrative —
  but it has no engine/API consumer.

---

## 3. Not Implemented

Concepts that are stubs, absent, or notional in the current code.

- **Jurisdiction model** — does not exist. The `M2-0001 "Add jurisdiction
  model"` commit actually contains the FOS-0050 narrative changes and no
  jurisdiction type (mislabeled commit).
- **Legislation → executable rules** — `LegalSource → LegalFragmentExtractor →
  RuleCandidateGenerator → RuleVersion / FiscalRule` is naive and standalone;
  `FiscalRule` is never evaluated and is not connected to classification.
- **Generic evaluation** — `Runtime.Evaluation.EvaluationEngine` is a skeleton
  returning an empty result; classification uses its own engine instead.
- **Delivery surface** — no API/DI wiring; the system is a library with no
  consumer.
- **Deferred policies** — unresolved-conflict escalation, regulatory reporting,
  localization, and structured/sectioned narrative: deferred, no code.

---

## 4. Current Architectural Conclusion

The legal-reasoning core (authority, conflict resolution, legal basis,
provenance, explanation) is real, tested, and integrated. Purpose reasoning and
knowledge projection are genuine but **unintegrated** capabilities awaiting a
data source and a consumer. Jurisdiction, legislation ingestion, generic
evaluation, and any delivery layer are stubs or absent.

Specific conclusions carried over from ARCH-0002 analysis:

- **No `KnowledgeAtom` is needed** — `Core.Classification.ExplanationNode`
  already fills that role; a separate type would duplicate it.
- **No `AtomGraph` is needed** — `KnowledgeProjectionResult` is the validated
  container. The codebase already has four graph-shaped containers
  (`AuditGraph`, `PurposeGraph`, the dormant `ExplanationGraph`,
  `KnowledgeProjectionResult`); a fifth would worsen proliferation.
- **Purpose stays a node**, not decision metadata — its relationships require a
  graph; no running code treats it as metadata.
- **Provenance stays single-sourced** in `AuditGraph` under
  `DecisionExplanation`; duplicating it elsewhere would re-introduce the
  divergence ARCH-0001 removed.

Net: the next architectural priority is **integration and data sourcing**
(give the experimental layers real inputs and consumers, drive the legislation
ingestion pipeline) rather than introducing additional models.
