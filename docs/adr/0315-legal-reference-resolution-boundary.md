# FOS-0315 Legal Reference Resolution Boundary Contract

Status: accepted.

## Decision

Define Legal Reference Resolution as downstream of raw document ingestion and repository handoff.

## Responsibilities

- Consume raw document identity and versions.
- Consume provenance, audit, snapshots, and policy references.
- Produce legal reference candidates and resolution outputs.

## Boundaries

Resolution does not acquire documents, rewrite raw identity, build source hierarchy graphs, use AI/NLP, or generate rules.

## References

- [Ingestion-to-Resolution Boundaries](../architecture/ingestion-to-resolution-boundaries.md)

