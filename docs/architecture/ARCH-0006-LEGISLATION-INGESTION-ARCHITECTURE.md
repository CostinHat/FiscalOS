# ARCH-0006 Legislation Ingestion Architecture

## Goal

Define the architecture for turning external legislation sources into reviewed, curated FiscalOS legal knowledge.

This is a planning document only.

Terminology note: this legacy planning document uses `LegalGraph` and
`LegalAtom` as conceptual or future architecture terms. They are not
implemented ingestion artifacts, and this document does not authorize graph
implementation, graph traversal, AI/NLP integration, or automatic rule
generation.

## Non-Goals

- No parser implementation.
- No NLP implementation.
- No AI extraction implementation.
- No rule generator.
- No database.
- No API.
- No runtime ingestion path.
- No automatic promotion of legal content into executable rules.

## Pipeline Stages

1. Source discovery
2. Source acquisition
3. Source versioning
4. Text normalization
5. Citation detection
6. Candidate legal atom extraction
7. Human review
8. Curated promotion
9. Rule binding

## Human Review Boundary

Candidate extracted content is not trusted legal knowledge.

Human review is required before any extracted content becomes part of curated
FiscalOS legal knowledge. In this legacy document, `LegalGraph` means a future
or conceptual curated legal-knowledge architecture term, not an implemented
runtime graph or ingestion artifact.

## Promotion Boundary

Only reviewed and accepted content may be promoted into curated FiscalOS knowledge.

Promotion may create or update:

- Legal citations
- Legal atoms (conceptual/future term only)
- Regime definitions
- Classification rules
- Explanation references

## Open Questions

- How should source versions be identified?
- How should amendments be represented?
- How should partial extraction confidence be stored?
- How should competing legal interpretations be represented before review?
- How should promoted rules retain provenance back to raw source material?
