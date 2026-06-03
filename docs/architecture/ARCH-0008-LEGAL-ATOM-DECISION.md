# ARCH-0008 LegalAtom Decision

## Decision

LegalAtom remains a planning term.

It will not become a first-class implemented model yet.

## Rationale

FiscalOS already has implemented concepts that cover the current legal reasoning path:

- LegalCitation
- Regime definitions
- ClassificationRule
- DecisionLegalBasis
- ConflictResolver

Introducing LegalAtom now would create a parallel abstraction before ingestion has proven the need for it.

## Consequences

Current runtime and curated-rule paths remain unchanged.

Architecture documents may continue to use LegalAtom as planning terminology, but implementation work should not introduce a LegalAtom model until a concrete ingestion or review workflow requires it.

## Follow-up

Future ingestion work should revisit this decision when candidate extraction, human review, and curated promotion need a stable intermediate representation.
