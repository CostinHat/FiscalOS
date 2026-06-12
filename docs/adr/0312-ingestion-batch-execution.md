# FOS-0312 Ingestion Batch Execution Contract

Status: accepted.

## Decision

Define the ingestion batch as the orchestration boundary.

## Responsibilities

- Own batch identity and lifecycle.
- Select sources.
- Create or reference snapshots.
- Coordinate acquisition.
- Coordinate normalization and fingerprinting.
- Ensure raw document identity decisions.
- Emit provenance and audit.
- Track failures, retries, and summaries.

## Boundaries

Batches orchestrate; they do not hard-code source-specific behavior or generate legal meaning.

## References

- [Ingestion-to-Resolution Boundaries](../architecture/ingestion-to-resolution-boundaries.md)

