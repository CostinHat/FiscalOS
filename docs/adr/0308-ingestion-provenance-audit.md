# FOS-0308 Ingestion Provenance and Audit Model

Status: accepted.

## Decision

Separate provenance from audit for ingestion.

## Provenance

Provenance records lineage: source, acquisition locator, batch, snapshots, identity decisions, and raw document linkage.

## Audit

Audit records operational events: what happened, when, why, and with which outcome.

## Requirements

- Audit is append-oriented.
- Provenance remains durable as long as related identity is retained.
- Failures, retries, identity decisions, and skipped candidates are explicit.

## References

- [Provenance and Audit Event Taxonomy](../architecture/provenance-audit-taxonomy.md)

