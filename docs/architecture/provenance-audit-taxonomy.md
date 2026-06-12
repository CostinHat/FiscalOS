# Provenance and Audit Event Taxonomy

## Distinction

Provenance records lineage and origin.

Audit records event history, decisions, failures, retries, replay, and correction.

## Ingestion Audit Events

- Batch created.
- Batch started.
- Configuration snapshot selected.
- Source selected.
- Source skipped.
- Source metadata snapshot created.
- Source acquisition started.
- Candidate discovered.
- Candidate fetched.
- Candidate skipped.
- URI/content/metadata normalized.
- Fingerprint computed.
- Raw document identity decision made.
- Duplicate candidate detected.
- Failure recorded.
- Retry scheduled.
- Retry attempted.
- Batch completed.
- Repository handoff package created.

## Resolution Audit Events

- Resolution triggered.
- Legal reference candidate created.
- Reference normalized.
- Resolution policy selected.
- Target corpus snapshot selected.
- Source hierarchy context selected.
- Resolution attempt started.
- Candidate match found.
- Candidate match rejected.
- Resolution output created.
- Output marked resolved.
- Output marked unresolved.
- Output marked ambiguous.
- Correction created.
- Output superseded.
- Replay comparison created.

## Audit Requirements

- Events are append-oriented.
- Events include correlation IDs where possible.
- Events include policy and snapshot IDs when decisions depend on them.
- Secrets and sensitive payloads must be redacted.

