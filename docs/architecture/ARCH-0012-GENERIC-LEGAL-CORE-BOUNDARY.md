# ARCH-0012 Generic Legal Core Boundary

Status: Planning
Date: 2026-06-13

Related: [ARCH-0011-Legal-Reference-Addressing-Model.md](ARCH-0011-Legal-Reference-Addressing-Model.md),
[global-invariants.md](global-invariants.md),
[identity-model-matrix.md](identity-model-matrix.md),
[ingestion-to-resolution-boundaries.md](ingestion-to-resolution-boundaries.md)

This is an architecture decision record only. It defines the boundary for a
future generic legal core. It does not move code, create a project, add runtime
behavior, add persistence, add graph behavior, add AI/NLP, or generate rules.

## Context

FiscalOS now has multiple legal concept families:

- ingestion concepts in `FiscalOS.Domain.LegislationIngestion`,
- legal reference addressing and resolution contracts in
  `FiscalOS.Domain.LegalReferences`,
- citation, jurisdiction, source authority, source hierarchy and knowledge
  concepts in `FiscalOS.LegalKnowledge`,
- fiscal classification and rule execution in Runtime/Core.

Several concepts are structural legal primitives rather than ingestion,
resolution, repository, graph, fiscal rule, or runtime concepts. Examples
include jurisdiction identity, legal document references, structural reference
segments, structural legal references, and fully qualified legal references.

Without an explicit boundary, these primitives can drift into the subsystem
that first needed them. That increases coupling between ingestion, legal
reference resolution, legal knowledge, classification, and future repository
handoff work.

## Problem

The current project layout is workable, but it does not state which boundary
owns reusable legal primitives that are not specific to ingestion, resolution,
classification, or legal knowledge storage.

This creates three risks:

- structural legal concepts become coupled to resolution attempts or runtime
  execution,
- jurisdiction and authority concepts become coupled to fiscal rule behavior,
- future ingestion-to-resolution handoff work reuses legal concepts without a
  clear dependency direction.

## Decision

FiscalOS will recognize a Generic Legal Core boundary for reusable legal
primitives that are:

- structural,
- deterministic,
- immutable or value-object-like,
- independent from runtime execution,
- independent from repository storage,
- independent from fiscal classification,
- independent from legal interpretation.

The boundary exists to own vocabulary shared by ingestion, legal reference
resolution, legal knowledge, and future repository handoff without making those
subsystems depend on each other.

## Boundary Responsibilities

Generic Legal Core may own:

- jurisdiction identity,
- legal document references,
- structural reference segments,
- structural legal references,
- fully qualified legal references,
- source type and authority vocabulary only if kept descriptive and
  interpretation-free,
- source hierarchy context vocabulary only if kept graph-free.

Generic Legal Core must keep operations pure and local. Structural containment,
parent lookup, equality, comparison, normalization markers, and value validation
are acceptable. Repository lookup, resolution, graph traversal, semantic
interpretation, and rule construction are not.

## Non-Responsibilities

Generic Legal Core does not own:

- ingestion source identity,
- raw document identity decisions,
- ingestion batches or runtime stages,
- provenance or audit event emission,
- legal reference resolution attempts,
- resolution outputs, corrections, or supersession,
- repository persistence or storage references,
- fiscal classification,
- fiscal rules or future rule identity,
- source hierarchy graph storage or traversal,
- LegalGraph or LegalAtom implementation,
- AI/NLP interpretation,
- automatic rule generation.

## Candidate Vocabulary

The first candidate types for the boundary are:

- `JurisdictionId`,
- `LegalDocumentReference`,
- `ReferenceSegment`,
- `LegalReference`,
- `FullyQualifiedLegalReference`.

Later candidates, pending separate review:

- `LegalSourceType`,
- `SourceAuthorityLevel`,
- `SourceHierarchy`,
- `LegalCitation`.

These later candidates need extra care because they are closer to authority,
knowledge, conflict resolution, and classification behavior.

## Ownership Rules

- Ingestion owns source identity, raw document identity, fingerprints,
  snapshots, batch execution, provenance, audit, and repository handoff context.
- Legal Reference Resolution owns reference observations, resolution policies,
  attempts, outcomes, corrections, and replay comparison behavior.
- Legal Knowledge owns curated knowledge, conflict helpers, purpose/audit graph
  models and rule-adjacent concepts until a later boundary decision moves any
  primitive out.
- Runtime owns execution and composition.
- Generic Legal Core owns shared structural legal vocabulary only.

## Dependency Direction

The intended direction is:

```text
Runtime -> Domain / LegalKnowledge / future Generic Legal Core
Domain feature contracts -> future Generic Legal Core
LegalKnowledge -> future Generic Legal Core
Generic Legal Core -> no FiscalOS feature subsystem
```

Generic Legal Core must not reference ingestion, legal reference resolution,
legal knowledge, runtime, API, repositories, graphs, or classification
execution.

## Implementation Guidance

No code should move as part of this ADR.

The first implementation slice should be one of:

- documentation and tests that assert current ownership rules, or
- a minimal `FiscalOS.LegalCore` project/namespace containing only the lowest
  risk primitives after compatibility tests exist.

Do not start by moving `LegalCitation`, source authority behavior, source
hierarchy behavior, legal knowledge graphs, or classification-facing types.

## Consequences

- Gives future ingestion-to-resolution work a neutral vocabulary target.
- Reduces pressure to make ingestion depend on legal resolution or legal
  knowledge concepts.
- Preserves the distinction between structural legal addresses and legal
  reference identity records.
- Creates an incremental path to extract reusable primitives without broad
  namespace churn.
- Keeps graph implementation, AI/NLP, and rule generation explicitly outside
  this boundary.

## Deferred Decisions

- Whether the boundary becomes a new `FiscalOS.LegalCore` project or remains a
  namespace convention inside `FiscalOS.Domain` for now.
- Whether `JurisdictionId` moves before structural legal reference types.
- Whether source authority vocabulary is generic enough for this boundary.
- Whether `LegalCitation` remains in `FiscalOS.LegalKnowledge` or later becomes
  a composition of legal-core primitives plus authority metadata.
- Whether existing namespace names are migrated or wrapped for compatibility.

## Out of Scope

No code movement, no persistence, no repository implementation, no API
implementation, no source hierarchy graph implementation, no graph traversal,
no LegalGraph or LegalAtom implementation, no AI/NLP integration, and no
automatic rule generation.
