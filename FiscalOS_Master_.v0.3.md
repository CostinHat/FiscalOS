# FiscalOS_Master

Version: 0.3
Date: 2026-06-01
Status: Active

## Executive Summary
FiscalOS este un sistem de reprezentare, evaluare și explicare a cunoașterii fiscale.

Obiectivul său este transformarea legislației fiscale în cunoaștere executabilă, explicabilă, auditabilă și versionată.

FiscalOS este construit pe baza unui Legal Knowledge Engine care transformă sursele legislative în reguli fiscale executabile.





## Current Project Status

- Foundation: Completed
- Rule Engine: Operational
- API Layer: Operational
- Sprint 0: Completed
- Sprint 1: Completed
- M0 Legal Knowledge Engine: Planned
- Current Milestone: MILESTONE-0003

## M0 – Legal Knowledge Engine

### Purpose
Legal Knowledge Engine reprezintă fundația FiscalOS.

Acesta transformă legislația fiscală, normele metodologice,
ordinele ANAF și alte surse oficiale în cunoaștere structurată
și executabilă.

Rezultatul final este un set de reguli fiscale versionate,
auditabile și explicabile care pot fi evaluate de FiscalOS.
### Principles

- Legea este sursa adevărului.
- Orice decizie trebuie să fie explicabilă.
- Regulile trebuie să fie versionate.
- AI-ul nu este sursa adevărului.
- FiscalOS trebuie să fie auditabil.

### High Level Architecture

Legislation
↓
Legal Source Repository
↓
Legal Fragment Extraction
↓
Rule Candidate Generation
↓
Rule Review & Approval
↓
Rule Version Repository
↓
Rule Engine
↓
FiscalOS API

## Approved Milestones
### MILESTONE-0001
Foundation Established

Artefacts:
- FOS-0000 Project Charter
- FOS-0003 Canonical Vocabulary
- FOS-0004 Evaluation Model
- FOS-0005 Inference Model
- FOS-0006 Explanation Graph
- FOS-0007 Constitution
- FOS-0008 FRL

### MILESTONE-0002
Executable Architecture (In Progress)

## Aggregate Roots
- FiscalSubject
- Relationship
- RuleDefinition
- Ruleset
- Evaluation
- ObligationInstance

## Special Categories
### Domain Events
Universal Transition Mechanism

### Audit Artifacts
ExplanationGraph
Owner: Evaluation

## Major Decisions
- Persistăm fapte, nu concluzii.
- Events are the Source of History.
- Explanation Graph este produsul principal al unei evaluări.
- Ruleset este unitatea de publicare a cunoașterii.
- Rule Dependency Graph este DAG.

## Next Starting Point
FOS-0013 – Entity Model v1.0

Chapter 1:
FiscalSubject Entity

Subiecte:
- Identity
- Business Keys
- Attributes
- Invariants
- Lifecycle
- Domain Events

## Session Closing State
Project Status: Healthy
Architectural Consistency: High
Implementation Readiness: High
