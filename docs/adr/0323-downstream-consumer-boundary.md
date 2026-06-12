# FOS-0323 Downstream Consumer Boundary Contract

Status: accepted.

## Decision

Define downstream consumers as readers of resolution outputs and projections.

## Responsibilities

- Consume active and historical resolution outputs.
- Preserve canonical identity links.
- Handle corrections, supersession, unresolved, and ambiguous states.
- Avoid repository and source hierarchy internals.

## Exclusion

Resolution outputs are not generated rules.

## References

- [Repository and Consumer Boundaries](../architecture/repository-and-consumer-boundaries.md)

