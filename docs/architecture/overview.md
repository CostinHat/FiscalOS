# Ingestion and Legal Reference Resolution Architecture

## Purpose

This document consolidates the FOS-0306 through FOS-0324 planning series into a documentation-ready architecture for ingestion and Legal Reference Resolution.

The architecture keeps acquisition, identity, provenance, repository handoff, legal reference resolution, and downstream consumption as separate responsibilities.

## Flow

1. A configured ingestion batch selects sources.
2. The batch creates or references configuration and source metadata snapshots.
3. Sources acquire candidate raw documents through the source boundary.
4. The batch applies normalization and fingerprint policies.
5. Raw document identity decisions are made and audited.
6. Provenance records connect the document to source, batch, snapshots, and policies.
7. Repository handoff packages are produced for future storage.
8. Legal Reference Resolution consumes stable raw document versions.
9. Reference normalization produces canonical reference representations.
10. Resolution attempts use deterministic policies and target corpus snapshots.
11. Resolution outputs record resolved, unresolved, or ambiguous outcomes.
12. Corrections, supersession, and replay comparisons are additive.
13. Downstream consumers read outputs without owning canonical identity.

## Explicit Exclusions

- No persistence implementation.
- No API implementation.
- No source hierarchy graph implementation.
- No graph traversal implementation.
- No AI/NLP integration.
- No automatic rule generation.
- No graph implementation.

