CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE SCHEMA IF NOT EXISTS identity;

CREATE TABLE IF NOT EXISTS identity.staff_user
(
    uid                 UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username            VARCHAR(100) NOT NULL,
    normalized_username VARCHAR(100) NOT NULL,
    email               VARCHAR(255),
    normalized_email    VARCHAR(255),
    first_name          VARCHAR(100) NOT NULL,
    last_name           VARCHAR(100),
    password_hash       VARCHAR(1000) NOT NULL,

    is_active           BOOLEAN NOT NULL DEFAULT TRUE,
    is_locked           BOOLEAN NOT NULL DEFAULT FALSE,
    failed_login_count  INTEGER NOT NULL DEFAULT 0,
    locked_until_utc    TIMESTAMPTZ NULL,
    last_login_utc      TIMESTAMPTZ NULL,

    created_at_utc      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID NULL,
    updated_at_utc      TIMESTAMPTZ NULL,
    updated_by          UUID NULL,

    CONSTRAINT uq_staff_user_normalized_username
        UNIQUE (normalized_username),

    CONSTRAINT ck_staff_user_failed_login_count
        CHECK (failed_login_count >= 0)
);

CREATE UNIQUE INDEX IF NOT EXISTS
    ux_staff_user_normalized_email
ON identity.staff_user(normalized_email)
WHERE normalized_email IS NOT NULL;


CREATE TABLE IF NOT EXISTS identity.staff_role
(
    uid          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code         VARCHAR(100) NOT NULL,
    name         VARCHAR(150) NOT NULL,
    description  VARCHAR(500),
    is_active    BOOLEAN NOT NULL DEFAULT TRUE,
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_staff_role_code UNIQUE (code)
);


CREATE TABLE IF NOT EXISTS identity.staff_property
(
    uid          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    staff_uid    UUID NOT NULL,
    property_uid UUID NOT NULL,
    is_active    BOOLEAN NOT NULL DEFAULT TRUE,
    assigned_at_utc TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_staff_property_staff
        FOREIGN KEY (staff_uid)
        REFERENCES identity.staff_user(uid),

    CONSTRAINT uq_staff_property
        UNIQUE (staff_uid, property_uid)
);


CREATE TABLE IF NOT EXISTS identity.staff_user_role
(
    uid                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    staff_property_uid UUID NOT NULL,
    role_uid           UUID NOT NULL,
    assigned_at_utc    TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_staff_user_role_staff_property
        FOREIGN KEY (staff_property_uid)
        REFERENCES identity.staff_property(uid)
        ON DELETE CASCADE,

    CONSTRAINT fk_staff_user_role_role
        FOREIGN KEY (role_uid)
        REFERENCES identity.staff_role(uid),

    CONSTRAINT uq_staff_user_role
        UNIQUE (staff_property_uid, role_uid)
);


CREATE TABLE IF NOT EXISTS identity.staff_refresh_token
(
    uid                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    staff_uid          UUID NOT NULL,
    property_uid       UUID NOT NULL,
    token_hash         VARCHAR(500) NOT NULL,
    expires_at_utc     TIMESTAMPTZ NOT NULL,
    revoked_at_utc     TIMESTAMPTZ NULL,
    created_at_utc     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_ip_address VARCHAR(100),

    CONSTRAINT fk_staff_refresh_token_staff
        FOREIGN KEY (staff_uid)
        REFERENCES identity.staff_user(uid)
        ON DELETE CASCADE
);


CREATE INDEX IF NOT EXISTS ix_staff_property_staff_uid
    ON identity.staff_property(staff_uid);

CREATE INDEX IF NOT EXISTS ix_staff_property_property_uid
    ON identity.staff_property(property_uid);

CREATE INDEX IF NOT EXISTS ix_staff_refresh_token_staff_uid
    ON identity.staff_refresh_token(staff_uid);

CREATE INDEX IF NOT EXISTS ix_staff_refresh_token_token_hash
    ON identity.staff_refresh_token(token_hash);


INSERT INTO identity.staff_role(code, name, description)
VALUES
    ('HOTEL_ADMIN', 'Hotel Administrator', 'Full hotel administration access'),
    ('FRONT_DESK', 'Front Desk', 'Reservation, check-in and checkout access'),
    ('MANAGER', 'Hotel Manager', 'Hotel operational management access'),
    ('HOUSEKEEPING', 'Housekeeping', 'Housekeeping task access'),
    ('RESTAURANT', 'Restaurant Staff', 'Restaurant POS access'),
    ('BAR', 'Bar Staff', 'Bar POS access'),
    ('CASHIER', 'Cashier', 'Billing and payment access')
ON CONFLICT (code) DO NOTHING;