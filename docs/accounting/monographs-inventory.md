# Accounting monographs inventory

This document inventories accounting monograph mappings currently observed in the legacy Python prototype.

It is documentation only. It does not define FiscalOS rules yet.

## Issued invoices

| Category | Revenue account | Description |
|---|---:|---|
| servicii | 704 | Factura emisa servicii |
| marfa | 707 | Factura emisa marfa |
| chirie | 706 | Factura emisa chirie |
| produse | 701 | Factura emisa produse |

Source text used by the prototype:

> Recunoastere venituri cf. OMFP 1802/2014 (functiunea conturilor) si TVA colectata cf. Cod fiscal (Legea 227/2015) art. 268-291.

## Received invoices

Default supplier account:

| Case | Supplier account |
|---|---:|
| default | 401 |
| mijloace_fixe | 404 |

Source text used by the prototype:

> Recunoastere cf. OMFP 1802/2014 (functiunea conturilor) si TVA deductibila cf. Cod fiscal (Legea 227/2015) art. 297-300.

## Notes

The prototype has one legal source text for issued invoices and one legal source text for received invoices.

The source text does not vary per category.
