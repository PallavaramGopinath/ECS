-- Expand every public table's bounded BrCode column to 11 characters.
-- Existing BrCode values, including 11005, are not modified.
-- Run during a maintenance window: ALTER COLUMN takes table locks.
-- This does not make hyphenated BrCode values compatible with numeric ID generators.

BEGIN;
SET LOCAL lock_timeout = '5s';

DO $$
DECLARE
    item record;
BEGIN
    FOR item IN
        SELECT table_schema, table_name, column_name
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND lower(column_name) = 'brcode'
          AND data_type = 'character varying'
          AND character_maximum_length < 11
        ORDER BY table_name
    LOOP
        EXECUTE format(
            'ALTER TABLE %I.%I ALTER COLUMN %I TYPE varchar(11)',
            item.table_schema, item.table_name, item.column_name
        );
    END LOOP;
END $$;

COMMIT;
