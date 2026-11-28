-- Read-only audit to run before 22-tvs-brcode-migration.sql.
-- It reports every public BrCode table and every single-column numeric primary key.
-- Decimal/bigint IDs outside 1429 + seven digits need a separate reference-safe migration.

DO $$
DECLARE
    item record;
    old_rows bigint;
    target_rows bigint;
    out_of_range bigint;
BEGIN
    FOR item IN
        SELECT c.table_schema, c.table_name, c.column_name,
               pk.pk_column, pk.data_type
        FROM information_schema.columns c
        JOIN information_schema.tables t
          ON t.table_schema = c.table_schema AND t.table_name = c.table_name
        LEFT JOIN LATERAL (
            SELECT max(k.column_name) AS pk_column, max(p.data_type) AS data_type
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage k
              ON k.constraint_schema = tc.constraint_schema
             AND k.constraint_name = tc.constraint_name
             AND k.table_schema = tc.table_schema
             AND k.table_name = tc.table_name
            JOIN information_schema.columns p
              ON p.table_schema = k.table_schema AND p.table_name = k.table_name
             AND p.column_name = k.column_name
            WHERE tc.table_schema = c.table_schema AND tc.table_name = c.table_name
              AND tc.constraint_type = 'PRIMARY KEY'
            HAVING count(*) = 1
        ) pk ON true
        WHERE c.table_schema = 'public' AND t.table_type = 'BASE TABLE'
          AND lower(c.column_name) = 'brcode'
        ORDER BY c.table_name
    LOOP
        EXECUTE format('SELECT count(*) FROM %I.%I WHERE %I = $1',
                       item.table_schema, item.table_name, item.column_name)
            INTO old_rows USING '34-063-1429';
        EXECUTE format('SELECT count(*) FROM %I.%I WHERE %I = $1',
                       item.table_schema, item.table_name, item.column_name)
            INTO target_rows USING '1429';
        out_of_range := NULL;
        IF item.pk_column IS NOT NULL AND item.data_type IN ('numeric', 'bigint') THEN
            EXECUTE format(
                'SELECT count(*) FROM %I.%I WHERE %I = $1 AND (%I < 14290000001 OR %I > 14299999999)',
                item.table_schema, item.table_name, item.column_name,
                item.pk_column, item.pk_column)
                INTO out_of_range USING '34-063-1429';
        END IF;
        RAISE NOTICE '%.%: old=%, target=%, numeric PK=%, outside 1429 range=%',
            item.table_schema, item.table_name, old_rows, target_rows,
            item.pk_column, out_of_range;
    END LOOP;
END $$;
