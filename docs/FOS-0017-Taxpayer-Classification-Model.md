\# FOS-0017 Taxpayer Classification Model



\## Purpose



Definește modelul canonic pentru clasificarea contribuabililor.



\## TaxpayerCategory



\- Individual

\- LegalEntity

\- Microenterprise

\- SME

\- LargeTaxpayer



\## VATStatus



\- VATRegistered

\- NonVATRegistered



\## ClassificationResult



Conține:



\- TaxpayerCategory

\- VATStatus

\- ReasonCodes



\## Domain Events



\- TaxpayerClassified

\- TaxpayerClassificationChanged



\## Next Steps



FOS-0018 - Classification Rules

