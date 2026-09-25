-- Fossyl Labs - Multi -Tenant Hotel / Accommodation Management Schema
-- Target database: PostgreSQL 17+
-- Knuckles can be created as the first organization/property without changing this schema.

BEGIN;

CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS btree_gist;

CREATE SCHEMA IF NOT EXISTS hotel;

SET search_path TO hotel, public;

-- =============================================================
-- Common modified-date trigger
-- The API should set modified_by from the authenticated user's subject.
-- =============================================================

CREATE OR REPLACE FUNCTION hotel.set_modified_date()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.modified_date := CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$;

-- =============================================================
-- PLATFORM, ORGANIZATION AND PROPERTY
-- =============================================================

CREATE TABLE hotel.organizations
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    code                varchar(30) NOT NULL,
    name                varchar(200) NOT NULL,
    legal_name          varchar(250),
    default_currency    char(3) NOT NULL DEFAULT 'LKR',
    timezone            varchar(100) NOT NULL DEFAULT 'Asia/Colombo',
    status              varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT uq_organizations_uid UNIQUE (uid),
    CONSTRAINT uq_organizations_code UNIQUE (code),
    CONSTRAINT ck_organizations_status CHECK (status IN ('TRIAL', 'ACTIVE', 'SUSPENDED', 'CLOSED'))
);

CREATE TABLE hotel.properties
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    code                varchar(30) NOT NULL,
    name                varchar(200) NOT NULL,
    slug                varchar(200) NOT NULL,
    property_type       varchar(30) NOT NULL,
    description         text,
    address_line1       varchar(250),
    address_line2       varchar(250),
    city                varchar(100),
    district            varchar(100),
    province            varchar(100),
    postal_code         varchar(20),
    country_code        char(2) NOT NULL DEFAULT 'LK',
    latitude            numeric(10,7),
    longitude           numeric(10,7),
    phone               varchar(30),
    email               varchar(254),
    timezone            varchar(100) NOT NULL DEFAULT 'Asia/Colombo',
    default_currency    char(3) NOT NULL DEFAULT 'LKR',
    status              varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_properties_organization FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id),
    CONSTRAINT uq_properties_uid UNIQUE (uid),
    CONSTRAINT uq_properties_org_id UNIQUE (id, organization_id),
    CONSTRAINT uq_properties_org_code UNIQUE (organization_id, code),
    CONSTRAINT uq_properties_slug UNIQUE (slug),
    CONSTRAINT ck_properties_type CHECK (property_type IN
        ('HOTEL', 'GUESTHOUSE', 'VILLA', 'APARTMENT', 'HOSTEL', 'HOMESTAY',
         'RESORT', 'CAMPSITE', 'LODGE', 'BUNGALOW', 'OTHER')),
    CONSTRAINT ck_properties_status CHECK (status IN ('DRAFT', 'ACTIVE', 'SUSPENDED', 'CLOSED')),
    CONSTRAINT ck_properties_latitude CHECK (latitude IS NULL OR latitude BETWEEN -90 AND 90),
    CONSTRAINT ck_properties_longitude CHECK (longitude IS NULL OR longitude BETWEEN -180 AND 180)
);

CREATE TABLE hotel.property_settings
(
    id                      bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                     uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id         bigint NOT NULL,
    property_id             bigint NOT NULL,
    check_in_time           time NOT NULL DEFAULT '14:00',
    check_out_time          time NOT NULL DEFAULT '11:00',
    booking_number_prefix   varchar(20) NOT NULL DEFAULT 'BKG',
    invoice_number_prefix   varchar(20) NOT NULL DEFAULT 'INV',
    tax_rate                numeric(7,4) NOT NULL DEFAULT 0,
    service_charge_rate     numeric(7,4) NOT NULL DEFAULT 0,
    allow_overbooking       boolean NOT NULL DEFAULT false,
    extra_settings          jsonb NOT NULL DEFAULT '{}'::jsonb,
    is_active               boolean NOT NULL DEFAULT true,
    is_archived             boolean NOT NULL DEFAULT false,
    creation_date           timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              varchar(100),
    modified_date           timestamptz,
    modified_by             varchar(100),
    CONSTRAINT fk_property_settings_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT uq_property_settings_uid UNIQUE (uid),
    CONSTRAINT uq_property_settings_property UNIQUE (property_id),
    CONSTRAINT ck_property_settings_tax CHECK (tax_rate BETWEEN 0 AND 100),
    CONSTRAINT ck_property_settings_service_charge CHECK (service_charge_rate BETWEEN 0 AND 100)
);

CREATE TABLE hotel.app_users
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    auth_subject        varchar(100) NOT NULL,
    user_name           varchar(100),
    first_name          varchar(100),
    last_name           varchar(100),
    email               varchar(254),
    phone               varchar(30),
    status              varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT uq_app_users_uid UNIQUE (uid),
    CONSTRAINT uq_app_users_auth_subject UNIQUE (auth_subject),
    CONSTRAINT ck_app_users_status CHECK (status IN ('INVITED', 'ACTIVE', 'LOCKED', 'DISABLED'))
);

CREATE TABLE hotel.user_property_access
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    user_id             bigint NOT NULL,
    organization_id     bigint NOT NULL,
    property_id         bigint,
    role_code           varchar(50) NOT NULL,
    is_default_property boolean NOT NULL DEFAULT false,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_user_property_access_user FOREIGN KEY (user_id) REFERENCES hotel.app_users(id),
    CONSTRAINT fk_user_property_access_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id),
    CONSTRAINT fk_user_property_access_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT uq_user_property_access_uid UNIQUE (uid),
    CONSTRAINT uq_user_property_access_assignment UNIQUE NULLS NOT DISTINCT
        (user_id, organization_id, property_id, role_code)
);

-- =============================================================
-- ACCOMMODATION, MEAL PLANS AND RATES
-- =============================================================

CREATE TABLE hotel.accommodation_types
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    code                varchar(30) NOT NULL,
    name                varchar(150) NOT NULL,
    unit_kind           varchar(30) NOT NULL DEFAULT 'ROOM',
    description         text,
    max_adults          integer NOT NULL DEFAULT 1,
    max_children        integer NOT NULL DEFAULT 0,
    max_occupancy       integer NOT NULL DEFAULT 1,
    default_quantity    integer NOT NULL DEFAULT 1,
    base_rate           numeric(18,2) NOT NULL DEFAULT 0,
    sort_order          integer NOT NULL DEFAULT 0,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_accommodation_types_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT uq_accommodation_types_uid UNIQUE (uid),
    CONSTRAINT uq_accommodation_types_org_id UNIQUE (id, organization_id),
    CONSTRAINT uq_accommodation_types_property_code UNIQUE (property_id, code),
    CONSTRAINT ck_accommodation_types_kind CHECK (unit_kind IN
        ('ROOM', 'VILLA', 'APARTMENT', 'CABIN', 'TENT', 'DORM_BED', 'COTTAGE', 'ENTIRE_PROPERTY', 'OTHER')),
    CONSTRAINT ck_accommodation_types_capacity CHECK
        (max_adults >= 0 AND max_children >= 0 AND max_occupancy > 0 AND max_occupancy >= max_adults),
    CONSTRAINT ck_accommodation_types_rate CHECK (base_rate >= 0)
);

CREATE TABLE hotel.accommodation_units
(
    id                      bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                     uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id         bigint NOT NULL,
    property_id             bigint NOT NULL,
    accommodation_type_id   bigint NOT NULL,
    unit_code               varchar(50) NOT NULL,
    unit_name               varchar(150),
    floor_or_area           varchar(100),
    status                  varchar(30) NOT NULL DEFAULT 'AVAILABLE',
    housekeeping_status     varchar(30) NOT NULL DEFAULT 'CLEAN',
    notes                   text,
    is_active               boolean NOT NULL DEFAULT true,
    is_archived             boolean NOT NULL DEFAULT false,
    creation_date           timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              varchar(100),
    modified_date           timestamptz,
    modified_by             varchar(100),
    CONSTRAINT fk_accommodation_units_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_accommodation_units_type FOREIGN KEY (accommodation_type_id, organization_id)
        REFERENCES hotel.accommodation_types(id, organization_id),
    CONSTRAINT uq_accommodation_units_uid UNIQUE (uid),
    CONSTRAINT uq_accommodation_units_org_id UNIQUE (id, organization_id),
    CONSTRAINT uq_accommodation_units_property_code UNIQUE (property_id, unit_code),
    CONSTRAINT ck_accommodation_units_status CHECK
        (status IN ('AVAILABLE', 'OCCUPIED', 'OUT_OF_SERVICE', 'MAINTENANCE', 'INACTIVE')),
    CONSTRAINT ck_accommodation_units_housekeeping CHECK
        (housekeeping_status IN ('CLEAN', 'DIRTY', 'INSPECTED', 'IN_PROGRESS', 'NOT_APPLICABLE'))
);

CREATE TABLE hotel.meal_plans
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    code                varchar(30) NOT NULL,
    name                varchar(100) NOT NULL,
    description         text,
    includes_breakfast  boolean NOT NULL DEFAULT false,
    includes_lunch      boolean NOT NULL DEFAULT false,
    includes_dinner     boolean NOT NULL DEFAULT false,
    allow_byo           boolean NOT NULL DEFAULT false,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_meal_plans_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT uq_meal_plans_uid UNIQUE (uid),
    CONSTRAINT uq_meal_plans_org_id UNIQUE (id, organization_id),
    CONSTRAINT uq_meal_plans_property_code UNIQUE (property_id, code)
);

CREATE TABLE hotel.rate_plans
(
    id                      bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                     uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id         bigint NOT NULL,
    property_id             bigint NOT NULL,
    accommodation_type_id   bigint NOT NULL,
    meal_plan_id            bigint,
    code                    varchar(30) NOT NULL,
    name                    varchar(150) NOT NULL,
    pricing_basis           varchar(30) NOT NULL,
    currency                char(3) NOT NULL DEFAULT 'LKR',
    description             text,
    is_refundable           boolean NOT NULL DEFAULT true,
    is_active               boolean NOT NULL DEFAULT true,
    is_archived             boolean NOT NULL DEFAULT false,
    creation_date           timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              varchar(100),
    modified_date           timestamptz,
    modified_by             varchar(100),
    CONSTRAINT fk_rate_plans_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_rate_plans_type FOREIGN KEY (accommodation_type_id, organization_id)
        REFERENCES hotel.accommodation_types(id, organization_id),
    CONSTRAINT fk_rate_plans_meal FOREIGN KEY (meal_plan_id, organization_id)
        REFERENCES hotel.meal_plans(id, organization_id),
    CONSTRAINT uq_rate_plans_uid UNIQUE (uid),
    CONSTRAINT uq_rate_plans_org_id UNIQUE (id, organization_id),
    CONSTRAINT uq_rate_plans_property_code UNIQUE (property_id, code),
    CONSTRAINT ck_rate_plans_pricing_basis CHECK
        (pricing_basis IN ('PER_ROOM_PER_NIGHT', 'PER_PERSON_PER_NIGHT',
                           'PER_BED_PER_NIGHT', 'FLAT_PER_STAY'))
);

CREATE TABLE hotel.rate_plan_prices
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    rate_plan_id        bigint NOT NULL,
    start_date          date NOT NULL,
    end_date            date NOT NULL,
    day_of_week         smallint,
    adult_rate          numeric(18,2),
    child_rate          numeric(18,2),
    unit_rate           numeric(18,2) NOT NULL,
    minimum_stay        integer NOT NULL DEFAULT 1,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_rate_plan_prices_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_rate_plan_prices_plan FOREIGN KEY (rate_plan_id, organization_id)
        REFERENCES hotel.rate_plans(id, organization_id),
    CONSTRAINT uq_rate_plan_prices_uid UNIQUE (uid),
    CONSTRAINT ck_rate_plan_prices_dates CHECK (end_date >= start_date),
    CONSTRAINT ck_rate_plan_prices_day CHECK (day_of_week IS NULL OR day_of_week BETWEEN 0 AND 6),
    CONSTRAINT ck_rate_plan_prices_amounts CHECK
        (unit_rate >= 0 AND COALESCE(adult_rate, 0) >= 0 AND COALESCE(child_rate, 0) >= 0),
    CONSTRAINT ck_rate_plan_prices_minimum_stay CHECK (minimum_stay > 0)
);

CREATE TABLE hotel.unit_blocks
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    unit_id             bigint NOT NULL,
    start_date          date NOT NULL,
    end_date            date NOT NULL,
    block_type          varchar(30) NOT NULL,
    reason              text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_unit_blocks_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_unit_blocks_unit FOREIGN KEY (unit_id, organization_id)
        REFERENCES hotel.accommodation_units(id, organization_id),
    CONSTRAINT uq_unit_blocks_uid UNIQUE (uid),
    CONSTRAINT ck_unit_blocks_dates CHECK (end_date > start_date),
    CONSTRAINT ck_unit_blocks_type CHECK (block_type IN ('MAINTENANCE', 'OWNER_USE', 'CLOSED', 'OTHER'))
);

-- =============================================================
-- GUESTS
-- =============================================================

CREATE TABLE hotel.guests
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    guest_type          varchar(30) NOT NULL DEFAULT 'INDIVIDUAL',
    title               varchar(20),
    first_name          varchar(100),
    last_name           varchar(100),
    display_name        varchar(200) NOT NULL,
    phone               varchar(30),
    alternate_phone     varchar(30),
    email               varchar(254),
    nationality_code    char(2),
    date_of_birth       date,
    preferred_language  varchar(10),
    address             text,
    city                varchar(100),
    country_code        char(2),
    notes               text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_guests_organization FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id),
    CONSTRAINT uq_guests_uid UNIQUE (uid),
    CONSTRAINT uq_guests_org_id UNIQUE (id, organization_id),
    CONSTRAINT ck_guests_type CHECK
        (guest_type IN ('INDIVIDUAL', 'COUPLE', 'FAMILY', 'GROUP', 'CORPORATE', 'TRAVEL_AGENT'))
);

CREATE TABLE hotel.guest_documents
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    guest_id            bigint NOT NULL,
    document_type       varchar(30) NOT NULL,
    document_number     varchar(100) NOT NULL,
    issuing_country     char(2),
    issued_date         date,
    expiry_date         date,
    file_url            text,
    is_verified         boolean NOT NULL DEFAULT false,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_guest_documents_guest FOREIGN KEY (guest_id, organization_id)
        REFERENCES hotel.guests(id, organization_id),
    CONSTRAINT uq_guest_documents_uid UNIQUE (uid),
    CONSTRAINT uq_guest_documents_number UNIQUE (organization_id, document_type, document_number),
    CONSTRAINT ck_guest_documents_type CHECK (document_type IN ('NIC', 'PASSPORT', 'DRIVING_LICENCE', 'OTHER')),
    CONSTRAINT ck_guest_documents_expiry CHECK
        (expiry_date IS NULL OR issued_date IS NULL OR expiry_date >= issued_date)
);

-- =============================================================
-- BOOKINGS, ROOM ALLOCATION, CHARGES AND PAYMENTS
-- =============================================================

CREATE TABLE hotel.bookings
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    booking_number      varchar(50) NOT NULL,
    lead_guest_id       bigint,
    booking_source      varchar(30) NOT NULL DEFAULT 'DIRECT',
    external_reference  varchar(100),
    status              varchar(30) NOT NULL DEFAULT 'PENDING',
    check_in_date       date NOT NULL,
    check_out_date      date NOT NULL,
    number_of_nights    integer GENERATED ALWAYS AS (check_out_date - check_in_date) STORED,
    adults              integer NOT NULL DEFAULT 1,
    children            integer NOT NULL DEFAULT 0,
    infants             integer NOT NULL DEFAULT 0,
    currency            char(3) NOT NULL DEFAULT 'LKR',
    discount_amount     numeric(18,2) NOT NULL DEFAULT 0,
    tax_amount          numeric(18,2) NOT NULL DEFAULT 0,
    service_charge      numeric(18,2) NOT NULL DEFAULT 0,
    quoted_total        numeric(18,2),
    arrival_time        time,
    departure_time      time,
    special_requests    text,
    internal_notes      text,
    confirmed_at        timestamptz,
    checked_in_at       timestamptz,
    checked_out_at      timestamptz,
    cancelled_at        timestamptz,
    cancellation_reason text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_bookings_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_bookings_lead_guest FOREIGN KEY (lead_guest_id, organization_id)
        REFERENCES hotel.guests(id, organization_id),
    CONSTRAINT uq_bookings_uid UNIQUE (uid),
    CONSTRAINT uq_bookings_org_id UNIQUE (id, organization_id),
    CONSTRAINT uq_bookings_property_number UNIQUE (property_id, booking_number),
    CONSTRAINT ck_bookings_dates CHECK (check_out_date > check_in_date),
    CONSTRAINT ck_bookings_guests CHECK (adults > 0 AND children >= 0 AND infants >= 0),
    CONSTRAINT ck_bookings_amounts CHECK
        (discount_amount >= 0 AND tax_amount >= 0 AND service_charge >= 0 AND COALESCE(quoted_total, 0) >= 0),
    CONSTRAINT ck_bookings_status CHECK
        (status IN ('INQUIRY', 'PENDING', 'TENTATIVE', 'CONFIRMED', 'CHECKED_IN',
                    'CHECKED_OUT', 'COMPLETED', 'CANCELLED', 'NO_SHOW')),
    CONSTRAINT ck_bookings_source CHECK
        (booking_source IN ('WALK_IN', 'PHONE', 'WHATSAPP', 'DIRECT', 'WEBSITE',
                            'TRAVEL_AGENT', 'BOOKING_COM', 'AIRBNB', 'OTHER'))
);

CREATE TABLE hotel.booking_units
(
    id                      bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                     uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id         bigint NOT NULL,
    property_id             bigint NOT NULL,
    booking_id              bigint NOT NULL,
    accommodation_type_id   bigint NOT NULL,
    unit_id                 bigint,
    rate_plan_id            bigint,
    meal_plan_id            bigint,
    check_in_date           date NOT NULL,
    check_out_date          date NOT NULL,
    number_of_nights        integer GENERATED ALWAYS AS (check_out_date - check_in_date) STORED,
    adults                  integer NOT NULL DEFAULT 1,
    children                integer NOT NULL DEFAULT 0,
    unit_quantity           integer NOT NULL DEFAULT 1,
    guest_count             integer NOT NULL DEFAULT 1,
    pricing_basis           varchar(30) NOT NULL,
    unit_rate               numeric(18,2) NOT NULL,
    discount_amount         numeric(18,2) NOT NULL DEFAULT 0,
    tax_amount              numeric(18,2) NOT NULL DEFAULT 0,
    total_amount            numeric(18,2) NOT NULL,
    allocation_status       varchar(30) NOT NULL DEFAULT 'HELD',
    notes                   text,
    is_active               boolean NOT NULL DEFAULT true,
    is_archived             boolean NOT NULL DEFAULT false,
    creation_date           timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              varchar(100),
    modified_date           timestamptz,
    modified_by             varchar(100),
    CONSTRAINT fk_booking_units_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_booking_units_booking FOREIGN KEY (booking_id, organization_id)
        REFERENCES hotel.bookings(id, organization_id),
    CONSTRAINT fk_booking_units_type FOREIGN KEY (accommodation_type_id, organization_id)
        REFERENCES hotel.accommodation_types(id, organization_id),
    CONSTRAINT fk_booking_units_unit FOREIGN KEY (unit_id, organization_id)
        REFERENCES hotel.accommodation_units(id, organization_id),
    CONSTRAINT fk_booking_units_rate FOREIGN KEY (rate_plan_id, organization_id)
        REFERENCES hotel.rate_plans(id, organization_id),
    CONSTRAINT fk_booking_units_meal FOREIGN KEY (meal_plan_id, organization_id)
        REFERENCES hotel.meal_plans(id, organization_id),
    CONSTRAINT uq_booking_units_uid UNIQUE (uid),
    CONSTRAINT uq_booking_units_org_id UNIQUE (id, organization_id),
    CONSTRAINT ck_booking_units_dates CHECK (check_out_date > check_in_date),
    CONSTRAINT ck_booking_units_counts CHECK
        (adults >= 0 AND children >= 0 AND unit_quantity > 0 AND guest_count > 0),
    CONSTRAINT ck_booking_units_amounts CHECK
        (unit_rate >= 0 AND discount_amount >= 0 AND tax_amount >= 0 AND total_amount >= 0),
    CONSTRAINT ck_booking_units_pricing_basis CHECK
        (pricing_basis IN ('PER_ROOM_PER_NIGHT', 'PER_PERSON_PER_NIGHT',
                           'PER_BED_PER_NIGHT', 'FLAT_PER_STAY')),
    CONSTRAINT ck_booking_units_allocation_status CHECK
        (allocation_status IN ('HELD', 'CONFIRMED', 'CHECKED_IN', 'RELEASED', 'CANCELLED')),
    CONSTRAINT ex_booking_units_no_overlap EXCLUDE USING gist
        (unit_id WITH =, daterange(check_in_date, check_out_date, '[)') WITH &&)
        WHERE (unit_id IS NOT NULL AND allocation_status IN ('HELD', 'CONFIRMED', 'CHECKED_IN') AND is_archived = false)
);

CREATE TABLE hotel.booking_guests
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    booking_id          bigint NOT NULL,
    booking_unit_id     bigint,
    guest_id            bigint NOT NULL,
    is_lead_guest       boolean NOT NULL DEFAULT false,
    checked_in_at       timestamptz,
    checked_out_at      timestamptz,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_booking_guests_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_booking_guests_booking FOREIGN KEY (booking_id, organization_id)
        REFERENCES hotel.bookings(id, organization_id),
    CONSTRAINT fk_booking_guests_booking_unit FOREIGN KEY (booking_unit_id, organization_id)
        REFERENCES hotel.booking_units(id, organization_id),
    CONSTRAINT fk_booking_guests_guest FOREIGN KEY (guest_id, organization_id)
        REFERENCES hotel.guests(id, organization_id),
    CONSTRAINT uq_booking_guests_uid UNIQUE (uid),
    CONSTRAINT uq_booking_guests_booking_guest UNIQUE (booking_id, guest_id)
);

CREATE TABLE hotel.booking_charge_types
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    code                varchar(30) NOT NULL,
    name                varchar(100) NOT NULL,
    category            varchar(30) NOT NULL,
    is_taxable          boolean NOT NULL DEFAULT false,
    default_price       numeric(18,2),
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_booking_charge_types_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT uq_booking_charge_types_uid UNIQUE (uid),
    CONSTRAINT uq_booking_charge_types_org_id UNIQUE (id, organization_id),
    CONSTRAINT uq_booking_charge_types_property_code UNIQUE (property_id, code),
    CONSTRAINT ck_booking_charge_types_category CHECK
        (category IN ('FOOD', 'COOKING', 'LAUNDRY', 'BEVERAGE', 'TRANSPORT',
                      'ACTIVITY', 'DAMAGE', 'OTHER')),
    CONSTRAINT ck_booking_charge_types_price CHECK (default_price IS NULL OR default_price >= 0)
);

CREATE TABLE hotel.booking_charges
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    booking_id          bigint NOT NULL,
    booking_unit_id     bigint,
    charge_type_id      bigint NOT NULL,
    service_date        date NOT NULL DEFAULT CURRENT_DATE,
    description         varchar(250) NOT NULL,
    quantity            numeric(12,3) NOT NULL DEFAULT 1,
    unit_price          numeric(18,2) NOT NULL,
    discount_amount     numeric(18,2) NOT NULL DEFAULT 0,
    tax_amount          numeric(18,2) NOT NULL DEFAULT 0,
    total_amount        numeric(18,2) GENERATED ALWAYS AS
        (round((quantity * unit_price) - discount_amount + tax_amount, 2)) STORED,
    notes               text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_booking_charges_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_booking_charges_booking FOREIGN KEY (booking_id, organization_id)
        REFERENCES hotel.bookings(id, organization_id),
    CONSTRAINT fk_booking_charges_booking_unit FOREIGN KEY (booking_unit_id, organization_id)
        REFERENCES hotel.booking_units(id, organization_id),
    CONSTRAINT fk_booking_charges_type FOREIGN KEY (charge_type_id, organization_id)
        REFERENCES hotel.booking_charge_types(id, organization_id),
    CONSTRAINT uq_booking_charges_uid UNIQUE (uid),
    CONSTRAINT ck_booking_charges_amounts CHECK
        (quantity > 0 AND unit_price >= 0 AND discount_amount >= 0 AND tax_amount >= 0)
);

CREATE TABLE hotel.booking_payments
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    booking_id          bigint NOT NULL,
    payment_method      varchar(30) NOT NULL,
    payment_type        varchar(30) NOT NULL DEFAULT 'PAYMENT',
    amount              numeric(18,2) NOT NULL,
    currency            char(3) NOT NULL DEFAULT 'LKR',
    status              varchar(30) NOT NULL DEFAULT 'COMPLETED',
    reference_number    varchar(150),
    paid_at             timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    notes               text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_booking_payments_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_booking_payments_booking FOREIGN KEY (booking_id, organization_id)
        REFERENCES hotel.bookings(id, organization_id),
    CONSTRAINT uq_booking_payments_uid UNIQUE (uid),
    CONSTRAINT uq_booking_payments_org_id UNIQUE (id, organization_id),
    CONSTRAINT ck_booking_payments_amount CHECK (amount > 0),
    CONSTRAINT ck_booking_payments_method CHECK
        (payment_method IN ('CASH', 'BANK_TRANSFER', 'CARD', 'ONLINE_GATEWAY', 'CHEQUE', 'OTHER')),
    CONSTRAINT ck_booking_payments_type CHECK (payment_type IN ('DEPOSIT', 'PAYMENT', 'ADJUSTMENT')),
    CONSTRAINT ck_booking_payments_status CHECK
        (status IN ('PENDING', 'COMPLETED', 'FAILED', 'CANCELLED'))
);

CREATE TABLE hotel.booking_refunds
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    booking_id          bigint NOT NULL,
    payment_id          bigint NOT NULL,
    amount              numeric(18,2) NOT NULL,
    status              varchar(30) NOT NULL DEFAULT 'COMPLETED',
    reason              text NOT NULL,
    refunded_at         timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_booking_refunds_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_booking_refunds_booking FOREIGN KEY (booking_id, organization_id)
        REFERENCES hotel.bookings(id, organization_id),
    CONSTRAINT fk_booking_refunds_payment FOREIGN KEY (payment_id, organization_id)
        REFERENCES hotel.booking_payments(id, organization_id),
    CONSTRAINT uq_booking_refunds_uid UNIQUE (uid),
    CONSTRAINT ck_booking_refunds_amount CHECK (amount > 0),
    CONSTRAINT ck_booking_refunds_status CHECK (status IN ('PENDING', 'COMPLETED', 'FAILED', 'CANCELLED'))
);

CREATE TABLE hotel.booking_status_history
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    booking_id          bigint NOT NULL,
    old_status          varchar(30),
    new_status          varchar(30) NOT NULL,
    reason              text,
    changed_at          timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    changed_by          varchar(100),
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_booking_status_history_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_booking_status_history_booking FOREIGN KEY (booking_id, organization_id)
        REFERENCES hotel.bookings(id, organization_id),
    CONSTRAINT uq_booking_status_history_uid UNIQUE (uid)
);

-- =============================================================
-- EXPENSES, UTILITIES AND OTHER INCOME
-- =============================================================

CREATE TABLE hotel.expense_categories
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    parent_id           bigint,
    code                varchar(30) NOT NULL,
    name                varchar(100) NOT NULL,
    expense_group       varchar(30) NOT NULL,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_expense_categories_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id),
    CONSTRAINT fk_expense_categories_parent FOREIGN KEY (parent_id) REFERENCES hotel.expense_categories(id),
    CONSTRAINT uq_expense_categories_uid UNIQUE (uid),
    CONSTRAINT uq_expense_categories_org_id UNIQUE (id, organization_id),
    CONSTRAINT uq_expense_categories_org_code UNIQUE (organization_id, code),
    CONSTRAINT ck_expense_categories_group CHECK
        (expense_group IN ('STAFF', 'UTILITY', 'FOOD', 'MAINTENANCE', 'TRANSPORT',
                           'MARKETING', 'SUPPLIES', 'ADMINISTRATION', 'OTHER'))
);

CREATE TABLE hotel.suppliers
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    name                varchar(200) NOT NULL,
    phone               varchar(30),
    email               varchar(254),
    address             text,
    tax_number          varchar(100),
    notes               text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_suppliers_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id),
    CONSTRAINT uq_suppliers_uid UNIQUE (uid),
    CONSTRAINT uq_suppliers_org_id UNIQUE (id, organization_id)
);

CREATE TABLE hotel.expenses
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    expense_category_id bigint NOT NULL,
    supplier_id         bigint,
    booking_id          bigint,
    expense_date        date NOT NULL,
    description         varchar(300) NOT NULL,
    amount              numeric(18,2) NOT NULL,
    currency            char(3) NOT NULL DEFAULT 'LKR',
    payment_method      varchar(30),
    reference_number    varchar(150),
    receipt_url         text,
    notes               text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_expenses_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_expenses_category FOREIGN KEY (expense_category_id, organization_id)
        REFERENCES hotel.expense_categories(id, organization_id),
    CONSTRAINT fk_expenses_supplier FOREIGN KEY (supplier_id, organization_id)
        REFERENCES hotel.suppliers(id, organization_id),
    CONSTRAINT fk_expenses_booking FOREIGN KEY (booking_id, organization_id)
        REFERENCES hotel.bookings(id, organization_id),
    CONSTRAINT uq_expenses_uid UNIQUE (uid),
    CONSTRAINT uq_expenses_org_id UNIQUE (id, organization_id),
    CONSTRAINT ck_expenses_amount CHECK (amount > 0),
    CONSTRAINT ck_expenses_payment_method CHECK
        (payment_method IS NULL OR payment_method IN
            ('CASH', 'BANK_TRANSFER', 'CARD', 'CHEQUE', 'OTHER'))
);

CREATE TABLE hotel.utility_types
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    code                varchar(30) NOT NULL,
    name                varchar(100) NOT NULL,
    unit_of_measure     varchar(30),
    is_metered          boolean NOT NULL DEFAULT true,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_utility_types_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id),
    CONSTRAINT uq_utility_types_uid UNIQUE (uid),
    CONSTRAINT uq_utility_types_org_id UNIQUE (id, organization_id),
    CONSTRAINT uq_utility_types_org_code UNIQUE (organization_id, code)
);

CREATE TABLE hotel.utility_bills
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    utility_type_id     bigint NOT NULL,
    period_start        date NOT NULL,
    period_end          date NOT NULL,
    previous_reading    numeric(18,3),
    current_reading     numeric(18,3),
    units_used          numeric(18,3),
    amount              numeric(18,2) NOT NULL,
    currency            char(3) NOT NULL DEFAULT 'LKR',
    due_date            date,
    paid_at             timestamptz,
    reference_number    varchar(150),
    notes               text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_utility_bills_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_utility_bills_type FOREIGN KEY (utility_type_id, organization_id)
        REFERENCES hotel.utility_types(id, organization_id),
    CONSTRAINT uq_utility_bills_uid UNIQUE (uid),
    CONSTRAINT ck_utility_bills_period CHECK (period_end >= period_start),
    CONSTRAINT ck_utility_bills_amount CHECK (amount >= 0),
    CONSTRAINT ck_utility_bills_readings CHECK
        (previous_reading IS NULL OR current_reading IS NULL OR current_reading >= previous_reading)
);

CREATE TABLE hotel.income_categories
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    code                varchar(30) NOT NULL,
    name                varchar(100) NOT NULL,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_income_categories_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id),
    CONSTRAINT uq_income_categories_uid UNIQUE (uid),
    CONSTRAINT uq_income_categories_org_id UNIQUE (id, organization_id),
    CONSTRAINT uq_income_categories_org_code UNIQUE (organization_id, code)
);

CREATE TABLE hotel.other_income
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    income_category_id  bigint NOT NULL,
    booking_id          bigint,
    income_date         date NOT NULL,
    description         varchar(300) NOT NULL,
    amount              numeric(18,2) NOT NULL,
    currency            char(3) NOT NULL DEFAULT 'LKR',
    payment_method      varchar(30),
    reference_number    varchar(150),
    notes               text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_other_income_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_other_income_category FOREIGN KEY (income_category_id, organization_id)
        REFERENCES hotel.income_categories(id, organization_id),
    CONSTRAINT fk_other_income_booking FOREIGN KEY (booking_id, organization_id)
        REFERENCES hotel.bookings(id, organization_id),
    CONSTRAINT uq_other_income_uid UNIQUE (uid),
    CONSTRAINT ck_other_income_amount CHECK (amount > 0)
);

-- =============================================================
-- STAFF AND PAYMENTS
-- =============================================================

CREATE TABLE hotel.staff_roles
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    code                varchar(30) NOT NULL,
    name                varchar(100) NOT NULL,
    description         text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_staff_roles_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id),
    CONSTRAINT uq_staff_roles_uid UNIQUE (uid),
    CONSTRAINT uq_staff_roles_org_id UNIQUE (id, organization_id),
    CONSTRAINT uq_staff_roles_org_code UNIQUE (organization_id, code)
);

CREATE TABLE hotel.staff_members
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    staff_role_id       bigint NOT NULL,
    employee_number     varchar(50) NOT NULL,
    first_name          varchar(100) NOT NULL,
    last_name           varchar(100),
    phone               varchar(30),
    email               varchar(254),
    employment_type     varchar(30) NOT NULL,
    basic_salary        numeric(18,2),
    daily_rate          numeric(18,2),
    hourly_rate         numeric(18,2),
    joined_date         date,
    left_date           date,
    status              varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_staff_members_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_staff_members_role FOREIGN KEY (staff_role_id, organization_id)
        REFERENCES hotel.staff_roles(id, organization_id),
    CONSTRAINT uq_staff_members_uid UNIQUE (uid),
    CONSTRAINT uq_staff_members_org_id UNIQUE (id, organization_id),
    CONSTRAINT uq_staff_members_property_number UNIQUE (property_id, employee_number),
    CONSTRAINT ck_staff_members_employment CHECK
        (employment_type IN ('MONTHLY', 'DAILY', 'HOURLY', 'CONTRACT')),
    CONSTRAINT ck_staff_members_status CHECK (status IN ('ACTIVE', 'ON_LEAVE', 'LEFT', 'SUSPENDED')),
    CONSTRAINT ck_staff_members_rates CHECK
        (COALESCE(basic_salary, 0) >= 0 AND COALESCE(daily_rate, 0) >= 0 AND COALESCE(hourly_rate, 0) >= 0),
    CONSTRAINT ck_staff_members_dates CHECK (left_date IS NULL OR joined_date IS NULL OR left_date >= joined_date)
);

CREATE TABLE hotel.staff_work_logs
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    staff_id            bigint NOT NULL,
    booking_id          bigint,
    work_date           date NOT NULL,
    start_time          time,
    end_time            time,
    hours_worked        numeric(8,2) NOT NULL DEFAULT 0,
    overtime_hours      numeric(8,2) NOT NULL DEFAULT 0,
    notes               text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_staff_work_logs_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_staff_work_logs_staff FOREIGN KEY (staff_id, organization_id)
        REFERENCES hotel.staff_members(id, organization_id),
    CONSTRAINT fk_staff_work_logs_booking FOREIGN KEY (booking_id, organization_id)
        REFERENCES hotel.bookings(id, organization_id),
    CONSTRAINT uq_staff_work_logs_uid UNIQUE (uid),
    CONSTRAINT ck_staff_work_logs_hours CHECK (hours_worked >= 0 AND overtime_hours >= 0)
);

CREATE TABLE hotel.staff_payments
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    staff_id            bigint NOT NULL,
    period_start        date NOT NULL,
    period_end          date NOT NULL,
    basic_amount        numeric(18,2) NOT NULL DEFAULT 0,
    overtime_amount     numeric(18,2) NOT NULL DEFAULT 0,
    bonus_amount        numeric(18,2) NOT NULL DEFAULT 0,
    deduction_amount    numeric(18,2) NOT NULL DEFAULT 0,
    total_paid          numeric(18,2) NOT NULL,
    payment_method      varchar(30) NOT NULL,
    paid_at             timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    reference_number    varchar(150),
    notes               text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_staff_payments_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_staff_payments_staff FOREIGN KEY (staff_id, organization_id)
        REFERENCES hotel.staff_members(id, organization_id),
    CONSTRAINT uq_staff_payments_uid UNIQUE (uid),
    CONSTRAINT ck_staff_payments_period CHECK (period_end >= period_start),
    CONSTRAINT ck_staff_payments_amounts CHECK
        (basic_amount >= 0 AND overtime_amount >= 0 AND bonus_amount >= 0 AND
         deduction_amount >= 0 AND total_paid >= 0),
    CONSTRAINT ck_staff_payments_method CHECK
        (payment_method IN ('CASH', 'BANK_TRANSFER', 'CARD', 'CHEQUE', 'OTHER'))
);

CREATE TABLE hotel.booking_cost_allocations
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint NOT NULL,
    property_id         bigint NOT NULL,
    booking_id          bigint NOT NULL,
    expense_id          bigint,
    allocation_month    date NOT NULL,
    allocation_type     varchar(30) NOT NULL,
    calculation_basis   varchar(30) NOT NULL,
    amount              numeric(18,2) NOT NULL,
    notes               text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_booking_cost_allocations_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT fk_booking_cost_allocations_booking FOREIGN KEY (booking_id, organization_id)
        REFERENCES hotel.bookings(id, organization_id),
    CONSTRAINT fk_booking_cost_allocations_expense FOREIGN KEY (expense_id, organization_id)
        REFERENCES hotel.expenses(id, organization_id),
    CONSTRAINT uq_booking_cost_allocations_uid UNIQUE (uid),
    CONSTRAINT ck_booking_cost_allocations_type CHECK (allocation_type IN ('DIRECT', 'SHARED_OVERHEAD')),
    CONSTRAINT ck_booking_cost_allocations_basis CHECK
        (calculation_basis IN ('DIRECT', 'GUEST_NIGHT', 'ROOM_NIGHT', 'REVENUE_SHARE', 'MANUAL')),
    CONSTRAINT ck_booking_cost_allocations_amount CHECK (amount >= 0),
    CONSTRAINT ck_booking_cost_allocations_month CHECK
        (allocation_month = date_trunc('month', allocation_month)::date)
);

CREATE TABLE hotel.audit_logs
(
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uid                 uuid NOT NULL DEFAULT gen_random_uuid(),
    organization_id     bigint,
    property_id         bigint,
    actor_subject       varchar(100),
    action              varchar(100) NOT NULL,
    entity_name         varchar(100) NOT NULL,
    entity_uid          uuid,
    old_values          jsonb,
    new_values          jsonb,
    ip_address          inet,
    user_agent          text,
    is_active           boolean NOT NULL DEFAULT true,
    is_archived         boolean NOT NULL DEFAULT false,
    creation_date       timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          varchar(100),
    modified_date       timestamptz,
    modified_by         varchar(100),
    CONSTRAINT fk_audit_logs_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id),
    CONSTRAINT fk_audit_logs_property FOREIGN KEY (property_id, organization_id)
        REFERENCES hotel.properties(id, organization_id),
    CONSTRAINT uq_audit_logs_uid UNIQUE (uid)
);

-- =============================================================
-- INDEXES
-- =============================================================

CREATE INDEX ix_properties_organization ON hotel.properties (organization_id) WHERE is_archived = false;
CREATE INDEX ix_user_property_access_user ON hotel.user_property_access (user_id, organization_id, property_id) WHERE is_active = true AND is_archived = false;
CREATE INDEX ix_accommodation_types_property ON hotel.accommodation_types (organization_id, property_id) WHERE is_active = true AND is_archived = false;
CREATE INDEX ix_accommodation_units_property_status ON hotel.accommodation_units (organization_id, property_id, status) WHERE is_archived = false;
CREATE INDEX ix_rate_plan_prices_dates ON hotel.rate_plan_prices (rate_plan_id, start_date, end_date) WHERE is_active = true AND is_archived = false;
CREATE INDEX ix_unit_blocks_dates ON hotel.unit_blocks (unit_id, start_date, end_date) WHERE is_active = true AND is_archived = false;
CREATE INDEX ix_guests_search_name ON hotel.guests (organization_id, lower(display_name)) WHERE is_archived = false;
CREATE INDEX ix_guests_phone ON hotel.guests (organization_id, phone) WHERE phone IS NOT NULL AND is_archived = false;
CREATE INDEX ix_bookings_property_dates ON hotel.bookings (organization_id, property_id, check_in_date, check_out_date) WHERE is_archived = false;
CREATE INDEX ix_bookings_property_status ON hotel.bookings (organization_id, property_id, status) WHERE is_archived = false;
CREATE INDEX ix_bookings_lead_guest ON hotel.bookings (organization_id, lead_guest_id) WHERE lead_guest_id IS NOT NULL AND is_archived = false;
CREATE INDEX ix_booking_units_booking ON hotel.booking_units (booking_id) WHERE is_archived = false;
CREATE INDEX ix_booking_units_unit_dates ON hotel.booking_units (unit_id, check_in_date, check_out_date) WHERE unit_id IS NOT NULL AND is_archived = false;
CREATE INDEX ix_booking_guests_guest ON hotel.booking_guests (organization_id, guest_id) WHERE is_archived = false;
CREATE INDEX ix_booking_charges_booking ON hotel.booking_charges (booking_id, service_date) WHERE is_archived = false;
CREATE INDEX ix_booking_payments_booking ON hotel.booking_payments (booking_id, paid_at) WHERE is_archived = false;
CREATE INDEX ix_expenses_property_date ON hotel.expenses (organization_id, property_id, expense_date) WHERE is_archived = false;
CREATE INDEX ix_utility_bills_property_period ON hotel.utility_bills (organization_id, property_id, period_start, period_end) WHERE is_archived = false;
CREATE INDEX ix_staff_payments_property_period ON hotel.staff_payments (organization_id, property_id, period_start, period_end) WHERE is_archived = false;
CREATE INDEX ix_audit_logs_entity ON hotel.audit_logs (organization_id, entity_name, entity_uid, creation_date DESC);

-- =============================================================
-- MODIFIED-DATE TRIGGERS
-- =============================================================

DO $$
DECLARE
    table_name text;
BEGIN
    FOREACH table_name IN ARRAY ARRAY[
        'organizations', 'properties', 'property_settings', 'app_users',
        'user_property_access', 'accommodation_types', 'accommodation_units',
        'meal_plans', 'rate_plans', 'rate_plan_prices', 'unit_blocks',
        'guests', 'guest_documents', 'bookings', 'booking_units',
        'booking_guests', 'booking_charge_types', 'booking_charges',
        'booking_payments', 'booking_refunds', 'booking_status_history',
        'expense_categories', 'suppliers', 'expenses', 'utility_types',
        'utility_bills', 'income_categories', 'other_income', 'staff_roles',
        'staff_members', 'staff_work_logs', 'staff_payments',
        'booking_cost_allocations', 'audit_logs'
    ]
    LOOP
        EXECUTE format(
            'CREATE TRIGGER trg_%I_modified_date BEFORE UPDATE ON hotel.%I '
            'FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date()',
            table_name, table_name
        );
    END LOOP;
END;
$$;

-- =============================================================
-- REPORTING VIEWS
-- =============================================================

CREATE OR REPLACE VIEW hotel.vw_booking_financial_summary AS
WITH unit_totals AS
(
    SELECT
        booking_id,
        SUM(total_amount) AS room_revenue
    FROM hotel.booking_units
    WHERE is_active = true AND is_archived = false
      AND allocation_status <> 'CANCELLED'
    GROUP BY booking_id
),
charge_totals AS
(
    SELECT
        booking_id,
        SUM(total_amount) AS extra_income
    FROM hotel.booking_charges
    WHERE is_active = true AND is_archived = false
    GROUP BY booking_id
),
payment_totals AS
(
    SELECT
        booking_id,
        SUM(amount) FILTER (WHERE status = 'COMPLETED') AS payments_received
    FROM hotel.booking_payments
    WHERE is_active = true AND is_archived = false
    GROUP BY booking_id
),
refund_totals AS
(
    SELECT
        booking_id,
        SUM(amount) FILTER (WHERE status = 'COMPLETED') AS refunds_paid
    FROM hotel.booking_refunds
    WHERE is_active = true AND is_archived = false
    GROUP BY booking_id
)
SELECT
    b.id AS booking_id,
    b.uid AS booking_uid,
    b.organization_id,
    b.property_id,
    b.booking_number,
    b.status,
    b.booking_source,
    b.check_in_date,
    b.check_out_date,
    b.number_of_nights,
    b.adults,
    b.children,
    b.infants,
    b.currency,
    COALESCE(ut.room_revenue, 0)::numeric(18,2) AS room_revenue,
    COALESCE(ct.extra_income, 0)::numeric(18,2) AS extra_income,
    b.discount_amount,
    b.tax_amount,
    b.service_charge,
    (COALESCE(ut.room_revenue, 0) + COALESCE(ct.extra_income, 0)
        - b.discount_amount + b.tax_amount + b.service_charge)::numeric(18,2) AS total_booking_value,
    COALESCE(pt.payments_received, 0)::numeric(18,2) AS payments_received,
    COALESCE(rt.refunds_paid, 0)::numeric(18,2) AS refunds_paid,
    (COALESCE(pt.payments_received, 0) - COALESCE(rt.refunds_paid, 0))::numeric(18,2) AS net_paid,
    (COALESCE(ut.room_revenue, 0) + COALESCE(ct.extra_income, 0)
        - b.discount_amount + b.tax_amount + b.service_charge
        - COALESCE(pt.payments_received, 0) + COALESCE(rt.refunds_paid, 0))::numeric(18,2) AS outstanding_balance
FROM hotel.bookings b
LEFT JOIN unit_totals ut ON ut.booking_id = b.id
LEFT JOIN charge_totals ct ON ct.booking_id = b.id
LEFT JOIN payment_totals pt ON pt.booking_id = b.id
LEFT JOIN refund_totals rt ON rt.booking_id = b.id
WHERE b.is_active = true AND b.is_archived = false;

CREATE OR REPLACE VIEW hotel.vw_booking_profitability AS
WITH allocated_cost AS
(
    SELECT
        booking_id,
        SUM(amount) AS allocated_cost
    FROM hotel.booking_cost_allocations
    WHERE is_active = true AND is_archived = false
      AND (allocation_type = 'SHARED_OVERHEAD'
           OR (allocation_type = 'DIRECT' AND expense_id IS NULL))
    GROUP BY booking_id
),
direct_expense AS
(
    SELECT
        booking_id,
        SUM(amount) AS direct_expense
    FROM hotel.expenses
    WHERE booking_id IS NOT NULL
      AND is_active = true AND is_archived = false
    GROUP BY booking_id
)
SELECT
    f.*,
    COALESCE(de.direct_expense, 0)::numeric(18,2) AS direct_expense,
    COALESCE(ac.allocated_cost, 0)::numeric(18,2) AS allocated_overhead,
    (f.total_booking_value - COALESCE(de.direct_expense, 0)
        - COALESCE(ac.allocated_cost, 0))::numeric(18,2) AS estimated_profit,
    CASE
        WHEN (f.adults + f.children) > 0 AND f.number_of_nights > 0
        THEN round(
            (f.total_booking_value - COALESCE(de.direct_expense, 0) - COALESCE(ac.allocated_cost, 0))
            / ((f.adults + f.children) * f.number_of_nights), 2)
        ELSE NULL
    END AS profit_per_guest_night
FROM hotel.vw_booking_financial_summary f
LEFT JOIN direct_expense de ON de.booking_id = f.booking_id
LEFT JOIN allocated_cost ac ON ac.booking_id = f.booking_id;

CREATE OR REPLACE VIEW hotel.vw_monthly_property_summary AS
WITH months AS
(
    SELECT organization_id, property_id, date_trunc('month', check_in_date)::date AS report_month
    FROM hotel.bookings
    WHERE is_active = true AND is_archived = false
    UNION
    SELECT organization_id, property_id, date_trunc('month', expense_date)::date
    FROM hotel.expenses
    WHERE is_active = true AND is_archived = false
    UNION
    SELECT organization_id, property_id, date_trunc('month', period_end)::date
    FROM hotel.utility_bills
    WHERE is_active = true AND is_archived = false
    UNION
    SELECT organization_id, property_id, date_trunc('month', paid_at)::date
    FROM hotel.staff_payments
    WHERE is_active = true AND is_archived = false
    UNION
    SELECT organization_id, property_id, date_trunc('month', income_date)::date
    FROM hotel.other_income
    WHERE is_active = true AND is_archived = false
),
booking_income AS
(
    SELECT
        organization_id,
        property_id,
        date_trunc('month', check_in_date)::date AS report_month,
        SUM(room_revenue) AS booking_revenue,
        SUM(extra_income) AS booking_extra_income,
        SUM(total_booking_value) AS total_booking_income
    FROM hotel.vw_booking_financial_summary
    WHERE status IN ('CONFIRMED', 'CHECKED_IN', 'CHECKED_OUT', 'COMPLETED')
    GROUP BY organization_id, property_id, date_trunc('month', check_in_date)::date
),
general_expense AS
(
    SELECT
        e.organization_id,
        e.property_id,
        date_trunc('month', e.expense_date)::date AS report_month,
        SUM(e.amount) AS general_expenses
    FROM hotel.expenses e
    JOIN hotel.expense_categories ec ON ec.id = e.expense_category_id
    WHERE e.is_active = true AND e.is_archived = false
      -- Staff and utility costs are taken from their dedicated tables below.
      AND ec.expense_group NOT IN ('STAFF', 'UTILITY')
    GROUP BY e.organization_id, e.property_id, date_trunc('month', e.expense_date)::date
),
utility_expense AS
(
    SELECT
        organization_id,
        property_id,
        date_trunc('month', period_end)::date AS report_month,
        SUM(amount) AS utility_expenses
    FROM hotel.utility_bills
    WHERE is_active = true AND is_archived = false
    GROUP BY organization_id, property_id, date_trunc('month', period_end)::date
),
staff_expense AS
(
    SELECT
        organization_id,
        property_id,
        date_trunc('month', paid_at)::date AS report_month,
        SUM(total_paid) AS staff_expenses
    FROM hotel.staff_payments
    WHERE is_active = true AND is_archived = false
    GROUP BY organization_id, property_id, date_trunc('month', paid_at)::date
),
additional_income AS
(
    SELECT
        organization_id,
        property_id,
        date_trunc('month', income_date)::date AS report_month,
        SUM(amount) AS other_income
    FROM hotel.other_income
    WHERE is_active = true AND is_archived = false
    GROUP BY organization_id, property_id, date_trunc('month', income_date)::date
)
SELECT
    m.organization_id,
    m.property_id,
    p.uid AS property_uid,
    p.name AS property_name,
    m.report_month,
    COALESCE(bi.booking_revenue, 0)::numeric(18,2) AS booking_revenue,
    COALESCE(bi.booking_extra_income, 0)::numeric(18,2) AS booking_extra_income,
    COALESCE(ai.other_income, 0)::numeric(18,2) AS other_income,
    (COALESCE(bi.total_booking_income, 0) + COALESCE(ai.other_income, 0))::numeric(18,2) AS total_income,
    COALESCE(se.staff_expenses, 0)::numeric(18,2) AS staff_expenses,
    COALESCE(ue.utility_expenses, 0)::numeric(18,2) AS utility_expenses,
    COALESCE(ge.general_expenses, 0)::numeric(18,2) AS general_expenses,
    (COALESCE(se.staff_expenses, 0) + COALESCE(ue.utility_expenses, 0)
        + COALESCE(ge.general_expenses, 0))::numeric(18,2) AS total_expenses,
    (COALESCE(bi.total_booking_income, 0) + COALESCE(ai.other_income, 0)
        - COALESCE(se.staff_expenses, 0) - COALESCE(ue.utility_expenses, 0)
        - COALESCE(ge.general_expenses, 0))::numeric(18,2) AS net_profit
FROM months m
JOIN hotel.properties p ON p.id = m.property_id
LEFT JOIN booking_income bi
    ON bi.organization_id = m.organization_id AND bi.property_id = m.property_id AND bi.report_month = m.report_month
LEFT JOIN general_expense ge
    ON ge.organization_id = m.organization_id AND ge.property_id = m.property_id AND ge.report_month = m.report_month
LEFT JOIN utility_expense ue
    ON ue.organization_id = m.organization_id AND ue.property_id = m.property_id AND ue.report_month = m.report_month
LEFT JOIN staff_expense se
    ON se.organization_id = m.organization_id AND se.property_id = m.property_id AND se.report_month = m.report_month
LEFT JOIN additional_income ai
    ON ai.organization_id = m.organization_id AND ai.property_id = m.property_id AND ai.report_month = m.report_month;

CREATE OR REPLACE VIEW hotel.vw_monthly_utility_summary AS
SELECT
    ub.organization_id,
    ub.property_id,
    p.uid AS property_uid,
    date_trunc('month', ub.period_end)::date AS report_month,
    ut.code AS utility_code,
    ut.name AS utility_name,
    ut.unit_of_measure,
    SUM(COALESCE(ub.units_used, 0))::numeric(18,3) AS total_units,
    SUM(ub.amount)::numeric(18,2) AS total_cost
FROM hotel.utility_bills ub
JOIN hotel.utility_types ut ON ut.id = ub.utility_type_id
JOIN hotel.properties p ON p.id = ub.property_id
WHERE ub.is_active = true AND ub.is_archived = false
GROUP BY ub.organization_id, ub.property_id, p.uid,
         date_trunc('month', ub.period_end)::date,
         ut.code, ut.name, ut.unit_of_measure;

CREATE OR REPLACE VIEW hotel.vw_payment_method_summary AS
SELECT
    bp.organization_id,
    bp.property_id,
    p.uid AS property_uid,
    date_trunc('month', bp.paid_at)::date AS report_month,
    bp.payment_method,
    bp.currency,
    COUNT(*) AS payment_count,
    SUM(bp.amount)::numeric(18,2) AS amount_received
FROM hotel.booking_payments bp
JOIN hotel.properties p ON p.id = bp.property_id
WHERE bp.status = 'COMPLETED'
  AND bp.is_active = true
  AND bp.is_archived = false
GROUP BY bp.organization_id, bp.property_id, p.uid,
         date_trunc('month', bp.paid_at)::date,
         bp.payment_method, bp.currency;

CREATE OR REPLACE VIEW hotel.vw_guest_booking_history AS
SELECT
    bg.organization_id,
    bg.property_id,
    g.uid AS guest_uid,
    g.display_name,
    b.uid AS booking_uid,
    b.booking_number,
    b.status,
    b.check_in_date,
    b.check_out_date,
    b.number_of_nights,
    f.total_booking_value,
    f.net_paid,
    f.outstanding_balance
FROM hotel.booking_guests bg
JOIN hotel.guests g ON g.id = bg.guest_id
JOIN hotel.bookings b ON b.id = bg.booking_id
JOIN hotel.vw_booking_financial_summary f ON f.booking_id = b.id
WHERE bg.is_active = true AND bg.is_archived = false;

COMMIT;

-- Recommended report filters:
-- SELECT * FROM hotel.vw_monthly_property_summary
-- WHERE property_uid = :property_uid AND report_month = DATE '2026-08-01';
--
-- SELECT * FROM hotel.vw_booking_profitability
-- WHERE property_id = :authorized_property_id ORDER BY check_in_date DESC;


