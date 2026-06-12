# FOS-0310 Ingestion Configuration Snapshot Model

Status: accepted.

## Decision

Define immutable, redacted ingestion configuration snapshots.

## Captures

- Source filters.
- Retry policy reference.
- Normalization policy reference.
- Fingerprint policy reference.
- Limits and operational settings.
- Redaction policy reference.

## Secret Handling

Snapshots must not store secrets, tokens, passwords, raw authorization headers, cookies, or private credentials.

## References

- [Snapshot Model Matrix](../architecture/snapshot-model-matrix.md)

