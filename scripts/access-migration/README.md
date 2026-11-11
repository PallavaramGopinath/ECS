# Access (TSIS) → ECS PostgreSQL migration

One-off data load of a legacy TSIS Access file (`Tsis_MRF_2026_11_11.mdb`) into the `ECS` database.
Run on 2026-11-11. **No application code depends on this folder.**

## Rules applied
- All IDs become 12 digits: `11005` preamble + 7-digit running number (`mem_id` 1 → `110050000001`).
  Applies to PG `numeric`/`bigint` columns named `id`, `*_id`, `referid`, `calc_yrid` (incl. `voc_id`, `usr_id`, `yr_id`).
  0 → `110050000000`; NULL stays NULL; negative sentinels (-1, -2) are left unchanged. `integer` ID columns are copied as-is.
- `brcode` = `11005` and `voc_status` = `V` on every row.
- Not touched: `account_transactions`, `account_voctype`, `bank_master`, `business_day`, `gen_bank_name`,
  `menu_forms`, `menu_main`, `menu_sub`, `pay_component_assignments`, `pay_components`, plus app/config
  tables listed as `KEEP` in `convert.py` (users, reports_*, map_general, lockers masters, …).
- Tables with old society data and no Access source are emptied (see `mapping-report.md`).
- The Access schema differs from ECS; renamed tables/columns are mapped in `TABLE_MAP` / `COLMAP`.
  See `mapping-report.md` for the per-table result.

## Re-running
Requires `brew install mdbtools`.

```bash
W=/path/to/workdir; mkdir -p $W/survey
PGPASSWORD=... psql -h localhost -U postgres -d ECS -AtF'|' -c "select c.table_name,c.column_name,c.data_type,c.is_nullable,coalesce(c.column_default,''), coalesce((select 'PK' from information_schema.key_column_usage k join information_schema.table_constraints tc on tc.constraint_name=k.constraint_name and tc.table_name=k.table_name where tc.constraint_type='PRIMARY KEY' and k.table_name=c.table_name and k.column_name=c.column_name limit 1),'') from information_schema.columns c where c.table_schema='public' order by c.table_name,c.ordinal_position" > $W/survey/pg_meta.txt
python3 -I convert.py $W /path/to/Tsis.mdb          # writes $W/out/*.csv, $W/load.sql, $W/report.md
pg_dump -Fc -d ECS -f ECS_before.backup              # back up first
psql -d ECS -f $W/load.sql                           # single transaction; any error rolls back
```

## Known follow-ups
- `business_day` (excluded) still references an old `yr_id` not present in the new `fin_yr_master`.
- `map_general` / `map_suspenseaccounts` were kept and still hold pre-migration ledger IDs.
