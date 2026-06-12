# FOS-0313 Idempotency and Replay Contract

Status: accepted.

## Decision

Define repeat-safe ingestion and resolution behavior.

## Idempotency

Repeated logical operations should not create duplicate canonical identities.

## Replay

Replay is explicit re-execution or re-evaluation of historical context.

## Requirements

- Retries link to original failures.
- Replay does not overwrite history.
- Corrections are additive.
- Same-context replay uses historical snapshots and policies.

## References

- [Replay and Idempotency Rules](../architecture/replay-idempotency-rules.md)

