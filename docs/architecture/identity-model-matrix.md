# Identity Model Matrix

| Identity | Purpose | Owner | Stability Rule | Must Not Be Confused With |
| --- | --- | --- | --- | --- |
| Source ID | Identifies acquisition source | Source boundary | Stable across batches | Raw document ID |
| Source metadata snapshot ID | Identifies frozen source metadata | Ingestion | Immutable once referenced | Source ID |
| Configuration snapshot ID | Identifies frozen ingestion config | Ingestion | Immutable and redacted | Runtime config |
| Raw document ID | Identifies raw document lineage | Ingestion identity | Stable across re-ingestion | Content hash alone |
| Raw document version ID | Identifies observed document version | Ingestion identity | Changes with version/content evolution | Batch ID |
| Provenance ID | Identifies lineage record | Provenance model | Durable as long as identity is retained | Audit event ID |
| Audit event ID | Identifies operational event | Audit model | Append-only | Provenance ID |
| Legal reference ID | Identifies reference observation/entity | Resolution | Stable under same raw document/version and policy | Raw document ID |
| Resolution attempt ID | Identifies attempt execution | Resolution | One per attempt/retry/replay | Resolution output ID |
| Resolution output ID | Identifies durable outcome | Resolution | Additive; superseded, not rewritten | Legal reference ID |
| Target corpus snapshot ID | Identifies available target corpus context | Resolution | Immutable once referenced | Repository ID |
| Source hierarchy context ID | Identifies hierarchy context reference | Future hierarchy boundary | Context reference only | Graph identity |
| Repository reference | Identifies storage access location | Repository | Backend-specific | Canonical identity |
| Future rule ID | Identifies rule/obligation if ever implemented | Future rule boundary | Out of scope | Resolved legal reference |

## Canonical Concept Owners

| Concept | Owning Document | Notes |
| --- | --- | --- |
| Source identity | FOS-0306 ADR and this matrix | Acquisition source identity only |
| Raw document identity | FOS-0307 ADR and this matrix | Ingested artifact lineage/version identity |
| Provenance and audit | FOS-0308 ADR and provenance/audit taxonomy | Lineage and event history stay separate |
| Source metadata snapshot | FOS-0309 ADR and snapshot matrix | Acquisition-time source context |
| Configuration snapshot | FOS-0310 ADR and snapshot matrix | Redacted operational context |
| Normalization and fingerprint policies | FOS-0311 ADR | Technical ingestion comparison policies |
| Ingestion batch | FOS-0312 ADR | Orchestration boundary |
| Idempotency and replay | FOS-0313 ADR and replay/idempotency rules | Repeat-safe execution and historical replay |
| Repository handoff | FOS-0314 ADR and repository boundary doc | Storage handoff, not canonical identity |
| Legal Reference Resolution boundary | FOS-0315 ADR | Downstream reference-resolution boundary |
| Legal reference identity | FOS-0316 ADR and this matrix | Reference identity, distinct from ARCH-0011 structural address |
| Legal reference normalization | FOS-0317 ADR | Canonical reference representation, not target resolution |
| Resolution attempt and policy | FOS-0318 and FOS-0319 ADRs | Deterministic matching and outcome execution |
| Target corpus snapshot | FOS-0320 ADR and snapshot matrix | Available target context, not resolution itself |
| Source hierarchy context | FOS-0321 ADR and snapshot matrix | Context reference only; no graph implementation |
| Resolution output and correction | FOS-0322 ADR | Durable outcome, additive correction and supersession |
| Downstream consumer boundary | FOS-0323 ADR | Projection/reading boundary only |
| Documentation consolidation | FOS-0324 ADR | Acceptance and consolidation record |
