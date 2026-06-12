# Glossary

## Source Identity

Stable identity for an acquisition source. It is not raw document identity.

## Source Metadata Snapshot ID

Identifier for an immutable source metadata snapshot. It is not source identity.

## Configuration Snapshot ID

Identifier for an immutable, redacted ingestion configuration snapshot.

## Raw Document Identity

Durable identity for an ingested raw document lineage. It is not a source, batch, repository path, or content hash alone.

## Raw Document Version

Identity for a specific observed version of a raw document.

## Provenance

Lineage and origin record linking documents, sources, batches, snapshots, and decisions.

## Audit Event

Append-oriented operational event recording what happened, when, and why.

## Source Metadata Snapshot

Immutable record of source metadata, capability, authority context, adapter version, and acquisition context.

## Configuration Snapshot

Immutable, redacted record of operational settings, filters, limits, retry policy, normalization policy, and fingerprint policy.

## Normalization Policy

Versioned deterministic rules for converting raw values into comparable canonical forms.

## Fingerprint Policy

Versioned rules defining hash input selection and hash algorithms.

## Ingestion Batch

Operational container that orchestrates source selection, acquisition, normalization, identity decisions, provenance, audit, and handoff.

## Idempotency

Property that repeated logical operations do not create duplicate canonical identities.

## Replay

Explicit re-execution or re-evaluation of historical context.

## Repository Handoff

Repository-neutral package of raw document, identity, provenance, audit, snapshot, and fingerprint information prepared for storage.

## Legal Reference Identity

Durable identity for a legal reference observation or entity. It is distinct from raw document identity and future rule identity.

## LegalReference Structural Address

ARCH-0011 planning term for a structural address inside a legal document. It is
distinct from FOS-0316 legal reference identity.

## Canonical Reference Representation

Policy-derived normalized representation of a legal reference. It is not a resolved target.

## Resolution Attempt

Execution record for attempting to resolve a legal reference under a resolution policy and target corpus snapshot.

## Resolution Policy

Versioned deterministic policy defining matching, target selection, ambiguity, and unresolved behavior.

## Target Corpus Snapshot

Immutable reference to the legal reference target set available during a resolution attempt.

## Target Corpus Snapshot ID

Identifier for a target corpus snapshot. It is not a repository identifier.

## Corpus Completeness

Declared status describing whether the target corpus snapshot is complete,
partial, unavailable or otherwise limited for a resolution attempt.

## Source Hierarchy Context

Repository-neutral context reference for authority, jurisdiction, source, or official-status relationships. It is not graph implementation.

## Source Hierarchy Context Reference

Identifier or descriptor for source hierarchy context. It does not expose graph
storage, graph edges or graph traversal.

## Resolution Output

Durable resolved, unresolved, or ambiguous result of a resolution attempt.

## Resolution Output Identity

Identifier for a durable resolution output. It is not legal reference identity
and not future rule identity.

## Additive Correction

New record that corrects a prior output without rewriting historical records.

## Supersession

Additive relationship marking one resolution output as replaced by another
while preserving historical records.

## Candidate Match

Deterministic candidate target considered during a resolution attempt.

## Repository Reference

Repository-specific access handle or storage reference. It is not source, raw
document, legal reference or rule identity.

## Future Rule Identity

Placeholder for a future rule or obligation identity. It is out of scope for
the ingestion and Legal Reference Resolution documentation set.

## Downstream Consumer

System that reads resolution outputs or projections without owning canonical identity or generating rules.
