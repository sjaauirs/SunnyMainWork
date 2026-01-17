
-- ============================================================================
-- 🚀 Script    : Add Tableau config to Tenant Attribute (KP)
-- 📌 Purpose   : Adds or replaces the "tableau" array at the root of tenant_attr JSONB
-- 🧑 Author    : Pranav Prakash
-- 📅 Date      : 2026-01-12
-- 🧾 Jira      : CPT-9
-- ⚠️ Inputs    : <KP-TENANT-CODE>
-- 📤 Output    : Updated tenant_attr JSONB with new tableau config
-- 📝 Notes     : Assumes tenant_attr is JSONB. If "tableau" exists, it will be overwritten.
-- ============================================================================
DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];

    v_tableau JSONB;
    v_tenant_code TEXT;
BEGIN
    v_tableau := jsonb_build_array(
        jsonb_build_object(
            'src', 'https://prod-useast-b.online.tableau.com/t/sunnyrewards/views/KaiserPermanenteRewardsProgramMonitoring/ExecutiveSummary?:showVizHome=no&:toolbar=no&:tabs=no&:embed=yes',
            'path', 'overview',
            'label', 'Overview',
            'roles', jsonb_build_array('tenant_admin')
        ),
        jsonb_build_object(
            'src', 'https://prod-useast-b.online.tableau.com/t/sunnyrewards/views/KaiserPermanenteRewardsProgramMonitoring/Activation?:showVizHome=no&:toolbar=no&:tabs=no&:embed=yes',
            'path', 'activations',
            'label', 'Activations',
            'roles', jsonb_build_array('tenant_admin')
        ),
        jsonb_build_object(
            'src', 'https://prod-useast-b.online.tableau.com/t/sunnyrewards/views/KaiserPermanenteRewardsProgramMonitoring/ActionCompletion?:showVizHome=no&:toolbar=no&:tabs=no&:embed=yes',
            'path', 'action-completions',
            'label', 'Action Completions',
            'roles', jsonb_build_array('tenant_admin')
        ),
        jsonb_build_object(
            'src', 'https://prod-useast-b.online.tableau.com/t/sunnyrewards/views/KaiserPermanenteRewardsProgramMonitoring/HealthAdventureProgress?:showVizHome=no&:toolbar=no&:tabs=no&:embed=yes',
            'path', 'health-adventures',
            'label', 'Health Adventures',
            'roles', jsonb_build_array('tenant_admin')
        ),
        jsonb_build_object(
            'src', 'https://prod-useast-b.online.tableau.com/t/sunnyrewards/views/KaiserPermanenteRewardsProgramMonitoring/CardsOrderingActivationandUtilization?:showVizHome=no&:toolbar=no&:tabs=no&:embed=yes',
            'path', 'card-status',
            'label', 'Card Status',
            'roles', jsonb_build_array('tenant_admin')
        ),
        jsonb_build_object(
            'src', 'https://prod-useast-b.online.tableau.com/t/sunnyrewards/views/KaiserPermanenteRewardsProgramMonitoring/SpendActivity?:showVizHome=no&:toolbar=no&:tabs=no&:embed=yes',
            'path', 'spend-activity',
            'label', 'Spend Activity',
            'roles', jsonb_build_array('tenant_admin')
        ),
        jsonb_build_object(
            'src', 'https://prod-useast-b.online.tableau.com/t/sunnyrewards/views/KaiserPermanenteRewardsProgramMonitoring/SpendActivityMCC?:showVizHome=no&:toolbar=no&:tabs=no&:embed=yes',
            'path', 'spend-by-mcc',
            'label', 'Spend by MCC',
            'roles', jsonb_build_array('tenant_admin')
        ),
        jsonb_build_object(
            'src', 'https://prod-useast-b.online.tableau.com/t/sunnyrewards/views/KaiserPermanenteRewardsProgramMonitoring/MemberServicesSummaryCS?:showVizHome=no&:toolbar=no&:tabs=no&:embed=yes',
            'path', 'member-services',
            'label', 'Member Services',
            'roles', jsonb_build_array('tenant_admin')
        )
    );

    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{tableau}',
            v_tableau,
            true
        )
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '[INFO] Updated tableau for tenant: %', v_tenant_code;
    END LOOP;
END $$;
