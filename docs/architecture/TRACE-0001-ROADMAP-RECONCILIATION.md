# TRACE-0001 — Roadmap Reconciliation

Status: Active
Date: 2026-06-02

Purpose: reconcile the ticket/roadmap naming schemes, the milestone names, the
git commit history, and the actual implementation, recording every discrepancy
found during ARCH-0002. Documentation only — no code, model, or test changes.

Companion document: [ARCH-0002-REALITY-CHECK.md](ARCH-0002-REALITY-CHECK.md).

---

## 1. Roadmap Taxonomy

Several independent identifier schemes coexist in the history without a single
documented definition. Observed usage:

| Prefix | Observed meaning (from usage) | In commit history? | Notes |
|--------|-------------------------------|--------------------|-------|
| **FOS** | Incremental feature tickets that built the legal-reasoning stack | Yes — `FOS-0028` … `FOS-0049` | The primary workstream. `FOS-0050` was produced but **not** committed under its own name (see §2). Some tickets span two commits (e.g. `FOS-0047`). |
| **MILESTONE** | Conceptual milestones in the master doc (`MILESTONE-0001` Foundation, `-0002` Executable Architecture, `-0003` current) | No (doc only) | Used as planning milestones, never as commit prefixes. No `M3`/`M4` defined anywhere. |
| **M-series** | Appears once as a commit prefix: `M2-0001 "Add jurisdiction model"` | Yes — `b58044c` | Anomalous. Overlaps conceptually with `MILESTONE`. Its commit content is unrelated to its title (see §2). |
| **V-series** | Version/experimental slice: `V1-0001 "Add knowledge projection slice"` | Yes — `ef1e4d0` | A standalone experimental projection; later corrected by `ARCH-0001`. |
| **ARCH** | Architecture review / remediation | Yes — `ARCH-0001` (`acad79a`), `ARCH-0002` (`d88c2f5`) | `ARCH-0001` = review + rebase; `ARCH-0002` = reality-check doc. |
| **TRACE** | Reconciliation / traceability documentation | Yes — this document | Status/traceability records. |

**Taxonomy finding:** six overlapping schemes (`FOS`, `MILESTONE`, `M`, `V`,
`ARCH`, `TRACE`) are in use with no documented authority over which governs
feature work vs. milestones vs. versions vs. architecture. `M` and `MILESTONE`
are easy to confuse.

---

## 2. Expected vs Actual

Discrepancies discovered during ARCH-0002:

### Discrepancy A — Jurisdiction model
- **Expected:** `M2-0001 Jurisdiction Model`.
- **Actual:** No jurisdiction model exists in code. Commit `b58044c`
  (titled `M2-0001 Add jurisdiction model`) contains different work — the
  `FOS-0050` purpose-aware narrative chain rendering
  (`PurposeAwareNarrativeProjector` + its tests). The commit message does not
  match its contents.

### Discrepancy B — Missing FOS-0050 commit
- **Expected:** `FOS-0050 Render teleological purpose chains` as its own commit.
- **Actual:** No commit is titled `FOS-0050`. The FOS-0050 changes live inside
  `b58044c` (mislabeled `M2-0001`). FOS-0050 is implemented and tested, but
  untraceable by its ticket id in history.

### Discrepancy C — V1-0001 superseded
- **Expected:** `V1-0001` knowledge projection as committed (`ef1e4d0`).
- **Actual:** As originally committed it diverged from the canonical pipeline
  (projected from the thin `ClassificationResult`, fabricated a `Purpose` atom,
  used `Atoms`/`Relationships`, lacked graph validation). `ARCH-0001`
  (`acad79a`) rebased it onto `ClassificationDecision`/`DecisionExplanation`,
  removed the fake purpose, renamed to `Nodes`/`Edges`, and added validation.
  The current behavior differs from the `V1-0001` commit.

### Discrepancy D — Taxonomy overlap
- **Expected:** one identifier scheme per concern.
- **Actual:** `FOS` / `MILESTONE` / `M` / `V` / `ARCH` / `TRACE` coexist
  without a documented mapping (see §1).

---

## 3. Validated Capabilities

Capabilities exercised end-to-end in `ClassificationEngine.ClassifyAsync` and
covered by passing integrated tests:

- Legal-authority model — `SourceAuthorityLevel`, `SourceAuthority`,
  `SourceHierarchy`, `CitationAuthority` / `CitationSpecificity` /
  `CitationChronology`.
- Conflict resolution — `LexSuperior`, `LexSpecialis`, `LexPosterior`,
  composed by `ConflictResolver` (via `DecisionLegalBasis.Resolve`).
- Legal basis — `DecisionLegalBasis` attached to every decision.
- Provenance — `AuditGraph` built by `DecisionAuditGraphBuilder`, attached via
  `DecisionExplanation.AuditGraph`.
- Canonical explanation surface — `DecisionExplanation`,
  `ExplanationNarrative` / `ExplanationNarrativeProjector`.

---

## 4. Experimental Capabilities

Real, tested capabilities that are **not integrated** into the engine/decision
(inputs are constructed only by tests; no production caller):

- Purpose reasoning — `PurposeGraph`, `PurposeAwareExplanation`,
  `GoverningPurposes()`, `GoverningPurposeChains()`, purpose-aware narrative.
  No code path produces a `PurposeGraph` from real legislation.
- Knowledge projection — `IKnowledgeProjector` / `DefaultKnowledgeProjector` /
  `KnowledgeProjectionResult` (post-`ARCH-0001`: rebased, validated, reuses the
  canonical narrative) — but no engine/API consumer invokes it.

---

## 5. Missing Capabilities

Referenced in roadmap/commit discussions but absent or stubbed in code:

- **Jurisdiction model** — referenced by `M2-0001`'s title; absent from code.
- **Legislation → executable rules** — `LegalSource → LegalFragmentExtractor →
  RuleCandidateGenerator → RuleVersion / FiscalRule` is naive and standalone;
  `FiscalRule` is never evaluated; not connected to classification.
- **Generic evaluation** — `Runtime.Evaluation.EvaluationEngine` is a skeleton
  returning an empty result.
- **Delivery surface** — no API/DI wiring; library only.
- **Deferred policies** — unresolved-conflict escalation, regulatory reporting,
  localization, structured/sectioned narrative.

---

## 6. Recommendations

Documentation/governance only. No new architecture is proposed.

1. **Define one ticket taxonomy.** Document what `FOS`, `MILESTONE`, `M`, `V`,
   `ARCH`, and `TRACE` each govern, and retire or alias overlapping prefixes
   (notably `M` vs `MILESTONE`).
2. **Record the `b58044c` mislabel.** Document that `b58044c` is the `FOS-0050`
   work, and that `M2-0001 Jurisdiction Model` is **not** implemented, so the
   history is auditable despite the wrong commit message. (Any actual commit-
   message correction is a separate git decision, out of scope for this doc.)
3. **Mark `V1-0001` as superseded by `ARCH-0001`.** Note that the current
   knowledge-projection behavior is the `ARCH-0001` rebase, not the `V1-0001`
   commit.
4. **Treat `ARCH-0002` + this `TRACE-0001` as the status source of truth** until
   the roadmap and history are reconciled.
5. **Flag absent capabilities as absent** (jurisdiction, ingestion, generic
   evaluation, delivery) — do not infer them from commit titles.
