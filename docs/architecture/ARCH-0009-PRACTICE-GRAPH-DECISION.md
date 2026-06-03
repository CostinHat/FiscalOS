# ARCH-0009 PracticeGraph Decision

## Decision

PracticeGraph remains a planning concept.

It should be treated as independent from LegalGraph, not as a specialization of it.

## Rationale

LegalGraph represents curated legal knowledge: citations, regimes, rules, and their relationships.

PracticeGraph represents observed administrative or judicial practice: inspection reports, assessments, appeals, authority positions, and court outcomes.

These are different knowledge layers.

FiscalOS should not treat authority practice as the law itself. Practice can inform risk, likelihood, and explanation, but it should remain separate from the curated legal model.

## Consequences

No PracticeGraph runtime model is introduced yet.

Architecture documents may continue to use PracticeGraph as planning terminology.

Future implementation should keep legal authority and observed practice distinct, even when they are connected by citations, disputes, or audit outcomes.

## Follow-up

Future inspection, appeal, or court-outcome modeling should revisit this decision when FiscalOS needs to represent administrative practice as structured data.
