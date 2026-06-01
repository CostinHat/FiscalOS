\# FOS-0016 Taxpayer Classification



\## Goal



Classify a FiscalSubject into a taxpayer category.



\## Inputs



\- Revenue

\- EmployeeCount



\## Outputs



\- TAXPAYER\_CLASSIFICATION = MICROENTERPRISE

\- TAXPAYER\_CLASSIFICATION = SME

\- TAXPAYER\_CLASSIFICATION = LARGE



\## Rules



\### Microenterprise



Revenue <= 500000

AND EmployeeCount >= 1



\### SME



Revenue > 500000

AND Revenue <= 50000000



\### Large



Revenue > 50000000

