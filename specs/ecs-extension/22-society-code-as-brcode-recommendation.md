# Recommendation: use the statewide Society Code as the ECS BrCode

**Status:** Implemented for TVS on 2026-11-27. This document records the identifier decision in `19-multi-society-login-and-access-plan.md` for new ECS societies. The TVS operational migration is in `22-tvs-brcode-migration.sql`, with checks in `22-tvs-brcode-preflight.sql` and `22-tvs-brcode-verify.sql`.

## Recommendation

Yes: use each society's unique, permanent **Society Code** as its operational `BrCode`, represented as a four-character string for new ECS societies. For example, Society Code `1429` becomes `BrCode = '1429'`; Society Code `7` becomes `BrCode = '0007'`. Keep the code unchanged if the society moves to another circle or region. Do not use the workbook's `S.No`: LUCAS TVS is serial number `990` but Society Code `1429`.

The reviewed workbook contains 1,637 distinct Society Codes, covering integers 1–1,637, and 1,637 distinct proposed combined codes. The separate `societies` directory already stores `society_code`, `region_code`, and `circle_code`; its `BRCode` source column was empty. The four-character convention can cover 1–9,999, but a later expansion beyond that range would need an explicit new rule. Store `BrCode` as **text**, preserving leading zeroes; never convert `'0007'` to an integer when using it as a tenant key.

| Use | Value for LUCAS TVS | Meaning |
| --- | --- | --- |
| `societies.society_code` | `1429` | Stable statewide society identity |
| Proposed operational `BrCode` | `'1429'` | Society data and login scope after migration |
| `societies.region_code` | `34` | Current regional MIS grouping |
| `societies.circle_code` | `63` | Current circle MIS grouping |
| `societies.proposed_combined_society_code` | `34-063-1429` | Human-readable directory/MIS reference, not a tenant key |

## Why this is simpler

The society remains identifiable without decoding its region or circle from `BrCode`. A regional or circle report can find the permitted societies in `societies` and then aggregate their operational rows. Reassigning a society to another circle changes its hierarchy assignment, not its login scope or transaction keys. A numeric four-character `BrCode` also fits the existing code paths that parse or concatenate a numeric branch code; the hyphenated value does not.

This does **not** mean a code change can be made by updating only `users.brcode`. The current code builds numeric IDs and account numbers by concatenating `BrCode` with suffixes. Some paths also derive scheme IDs from it. Those rules must be inventoried and tested for four-character ECS codes, including leading-zero codes. A numeric ID cannot retain the zeroes in a `'0007'` prefix, so collision checks must use the stored numeric values, not just formatted strings. The TVS import assigned numeric IDs using the `1429` prefix, but its integer scheme IDs and some other identifiers were separately remapped; they must not be assumed to match `BrCode + suffix` rules.

## MIS design

Use `societies.society_code` as the stable society key and `region_code`/`circle_code` as directory attributes. Enforce a unique constraint on `societies.society_code` before using it as a key; the current data are unique, but the table definition does not yet enforce that. For operational reporting, resolve each application's `BrCode` to exactly one `societies` row through a controlled crosswalk (for example, a separate `society_operational_codes` table). Keep `societies` free of a `brcode` column, as requested. The crosswalk is important during migration and for the existing five-character `11005` test branch. Do not join by converting arbitrary `BrCode` text to an integer.

A Regional Joint Registrar should receive a read-only region assignment; a Circle Deputy Registrar should receive a read-only circle assignment. The server, not a browser-supplied code, determines the permitted society set and filters every report/export to it. Society users remain limited to their own society. Reconcile circle totals to society totals and region totals to circle totals. The current `societies` row records only the **current** hierarchy. If historical MIS must reflect the hierarchy as it stood on a past date, add effective-dated assignment history and use the report's as-of date.

## Existing data and required migration

The configured ECS database has operational data for `11005` and TVS. TVS's 1,991 member rows, user, society master, and other operational rows now use `BrCode = '1429'`. The existing `11005` test branch remains unchanged. TVS loan, term deposit, and SBCA scheme IDs were widened to bigint and remapped to the 11-digit `1429` prefix plus a seven-digit running number, together with their references. Other imported IDs and account numbers were already in the target range and were preserved. A pre-migration backup is stored in `.db-backups/ECS-before-4digit-20261127.dump`.

Before switching TVS to `'1429'`:

1. Confirm the Society Code register is authoritative, permanent, and will not reuse codes. Add database uniqueness for `societies.society_code` and a validated one-to-one operational crosswalk.
2. Inventory every TVS `BrCode` occurrence across society-owned tables, users, configuration, staged records, reports, and stored payloads. Audit generated account numbers, scheme references, numeric IDs, and external interfaces for old-code assumptions.
3. Fix generators and lookups that require a numeric or fixed-width `BrCode`; preserve the already-imported IDs and account numbers unless a separately reconciled migration requires changing them. In particular, do not infer scheme IDs from `BrCode` when they are stored as independent keys.
4. Take a durable backup and rehearse a transactional, all-table TVS `BrCode` update in a restored database. Verify row counts, references, logins, financial totals, and report scoping before and after; prepare a rollback.
5. Complete tenant isolation, business-day setup, and required master mappings before operational testing. A shorter `BrCode` alone does not make demand calculation, recovery appropriation, or disbursement safe.

**Decision implemented:** for new ECS societies, use the immutable Society Code rendered as four-character text as `BrCode`. TVS was migrated from `34-063-1429` to `1429` after backup, rehearsal, and verification; legacy `11005` was retained. The proposed combined code remains available for MIS display and cross-checking, not for operational filtering.
