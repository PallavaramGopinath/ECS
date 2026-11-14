-- Spec 13a — ECS Recovery Appropriation: Maker / Checker audit columns on mem_demand_status.
-- Additive and idempotent: run on every database (ECS and PCARDB alike) BEFORE deploying the 13a build —
-- the Mem_Demand_Status entity maps these columns, so every query on mem_demand_status fails without them.

ALTER TABLE mem_demand_status ADD COLUMN IF NOT EXISTS appropriated_by numeric;
ALTER TABLE mem_demand_status ADD COLUMN IF NOT EXISTS appropriated_on timestamp without time zone;
ALTER TABLE mem_demand_status ADD COLUMN IF NOT EXISTS rejected_by     numeric;
ALTER TABLE mem_demand_status ADD COLUMN IF NOT EXISTS rejected_on     timestamp without time zone;
ALTER TABLE mem_demand_status ADD COLUMN IF NOT EXISTS reject_reason   varchar(200);
