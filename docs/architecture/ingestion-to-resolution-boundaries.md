# Ingestion-to-Resolution Boundaries

## Ingestion Boundary

Ingestion owns:

- Source selection.
- Source acquisition orchestration.
- Configuration snapshots.
- Source metadata snapshots.
- Raw document normalization and fingerprinting.
- Raw document identity decisions.
- Ingestion provenance and audit records.
- Replay and idempotency context.
- Repository handoff package creation.

Ingestion does not own:

- Legal reference identity.
- Legal reference resolution.
- Source hierarchy graph traversal.
- Rule generation.
- AI/NLP interpretation.

## Legal Reference Resolution Boundary

Legal Reference Resolution owns:

- Consuming raw document identity and versions.
- Creating legal reference identities.
- Normalizing legal reference text into canonical forms.
- Running deterministic resolution attempts.
- Recording resolved, unresolved, and ambiguous outputs.
- Recording additive corrections and supersession.

Legal Reference Resolution does not own:

- Source acquisition.
- Raw document identity assignment.
- Repository storage identity.
- Rule generation.
- Graph implementation.

## Handoff Between Boundaries

The handoff from ingestion to resolution includes:

- Raw document ID.
- Raw document version ID.
- Content hash and hash algorithm.
- Source ID.
- Provenance ID.
- Configuration snapshot ID.
- Source metadata snapshot ID.
- Repository-neutral content reference, if available.

