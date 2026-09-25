-- Allow refresh tokens for every role, including the dummy SuperAdmin
-- (not stored in identity.staff_user). Safe to re-run.

BEGIN;

ALTER TABLE identity.staff_refresh_token
    ALTER COLUMN property_uid DROP NOT NULL;

ALTER TABLE identity.staff_refresh_token
    DROP CONSTRAINT IF EXISTS fk_staff_refresh_token_staff;

ALTER TABLE identity.staff_refresh_token
    ALTER COLUMN token_hash TYPE VARCHAR(128);

CREATE UNIQUE INDEX IF NOT EXISTS ux_staff_refresh_token_hash
    ON identity.staff_refresh_token(token_hash);

COMMIT;
