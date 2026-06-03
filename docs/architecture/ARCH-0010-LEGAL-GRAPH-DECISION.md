# ARCH-0010 LegalGraph Decision

## Decision

LegalGraph remains a planning concept.

It will not become a first-class implemented runtime model yet.

## Rationale

FiscalOS already has implemented concepts that cover the current legal reasoning path:

- LegalCitation
- Regime definitions
- ClassificationRule
- DecisionLegalBasis
- ConflictResolver
- PurposeGraph
- AuditGraph

Introducing LegalGraph now would create a broad abstraction before ingestion, review, and promotion workflows have proven the exact model boundaries.

## Consequences

No LegalGraph runtime model is introduced.

Architecture documents may continue to use LegalGraph as planning terminology, but implementation work should continue to use the existing concrete models.

## Follow-up

Future ingestion work should revisit this decision when candidate extraction, human review, curated promotion, and legal-source versioning require a stable graph-level representation.
