\# FOS-0018 Classification Rules



\## Purpose



Definește regulile care determină clasificarea contribuabililor.



\## Inputs



\- Legal Form

\- Turnover

\- Employee Count

\- VAT Registration

\- Special Regimes



\## Rules



\### Microenterprise



IF turnover <= threshold

THEN category = Microenterprise



\### SME



IF turnover > micro threshold

AND turnover <= SME threshold

THEN category = SME



\### Large Taxpayer



IF taxpayer is listed in ANAF large taxpayer registry

THEN category = LargeTaxpayer



\## Outputs



\- TaxpayerCategory

\- VATStatus

\- ReasonCodes



\## Next Step



FOS-0019 Classification Evaluation

