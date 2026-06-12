# ARCH-0007 Terminology Reconciliation

## Goal

The goal of this document is to reconcile planning terminology with implemented FiscalOS terminology.

It identifies differences between:
- implemented concepts,
- architecture planning concepts,
- future ingestion concepts,

and records decisions required to keep architecture, documentation, and implementation aligned.

## Current Implemented Terms

- LegalCitation
- ClassificationRule
- ClassificationResult
- DecisionLegalBasis
- ConflictResolver
- PurposeGraph
- AuditGraph
- KnowledgeProjectionResult
- Jurisdiction
- Regime definitions

## Planning Terms

- LegalGraph
- LegalAtom
- PracticeGraph
- PurposeGraph expansion
- AuditGraph expansion
- Legislation ingestion pipeline
- Candidate extraction
- Curated promotion

## FOS-0328 Hardening

`LegalGraph` and `LegalAtom` remain conceptual or future architecture terms.
They are not implemented ingestion artifacts, graph structures, graph traversal
features, AI/NLP outputs or rule-generation artifacts.

New ingestion and resolution documentation uses more precise identity terms:

- source identity,
- raw document identity,
- legal reference identity,
- repository reference,
- future rule identity.

These identities must remain separate. `LegalReference` in
ARCH-0011 describes a structural address concept; `legal reference identity` in
the FOS-0316+ documentation describes the durable identity of a reference
observation or entity.

## Terminology Gaps

- LegalGraph is referenced in planning documents but does not exist as an implemented model.
- LegalAtom is referenced in planning documents but does not exist as an implemented model.
- PracticeGraph is planned but not implemented.
- The relationship between LegalCitation and future LegalAtom concepts is not yet defined.
- The relationship between current regime definitions and future LegalGraph concepts is not yet defined.

## Open Questions

- Should LegalAtom become a first-class implemented model?
- Should LegalGraph be an implemented runtime structure or a documentation concept?
- Should regime definitions evolve into LegalAtom instances or remain separate concepts?
- Should PracticeGraph be independent from LegalGraph or a specialization of it?
- Which terminology should be considered canonical for future ingestion work?
