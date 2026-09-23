BEGIN;

CREATE TABLE IF NOT EXISTS hotel.guest_preferences
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    guest_id            bigint NOT NULL,
    preference_type     varchar(30) NOT NULL,
    preference_key      varchar(50) NOT NULL,
    preference_value    varchar(250) NOT NULL,
    notes               text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_guest_preferences_guest FOREIGN KEY (guest_id, organization_id)
        REFERENCES hotel.guests(id, organization_id),
    CONSTRAINT uq_guest_preferences_uid UNIQUE (uid),
    CONSTRAINT ck_guest_preferences_type CHECK
        (preference_type IN ('ROOM', 'DIETARY', 'ACCESSIBILITY', 'COMMUNICATION', 'OTHER'))
);

CREATE INDEX IF NOT EXISTS ix_guest_preferences_guest
    ON hotel.guest_preferences (organization_id, guest_id)
    WHERE is_archived = false;

DROP TRIGGER IF EXISTS trg_guest_preferences_modified_date ON hotel.guest_preferences;
CREATE TRIGGER trg_guest_preferences_modified_date
    BEFORE UPDATE ON hotel.guest_preferences
    FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();

COMMIT;
