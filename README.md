# FiscalOS

FiscalOS v0.0.1 starts with one validated vertical slice:

```text
Observed Fact
→ Rule Evaluation
→ Knowledge Assertion
→ Explanation
```

## Run

```bash
dotnet test
```

## First test

`Should_Derive_Microenterprise_Eligibility`

Given:

```text
Revenue = 320000
EmployeeCount = 3
```

Then:

```text
MICROENTERPRISE_ELIGIBLE = true
```
