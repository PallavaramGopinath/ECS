# Societies directory import

Imported the `ECS` sheet of `/Users/manojg/Downloads/ECS_Societies_Codes_Review.xlsx` into `public.societies` on 2026-11-27. Source SHA-256: `88647c7fa84404afa6aa8a962da7d895be30dd81ecc869ac18f0d2c33afa0cc7`.

- DDL: `21-societies-directory.sql`.
- Exporter: `scripts/societies-directory/export_csv.py`.
- Imported 1,637 rows; all 1,637 rows read back from PostgreSQL matched the exported workbook values in every imported field.
- The source `BRCode` column was empty and is absent from the new table. `proposed_combined_society_code` is retained as a distinct, unique review field. The LUCAS TVS row has proposed code `34-063-1429`.
- `bank_account_number` and `ifsc_code` are text. The workbook contains review placeholders, multiple account numbers in one cell, mixed-case IFSC values, and malformed IFSC values. They are preserved exactly and should be checked before payment use.
- This directory is not currently wired to login, tenant filtering, or `gen_bank_name`.
