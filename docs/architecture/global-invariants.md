# Global Architecture Invariants

These invariants apply across FOS-0306 through FOS-0326.

## Identity Invariants

- Source identity is not raw document identity.
- Raw document identity is not legal reference identity.
- Legal reference identity is not resolution output identity.
- Repository IDs and storage paths are not canonical legal or document identity.
- Batch IDs are operational context, not document or reference identity.
- Future rule identity must remain separate from resolved references.
- ARCH-0011 `LegalReference` structural addresses are distinct from FOS-0316
  legal reference identity records.

## Boundary Invariants

- Sources acquire; batches orchestrate.
- Ingestion identifies and traces raw documents; it does not resolve legal meaning.
- Legal Reference Resolution consumes ingestion outputs; it does not generate rules.
- Source Hierarchy is exposed as context only; graph storage and traversal are future work.
- Repositories store and retrieve; they do not redefine identity.
- Consumers read projections; they do not mutate canonical records.
- `LegalGraph` and `LegalAtom` are conceptual or future architecture terms, not
  implemented ingestion artifacts.

## Snapshot Invariants

- Snapshots are immutable once referenced.
- Snapshots are schema-versioned.
- Configuration snapshots must be redacted and must not store secrets.
- Historical snapshots remain interpretable after later configuration or policy changes.
- Replay must state whether it uses historical context or updated context.

## Audit And Provenance Invariants

- Provenance records lineage and origin.
- Audit records event history and accountability.
- Retries, replays, corrections, and supersessions are additive.
- Historical records are not silently rewritten.
