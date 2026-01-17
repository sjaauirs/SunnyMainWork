-- ============================================================================
-- 🚀 Script    : Update Language Specific Redirect Links & Top-Level Redirects
-- 📌 Purpose   : Deep-merge update of 'languageSpecificContent' JSON under tenant_attr
--                and also update top-level 'kpRedirectLink' & 'kpGaRedirectLink' keys.
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-11-07
-- 🧾 Jira      : SUN-1033
-- ⚠️ Inputs    : Array of KP Tenant Codes
--                lang_specific_content (JSONB block containing language-specific links)
-- 📤 Output    : Updates tenant_attr JSONB column with new redirect URLs
-- 🔗 Script URL: NA
-- 📝 Notes     : 
--    - Safe to execute multiple times (idempotent).
--    - Updates only provided keys (kpRedirectLink, kpGaRedirectLink).
--    - Preserves all existing keys and values in tenant_attr.
-- ============================================================================

DO
$$
DECLARE
    tenant_codes TEXT[] := ARRAY[
        '<KP-TENANT-CODE>',
        '<KP-TENANT-CODE>'
    ];  -- ✅ Replace with actual tenant codes

    lang_specific_content JSONB := '{
        "es": {
            "kpRedirectLink": "https://espanol-hpp.kaiserpermanente.org/es/colorado/secure/inner-door",
            "kpGaRedirectLink": "https://espanol-hpp.kaiserpermanente.org/es/georgia/secure/inner-door"
        },
        "en-US": {
            "kpRedirectLink": "https://hpp.kaiserpermanente.org/colorado/secure/inner-door",
            "kpGaRedirectLink": "https://hpp.kaiserpermanente.org/georgia/secure/inner-door"
        }
    }'::jsonb;

    top_level_links JSONB := '{
        "kpRedirectLink": "https://hpp.kaiserpermanente.org/colorado/secure/inner-door",
        "kpGaRedirectLink": "https://hpp.kaiserpermanente.org/georgia/secure/inner-door"
    }'::jsonb;
updated_attr JSONB;
tenant_rec RECORD;
BEGIN
    FOR tenant_rec IN 
        SELECT tenant_code, tenant_attr
        FROM tenant.tenant
        WHERE tenant_code = ANY(tenant_codes)
          AND delete_nbr = 0
    LOOP
        -- Step 1️: Deep merge the languageSpecificContent.es and en-US
        updated_attr := jsonb_set(
            jsonb_set(
                tenant_rec.tenant_attr,
                '{languageSpecificContent,es}',
                COALESCE(tenant_rec.tenant_attr -> 'languageSpecificContent' -> 'es', '{}'::jsonb)
                || (lang_specific_content -> 'es'),
                true
            ),
            '{languageSpecificContent,en-US}',
            COALESCE(tenant_rec.tenant_attr -> 'languageSpecificContent' -> 'en-US', '{}'::jsonb)
            || (lang_specific_content -> 'en-US'),
            true
        );

        -- Step 2️: Conditionally update top-level kpRedirectLink
        IF tenant_rec.tenant_attr ? 'kpRedirectLink' THEN
            updated_attr := jsonb_set(
                updated_attr,
                '{kpRedirectLink}',
                to_jsonb(top_level_links ->> 'kpRedirectLink'),
                true
            );
        END IF;

        -- Step 3️: Conditionally update top-level kpGaRedirectLink
        IF tenant_rec.tenant_attr ? 'kpGaRedirectLink' THEN
            updated_attr := jsonb_set(
                updated_attr,
                '{kpGaRedirectLink}',
                to_jsonb(top_level_links ->> 'kpGaRedirectLink'),
                true
            );
        END IF;

        -- Step 4️: Apply final update
        UPDATE tenant.tenant
        SET tenant_attr = updated_attr
        WHERE tenant_code = tenant_rec.tenant_code;
    END LOOP;

    RAISE NOTICE '✅ Updated languageSpecificContent and top-level links for tenants: %', tenant_codes;
END
$$;
