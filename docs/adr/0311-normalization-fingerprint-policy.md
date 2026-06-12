# FOS-0311 Normalization and Fingerprint Policy Model

Status: accepted.

## Decision

Define deterministic, versioned normalization and fingerprint policies for ingestion identity decisions.

## Policy Areas

- URI normalization.
- Content normalization.
- Metadata normalization.
- Hash input selection.
- Hash algorithm policy.

## Requirements

- Preserve raw values separately from normalized values.
- Record policy IDs and versions.
- Use fingerprints as evidence, not full document identity.
- Keep semantic interpretation out of scope.

## References

- [Identity Model Matrix](../architecture/identity-model-matrix.md)

