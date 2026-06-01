\# FOS-0021 Classification Result



\## Purpose



Classification Result reprezintă rezultatul final produs de procesul de clasificare a unui contribuabil.



Owner: Classification Evaluation



\---



\## Responsibilities



\- Capturarea rezultatului final al clasificării

\- Expunerea categoriei determinate

\- Expunerea regulii care a produs clasificarea

\- Expunerea explicației rezultatului

\- Păstrarea trasabilității complete



\---



\## Structure



\### ClassificationResultId



Identificator unic.



\### TaxpayerId



Referință către contribuabilul evaluat.



\### ClassificationCategory



Categoria atribuită.



Exemple:



\- Individual

\- MicroEnterprise

\- SME

\- LargeTaxpayer

\- PublicInstitution

\- NonProfit



\### Confidence



Nivelul de încredere.



Interval:



```text

0.0 .. 1.0

