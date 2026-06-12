# Replay and Idempotency Rules

## Idempotency

Repeated execution of the same logical ingestion or resolution operation should not create duplicate canonical identities.

Audit events may be added for repeated work. Stable identity outcomes should remain stable when inputs, policies, and snapshots are unchanged.

## Replay

Replay is explicit re-execution or re-evaluation of historical context.

Replay must record whether it uses:

- Same historical context.
- Updated configuration context.
- Updated source metadata context.
- Updated target corpus context.
- Updated resolution policy context.

## Rules

- Retry success does not delete failure history.
- Replay results do not overwrite original outcomes.
- Corrections are additive.
- Supersession preserves historical outputs.
- Batch IDs are never raw document identity.
- Repository write retries do not regenerate raw document identity.

