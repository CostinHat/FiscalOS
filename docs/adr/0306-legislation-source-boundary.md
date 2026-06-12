# FOS-0306 Legislation Source Boundary

Status: accepted.

## Decision

Define `ILegislationSource` as the acquisition boundary for external or upstream legislation sources.

## Responsibilities

- Identify the source.
- Expose source metadata and capabilities.
- Acquire source candidates.
- Return structured acquisition results.
- Preserve source provenance inputs.
- Surface acquisition failures explicitly.

## Boundaries

Sources do not persist records, assign raw document identity, resolve legal references, build hierarchy graphs, use AI/NLP, or generate rules.

## Integration

Source outputs are consumed by ingestion batches, source metadata snapshots, provenance, audit, and raw document identity decisions.

## References

- [Global Architecture Invariants](../architecture/global-invariants.md)
- [Ingestion-to-Resolution Boundaries](../architecture/ingestion-to-resolution-boundaries.md)

