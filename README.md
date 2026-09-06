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

The next objective is to define and deliver a narrow, measurable,
production-grade vertical slice using a real authoritative source. Its concrete
source, supported question, persistence technology, API technology, deployment
model and commercial packaging remain open decisions.
