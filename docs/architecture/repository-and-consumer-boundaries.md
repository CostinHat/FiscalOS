# Repository and Consumer Boundaries

## Repository Boundary

Repositories are downstream storage boundaries. They may store raw documents, snapshots, fingerprints, provenance records, audit records, and resolution artifacts.

Repositories must not redefine:

- Source identity.
- Raw document identity.
- Legal reference identity.
- Resolution output identity.
- Future rule identity.

Repository IDs and storage paths are access references only.

## Storage Handoff

The ingestion handoff package should include:

- Raw document content or content reference.
- Raw document identity and version identity.
- Identity decision outcome.
- Source linkage.
- Provenance and audit linkage.
- Snapshot references.
- Fingerprints and policy references.
- Batch and replay context.

## Downstream Consumer Boundary

Consumers read resolution outputs and may build projections. They do not own canonical records.

Consumers must preserve:

- Active and historical resolution outputs.
- Corrected and superseded records.
- Unresolved and ambiguous outcomes.
- Provenance and audit links.

Consumers must not treat resolved references as generated legal rules.

