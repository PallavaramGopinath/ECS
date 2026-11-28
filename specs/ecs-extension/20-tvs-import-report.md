# LUCAS TVS Access import, 2026-11-27

## Source and scope

- Source: `/Users/manojg/Documents/ECS_Documents/TVS_ForTesting.mdb` (SHA-256 `a20817d479a40843712c2a01ea81ba03c1a50de67075370cae52d0a4ac4cfbdd`).
- Target: the configured `ECS` PostgreSQL database, public schema; TVS rows use `brcode = '34-063-1429'`.
- The existing Access converter was run in append mode. Numeric IDs use `1429 × 10,000,000 + source ID`, which fits the target `numeric(12)` columns. Tenant-owned integer IDs and matching references use an offset of `1429000000`.
- A pre-import full PostgreSQL backup is at `/private/tmp/tvs-import/ecs-before-tvs.dump`. Move it to approved durable backup storage before the temporary directory is cleared.

## Loaded and checked

- 1,753,233 rows in 76 mapped application tables, plus one `gen_bank_name` row for LUCAS TVS, loaded in a single transaction.
- All 76 TVS table counts equal the converted Access row counts. Ten core loan, member, deposit, demand, voucher, and ledger reference checks found no missing target rows.
- Deposit, loan, member transaction, voucher, and demand amount totals matched the converted source within double-precision rounding.
- The three existing `11005` users remain. Its member and loan counts stayed at 1,303 and 6,367 respectively.
- `mem_payable.payment_amount`, `mem_payable.closing_balance`, `mem_trn.intcalc_amt`, and `mem_trn.intpaid_amt` changed from integer to double precision to retain source fractions. Their EF model properties were changed to `double`.
- An inactive `Admin` user was created for `gopulu.nath@gmail.com` under the TVS BrCode. The random password was discarded; activation requires a new credential issue and first-login change flow.

## Not imported or ready for use

- 173 Access tables have no mapped ECS application target; 104 of those are nonempty (68,046 rows). They include old snapshots, work tables, reports, and legacy modules such as `Loan_demand`. They were not silently copied into unrelated public tables. The conversion's detailed mapping is in `/private/tmp/tvs-import/report.md`.
- TVS has no `business_day` or `map_general` row. The Access file does not contain a direct source for those application tables. Financial operations must not be enabled until their values are established and reconciled.
- Current application ID and account-number generators still assume a numeric `BrCode`. `34-063-1429` will fail those paths; widening the database columns and importing data does not make demand, recovery, or disbursement safe to run.
- The existing database backup did not restore all constraints cleanly in the validation copy: a pre-existing `pay_component_assignments.employee_id` refers to a missing `mem_master.mem_id`. The TVS load itself succeeded with normal PostgreSQL constraints enabled.
- Full solution build verification is pending. The modified Models project built successfully; the full solution build did not complete in the local run.

## Reproduce the conversion

Use the existing converter with a fresh survey of target metadata, then review its report and load SQL before execution. For this source the environment was:

```text
ECS_IMPORT_BRCODE=34-063-1429
ECS_IMPORT_ID_PREFIX=14290000000
ECS_IMPORT_INT_OFFSET=1429000000
ECS_IMPORT_APPEND=1
ECS_IMPORT_SEED_SQL=specs/ecs-extension/20-tvs-society-seed.sql
```

Do not run the converter's default replacement mode on the shared database.
