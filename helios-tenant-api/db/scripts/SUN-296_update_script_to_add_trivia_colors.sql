-- Author      : Pernati Rakesh
-- Purpose     : Add 'ux.triviaColors' to tenant_attr JSONB if not already present
-- Jira Task   : SUN-296

DO $$
DECLARE
    rec RECORD;
    trivia_colors JSONB := '{
        "triviaOptionsTextColor": "#0D1C3D",
        "triviaInfoHeadingTextColor": "#0D1C3D",
        "triviaInfoDescriptionTextColor": "#0D1C3D"
    }'::jsonb;
BEGIN
    FOR rec IN
        SELECT tenant_id, tenant_attr
        FROM tenant.tenant
        WHERE delete_nbr = 0
          AND tenant_attr IS NOT NULL
          AND tenant_attr <> '{}'::jsonb
    LOOP
        IF NOT (rec.tenant_attr->'ux' ? 'triviaColors') THEN
            UPDATE tenant.tenant
            SET tenant_attr = jsonb_set(
                                  tenant_attr,
                                  '{ux,triviaColors}',
                                  trivia_colors,
                                  true -- create missing keys
                              ),
                update_ts = NOW(),
                update_user = 'SYSTEM'
            WHERE tenant_id = rec.tenant_id;

            RAISE NOTICE '✅ triviaColors added to tenant_id: %', rec.tenant_id;
        ELSE
            RAISE NOTICE '⚠️ triviaColors already exists for tenant_id: %, skipping.', rec.tenant_id;
        END IF;
    END LOOP;
END $$;
