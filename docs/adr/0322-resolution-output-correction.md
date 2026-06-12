# FOS-0322 Resolution Output and Correction Model

Status: accepted.

## Decision

Define durable resolution outputs and additive correction/supersession behavior.

## Output Types

- Resolved.
- Unresolved.
- Ambiguous.

## Correction Rules

- Corrections are additive.
- Supersession preserves history.
- Replay comparison does not overwrite original outcomes.

## References

- [Repository and Consumer Boundaries](../architecture/repository-and-consumer-boundaries.md)

