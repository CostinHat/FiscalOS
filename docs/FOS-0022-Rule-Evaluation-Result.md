\# FOS-0022 Rule Evaluation Result



\## Scop



Definește structura rezultatului produs de evaluarea unei reguli de clasificare.



\## Intrare



\- RuleDefinition

\- TaxpayerData

\- EvaluationContext



\## Rezultat



\### RuleEvaluationResult



| Câmp | Tip | Descriere |

|-------|-----|------------|

| ruleId | string | Identificatorul regulii |

| ruleName | string | Numele regulii |

| matched | boolean | Regula s-a aplicat |

| score | number | Scorul generat |

| weight | number | Ponderea regulii |

| contribution | number | Contribuția la scorul final |

| explanation | string | Explicația rezultatului |

| executionTimeMs | number | Timp de execuție |

| metadata | object | Informații suplimentare |



\## Statusuri



\### Match



Regula este satisfăcută.



\### NoMatch



Regula nu este satisfăcută.



\### Error



Evaluarea regulii a eșuat.



\## Exemple



\### Match



```json

{

&#x20; "ruleId": "R001",

&#x20; "matched": true,

&#x20; "score": 15,

&#x20; "weight": 2,

&#x20; "contribution": 30

}

