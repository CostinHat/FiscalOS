# FOS-0307 Raw Document Identity Model

Status: accepted.

## Decision

Define durable raw document identity for acquired artifacts, separate from source, repository, legal reference, and future rule identities.

## Responsibilities

- Assign stable raw document lineage identity.
- Assign raw document version identity for observed content/version state.
- Link to source, provenance, batch, snapshots, and fingerprints.
- Support duplicate detection and replay.

## Required Concepts

- Raw document ID.
- Raw document version ID.
- Source ID.
- Source document ID.
- Retrieval URI.
- Content hash and algorithm.
- Provenance ID.
- Ingestion batch ID.

## Boundaries

Content hash is evidence, not the complete identity. Batch ID and repository ID are not document identity.

## References

- [Identity Model Matrix](../architecture/identity-model-matrix.md)

