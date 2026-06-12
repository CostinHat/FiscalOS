# FOS-0314 Repository Boundary and Storage Handoff Contract

Status: accepted.

## Decision

Define repository handoff as a downstream storage boundary.

## Handoff Contents

- Raw document content or reference.
- Raw document identity and version.
- Identity decision outcome.
- Provenance and audit linkage.
- Snapshot references.
- Fingerprints and policy references.
- Batch and replay context.

## Boundary

Repositories store and retrieve; they do not redefine identity.

## References

- [Repository and Consumer Boundaries](../architecture/repository-and-consumer-boundaries.md)

