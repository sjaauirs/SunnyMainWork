-- =========================================================================================
-- Script Purpose:
-- Inserts a new record into the `notification.tenant_notification_channel` table
-- for a HAP tenant **only if it does not already exist**.
--
-- Jira Ticket: BEN-12
-- =========================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TenantCode>'; -- Example: ten-b4e920d3f6f74496ab533d1a9a8ef9e4
    v_config_json JSONB := '{
      "ChannelConfig": [
        {
          "Enabled": true,
          "Supported": true,
          "ChannelType": "EMAIL",
          "ConsumerDefaultEnabled": false
        },
        {
          "Enabled": true,
          "Supported": true,
          "ChannelType": "SMS",
          "ConsumerDefaultEnabled": false
        }
      ]
    }'::jsonb; -- Single-quoted JSON cast to jsonb
    v_channel_code TEXT;
BEGIN
    -- Check if record already exists
    IF NOT EXISTS (
        SELECT 1
        FROM notification.tenant_notification_channel
        WHERE tenant_code = v_tenant_code
          AND config_json = v_config_json
          AND delete_nbr = 0
    ) THEN
        -- Generate channel code with UUID
        v_channel_code := 'tch-' || gen_random_uuid();

        -- Insert record
        INSERT INTO notification.tenant_notification_channel (
            tenant_notification_channel_code,
            tenant_code,
            config_json,
            create_ts,
            create_user,
            update_user,
            delete_nbr
        )
        VALUES (
            v_channel_code,
            v_tenant_code,
            v_config_json,
            NOW(),       -- create_ts
            'SYSTEM',    -- create_user
            NULL,        -- update_user
            0            -- delete_nbr
        );
    END IF;
END $$;
