# Snapshot Model Matrix

| Snapshot | Captures | Required Linkages | Key Rules |
| --- | --- | --- | --- |
| Source metadata snapshot | Source identity, authority metadata, capabilities, adapter version, acquisition context | Source ID, batch ID, configuration snapshot ID | Immutable; does not replace source identity |
| Configuration snapshot | Source filters, retry policy, normalization policy, fingerprint policy, limits, redacted settings | Batch ID, policy refs, redaction policy | Must not store secrets |
| Target corpus snapshot | Available resolution targets and corpus scope | Resolution policy, resolution attempt, hierarchy context ref | Repository-neutral; does not resolve references |
| Source hierarchy context reference | Jurisdiction/authority/source context | Resolution policy, target corpus, resolution attempt | Reference only; no graph implementation |

## Shared Snapshot Requirements

- Immutable once referenced.
- Schema-versioned.
- Replay-compatible.
- Historical meaning preserved after later changes.
- Repository-neutral unless explicitly an access reference.

