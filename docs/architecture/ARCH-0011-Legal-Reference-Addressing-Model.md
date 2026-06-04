# ARCH-0011 Legal Reference Addressing Model

Status: Planning
Date: 2026-06-04

Related: [ARCH-0006-LEGISLATION-INGESTION-ARCHITECTURE.md](ARCH-0006-LEGISLATION-INGESTION-ARCHITECTURE.md),
[ARCH-0007-TERMINOLOGY-RECONCILIATION.md](ARCH-0007-TERMINOLOGY-RECONCILIATION.md),
[ARCH-0010-LEGAL-GRAPH-DECISION.md](ARCH-0010-LEGAL-GRAPH-DECISION.md)

This is an architecture (planning) document only. No code, domain types, or
runtime behaviour are introduced.

## Context

FiscalOS reasons over `LegalCitation` (`SourceType`, `SourceReference`,
`Article`, `Specificity`, `EffectiveDate`, `Jurisdiction`). A citation names a
provision, but its `Article` is a flat string: it cannot express the
*hierarchical structure* of a provision (document → article → paragraph →
letter → …) nor the *relationships* between two locations (is one nested inside
another?).

As legislation ingestion (FOS-0032..0035) and terminology reconciliation
(FOS-0036) progress, the system needs a precise, structural way to **address** a
location within legislation and to reason about containment between locations —
independent of what the provision says or what it implies fiscally.

## Problem

- `LegalCitation.Article` is an opaque string (e.g. `"Art. 47"`); there is no
  structured address for `Art. 47 alin. (3) lit. b)`.
- There is no way to compute the enclosing provision of a reference, or to test
  whether one reference is nested within another.
- Addressing is currently entangled with citation/authority concerns; there is
  no standalone, comparable, immutable legal address.

## Decision

**A `LegalReference` represents a structural legal address — not legal content,
interpretation, or fiscal conclusions.**

It identifies *where* a provision lives within a legal document's structure, and
nothing about *what it means* or *what it implies*. Content, authority, conflict
resolution, purpose, and conclusions remain the concern of other models:

- citation identity and authority → `LegalCitation` / `SourceAuthority`,
- conflict handling → `ConflictResolver`,
- purpose/teleology → `PurposeGraph`,
- fiscal conclusions → classification rules and regime definitions.

The addressing model is purely structural and side-effect free.

## Model

Model direction (types to be designed in a later implementation ticket — not
created here):

- **LegalDocumentReference** — identifies the legal document/instrument that
  roots an address (e.g. *Legea 227/2015*). It is the document context for a
  reference. (Document versioning is deferred — see Deferred Decisions.)
- **ReferenceSegment** — a single structural step in an address: a typed level
  plus an identifier (e.g. *Article* `47`, *Paragraph* `3`, *Letter* `b`). A
  segment carries a kind and a value; it has no content.
- **LegalReference** — an ordered path of `ReferenceSegment`s describing a
  structural location relative to a document (e.g. Article 47 → Paragraph 3 →
  Letter b). This is the core address.
- **FullyQualifiedLegalReference** — a `LegalDocumentReference` together with a
  `LegalReference`: the absolute address (which document + which location).

Shape (direction only):

```
LegalDocumentReference        : identifies a document
ReferenceSegment              : (kind, value)
LegalReference                : ordered [ReferenceSegment]
FullyQualifiedLegalReference  : (LegalDocumentReference, LegalReference)
```

All four are intended to be immutable value objects, consistent with the
existing FiscalOS value-object style.

## Examples

- Document: `Legea 227/2015` (Codul Fiscal).
- Reference: `[Article 47, Paragraph 3, Letter b]` → "Art. 47 alin. (3) lit. b)".
- Fully qualified: `Legea 227/2015 / Art. 47 alin. (3) lit. b)`.

Relationships (purely structural, by segment path):

- `Parent([Art 47, para 3, lit b])` = `[Art 47, para 3]`.
- `[Art 47]` `IsAncestorOf` `[Art 47, para 3, lit b]` → true.
- `[Art 47, para 3, lit b]` `IsDescendantOf` `[Art 47]` → true.
- `[Art 47, para 3]` `Contains` `[Art 47, para 3, lit b]` → true.
- `[Art 47, para 3]` `Contains` `[Art 48]` → false.

## Required Operations

Defined on the structural address (path of segments); for
`FullyQualifiedLegalReference` they apply only within the same
`LegalDocumentReference`.

- **`Parent()`** — the address with its last segment removed (the enclosing
  provision); none when already at the topmost segment.
- **`IsAncestorOf(other)`** — true when this address is a strict prefix of
  `other` (this strictly encloses `other`).
- **`IsDescendantOf(other)`** — true when `other` is a strict prefix of this
  (this is strictly nested within `other`); the inverse of `IsAncestorOf`.
- **`Contains(other)`** — inclusive containment: true when this address equals
  `other` **or** `IsAncestorOf(other)`.

All operations are pure prefix/path comparisons — deterministic, with no access
to content or interpretation.

## Consequences

- Provides a precise, immutable, comparable address for legal locations, and
  structural containment/hierarchy reasoning, without touching content,
  authority, or conclusions.
- Preserves the separation discipline: addressing (structure) is distinct from
  authority (`SourceAuthority`), conflict (`ConflictResolver`), purpose
  (`PurposeGraph`), and conclusions (rules/regimes).
- Gives ingestion a structural way to address extracted document locations, and
  gives `LegalCitation` a future, richer alternative to its flat `Article`
  string — though that integration is deliberately deferred.
- Pure structural operations are straightforward to test deterministically.

## Deferred Decisions

- Integration with `LegalCitation` (whether a citation adopts, embeds, or is
  derived from a `FullyQualifiedLegalReference`).
- Versioning of `LegalDocumentReference` (which consolidated version of a
  document an address resolves against) — ties to the ingestion versioning open
  question in ARCH-0005 / ARCH-0006.
- The exact `ReferenceSegment` kind taxonomy (article / paragraph / point /
  letter / annex / …) and any parsing of human-readable references — no parser
  (out of scope).
- Equality and normalization rules across differing reference notations.
- Cross-document references (addresses that point into other documents).
- Whether `FullyQualifiedLegalReference` participates in `ConflictResolver` or
  `DecisionLegalBasis`.

## Out of Scope

LegalGraph, PracticeGraph, LegalAtom, NLP, AI extraction, citation discovery,
and rule generation. No code, no domain types, no runtime behaviour changes are
introduced by this document.
