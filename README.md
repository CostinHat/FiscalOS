# FiscalOS

FiscalOS is an independent fiscal/legal knowledge product and platform. It owns
its canonical fiscal/legal data, capabilities, stable interfaces, data
lifecycle, and evolution independently of any consumer.

## Strategic baseline

AR-03 establishes FiscalOS as consumer-agnostic and API-first. Its architecture
will support future authorized commercial external consumption through a Public
API, without implying that every capability is public or implemented today.

Production-grade operation requires authoritative, versioned and curated
fiscal/legal data, durable persistence, independently traceable outputs, and
evidence-backed vertical slices. A FiscalOS human interface, if delivered, is a
first-party consumer of the same stable platform boundaries.

The current implementation remains a narrow deterministic in-memory foundation:
durable persistence, production source acquisition, Public API exposure and a
human-facing interface are roadmap requirements, not current capabilities.

## Architecture

The accepted [AR-03](docs/architecture/AR-03.md) strategic baseline supersedes
only the incompatible product-positioning statements in [AR-02](docs/architecture/AR-02.md).
AR-02's internal boundaries remain valid: small generic Legal Core; separate
Ingestion, Legal Knowledge, Resolution and Classification capabilities;
Runtime → Domain dependency direction; and transversal, separately traceable
Assurance/Traceability contracts.

AR-04 accepts the readiness constraints for that objective: semantic
bi-temporality where required, canonical-versus-consumer-scoped data separation,
and an executable Consumer Independence guard. The next step is to operationalize
those constraints and select Vertical Slice 01 against code and a real source;
its concrete source, question and technologies remain open.
