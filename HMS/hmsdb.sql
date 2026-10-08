--
-- PostgreSQL database dump
--

\restrict Mmhe4CqY2EFGMjXVMxVGVaJ7eqX5652Lgv9p4hxrBaT0XSLa3cXfdfcIrkwTeDg

-- Dumped from database version 16.15 (Debian 16.15-1.pgdg13+2)
-- Dumped by pg_dump version 16.15 (Debian 16.15-1.pgdg13+2)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: hotel; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA hotel;


ALTER SCHEMA hotel OWNER TO postgres;

--
-- Name: identity; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA identity;


ALTER SCHEMA identity OWNER TO postgres;

--
-- Name: btree_gist; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS btree_gist WITH SCHEMA public;


--
-- Name: EXTENSION btree_gist; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION btree_gist IS 'support for indexing common datatypes in GiST';


--
-- Name: pgcrypto; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;


--
-- Name: EXTENSION pgcrypto; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';


--
-- Name: set_modified_date(); Type: FUNCTION; Schema: hotel; Owner: postgres
--

CREATE FUNCTION hotel.set_modified_date() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    NEW.modified_date := CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$;


ALTER FUNCTION hotel.set_modified_date() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: accommodation_types; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.accommodation_types (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    code character varying(30) NOT NULL,
    name character varying(150) NOT NULL,
    unit_kind character varying(30) DEFAULT 'ROOM'::character varying NOT NULL,
    description text,
    max_adults integer DEFAULT 1 NOT NULL,
    max_children integer DEFAULT 0 NOT NULL,
    max_occupancy integer DEFAULT 1 NOT NULL,
    default_quantity integer DEFAULT 1 NOT NULL,
    base_rate numeric(18,2) DEFAULT 0 NOT NULL,
    sort_order integer DEFAULT 0 NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_accommodation_types_capacity CHECK (((max_adults >= 0) AND (max_children >= 0) AND (max_occupancy > 0) AND (max_occupancy >= max_adults))),
    CONSTRAINT ck_accommodation_types_kind CHECK (((unit_kind)::text = ANY (ARRAY[('ROOM'::character varying)::text, ('VILLA'::character varying)::text, ('APARTMENT'::character varying)::text, ('CABIN'::character varying)::text, ('TENT'::character varying)::text, ('DORM_BED'::character varying)::text, ('COTTAGE'::character varying)::text, ('ENTIRE_PROPERTY'::character varying)::text, ('OTHER'::character varying)::text]))),
    CONSTRAINT ck_accommodation_types_rate CHECK ((base_rate >= (0)::numeric))
);


ALTER TABLE hotel.accommodation_types OWNER TO postgres;

--
-- Name: accommodation_types_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.accommodation_types ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.accommodation_types_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: accommodation_units; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.accommodation_units (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    accommodation_type_id bigint NOT NULL,
    unit_code character varying(50) NOT NULL,
    unit_name character varying(150),
    floor_or_area character varying(100),
    status character varying(30) DEFAULT 'AVAILABLE'::character varying NOT NULL,
    housekeeping_status character varying(30) DEFAULT 'CLEAN'::character varying NOT NULL,
    notes text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_accommodation_units_housekeeping CHECK (((housekeeping_status)::text = ANY (ARRAY[('CLEAN'::character varying)::text, ('DIRTY'::character varying)::text, ('INSPECTED'::character varying)::text, ('IN_PROGRESS'::character varying)::text, ('NOT_APPLICABLE'::character varying)::text]))),
    CONSTRAINT ck_accommodation_units_status CHECK (((status)::text = ANY (ARRAY[('AVAILABLE'::character varying)::text, ('OCCUPIED'::character varying)::text, ('OUT_OF_SERVICE'::character varying)::text, ('MAINTENANCE'::character varying)::text, ('INACTIVE'::character varying)::text])))
);


ALTER TABLE hotel.accommodation_units OWNER TO postgres;

--
-- Name: accommodation_units_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.accommodation_units ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.accommodation_units_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: app_users; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.app_users (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    auth_subject character varying(100) NOT NULL,
    user_name character varying(100),
    first_name character varying(100),
    last_name character varying(100),
    email character varying(254),
    phone character varying(30),
    status character varying(30) DEFAULT 'ACTIVE'::character varying NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_app_users_status CHECK (((status)::text = ANY (ARRAY[('INVITED'::character varying)::text, ('ACTIVE'::character varying)::text, ('LOCKED'::character varying)::text, ('DISABLED'::character varying)::text])))
);


ALTER TABLE hotel.app_users OWNER TO postgres;

--
-- Name: app_users_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.app_users ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.app_users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: audit_logs; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.audit_logs (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint,
    property_id bigint,
    actor_subject character varying(100),
    action character varying(100) NOT NULL,
    entity_name character varying(100) NOT NULL,
    entity_uid uuid,
    old_values jsonb,
    new_values jsonb,
    ip_address inet,
    user_agent text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100)
);


ALTER TABLE hotel.audit_logs OWNER TO postgres;

--
-- Name: audit_logs_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.audit_logs ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.audit_logs_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: booking_charge_types; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.booking_charge_types (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    code character varying(30) NOT NULL,
    name character varying(100) NOT NULL,
    category character varying(30) NOT NULL,
    is_taxable boolean DEFAULT false NOT NULL,
    default_price numeric(18,2),
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_booking_charge_types_category CHECK (((category)::text = ANY (ARRAY[('FOOD'::character varying)::text, ('COOKING'::character varying)::text, ('LAUNDRY'::character varying)::text, ('BEVERAGE'::character varying)::text, ('TRANSPORT'::character varying)::text, ('ACTIVITY'::character varying)::text, ('DAMAGE'::character varying)::text, ('OTHER'::character varying)::text]))),
    CONSTRAINT ck_booking_charge_types_price CHECK (((default_price IS NULL) OR (default_price >= (0)::numeric)))
);


ALTER TABLE hotel.booking_charge_types OWNER TO postgres;

--
-- Name: booking_charge_types_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.booking_charge_types ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.booking_charge_types_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: booking_charges; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.booking_charges (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    booking_id bigint NOT NULL,
    booking_unit_id bigint,
    charge_type_id bigint NOT NULL,
    service_date date DEFAULT CURRENT_DATE NOT NULL,
    description character varying(250) NOT NULL,
    quantity numeric(12,3) DEFAULT 1 NOT NULL,
    unit_price numeric(18,2) NOT NULL,
    discount_amount numeric(18,2) DEFAULT 0 NOT NULL,
    tax_amount numeric(18,2) DEFAULT 0 NOT NULL,
    total_amount numeric(18,2) GENERATED ALWAYS AS (round((((quantity * unit_price) - discount_amount) + tax_amount), 2)) STORED,
    notes text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_booking_charges_amounts CHECK (((quantity > (0)::numeric) AND (unit_price >= (0)::numeric) AND (discount_amount >= (0)::numeric) AND (tax_amount >= (0)::numeric)))
);


ALTER TABLE hotel.booking_charges OWNER TO postgres;

--
-- Name: booking_charges_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.booking_charges ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.booking_charges_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: booking_cost_allocations; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.booking_cost_allocations (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    booking_id bigint NOT NULL,
    expense_id bigint,
    allocation_month date NOT NULL,
    allocation_type character varying(30) NOT NULL,
    calculation_basis character varying(30) NOT NULL,
    amount numeric(18,2) NOT NULL,
    notes text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_booking_cost_allocations_amount CHECK ((amount >= (0)::numeric)),
    CONSTRAINT ck_booking_cost_allocations_basis CHECK (((calculation_basis)::text = ANY (ARRAY[('DIRECT'::character varying)::text, ('GUEST_NIGHT'::character varying)::text, ('ROOM_NIGHT'::character varying)::text, ('REVENUE_SHARE'::character varying)::text, ('MANUAL'::character varying)::text]))),
    CONSTRAINT ck_booking_cost_allocations_month CHECK ((allocation_month = (date_trunc('month'::text, (allocation_month)::timestamp with time zone))::date)),
    CONSTRAINT ck_booking_cost_allocations_type CHECK (((allocation_type)::text = ANY (ARRAY[('DIRECT'::character varying)::text, ('SHARED_OVERHEAD'::character varying)::text])))
);


ALTER TABLE hotel.booking_cost_allocations OWNER TO postgres;

--
-- Name: booking_cost_allocations_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.booking_cost_allocations ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.booking_cost_allocations_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: booking_guests; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.booking_guests (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    booking_id bigint NOT NULL,
    booking_unit_id bigint,
    guest_id bigint NOT NULL,
    is_lead_guest boolean DEFAULT false NOT NULL,
    checked_in_at timestamp with time zone,
    checked_out_at timestamp with time zone,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100)
);


ALTER TABLE hotel.booking_guests OWNER TO postgres;

--
-- Name: booking_guests_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.booking_guests ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.booking_guests_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: booking_payments; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.booking_payments (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    booking_id bigint NOT NULL,
    payment_method character varying(30) NOT NULL,
    payment_type character varying(30) DEFAULT 'PAYMENT'::character varying NOT NULL,
    amount numeric(18,2) NOT NULL,
    currency character(3) DEFAULT 'LKR'::bpchar NOT NULL,
    status character varying(30) DEFAULT 'COMPLETED'::character varying NOT NULL,
    reference_number character varying(150),
    paid_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    notes text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_booking_payments_amount CHECK ((amount > (0)::numeric)),
    CONSTRAINT ck_booking_payments_method CHECK (((payment_method)::text = ANY (ARRAY[('CASH'::character varying)::text, ('BANK_TRANSFER'::character varying)::text, ('CARD'::character varying)::text, ('ONLINE_GATEWAY'::character varying)::text, ('CHEQUE'::character varying)::text, ('OTHER'::character varying)::text]))),
    CONSTRAINT ck_booking_payments_status CHECK (((status)::text = ANY (ARRAY[('PENDING'::character varying)::text, ('COMPLETED'::character varying)::text, ('FAILED'::character varying)::text, ('CANCELLED'::character varying)::text]))),
    CONSTRAINT ck_booking_payments_type CHECK (((payment_type)::text = ANY (ARRAY[('DEPOSIT'::character varying)::text, ('PAYMENT'::character varying)::text, ('ADJUSTMENT'::character varying)::text])))
);


ALTER TABLE hotel.booking_payments OWNER TO postgres;

--
-- Name: booking_payments_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.booking_payments ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.booking_payments_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: booking_refunds; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.booking_refunds (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    booking_id bigint NOT NULL,
    payment_id bigint NOT NULL,
    amount numeric(18,2) NOT NULL,
    status character varying(30) DEFAULT 'COMPLETED'::character varying NOT NULL,
    reason text NOT NULL,
    refunded_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_booking_refunds_amount CHECK ((amount > (0)::numeric)),
    CONSTRAINT ck_booking_refunds_status CHECK (((status)::text = ANY (ARRAY[('PENDING'::character varying)::text, ('COMPLETED'::character varying)::text, ('FAILED'::character varying)::text, ('CANCELLED'::character varying)::text])))
);


ALTER TABLE hotel.booking_refunds OWNER TO postgres;

--
-- Name: booking_refunds_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.booking_refunds ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.booking_refunds_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: booking_status_history; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.booking_status_history (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    booking_id bigint NOT NULL,
    old_status character varying(30),
    new_status character varying(30) NOT NULL,
    reason text,
    changed_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    changed_by character varying(100),
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100)
);


ALTER TABLE hotel.booking_status_history OWNER TO postgres;

--
-- Name: booking_status_history_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.booking_status_history ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.booking_status_history_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: booking_units; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.booking_units (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    booking_id bigint NOT NULL,
    accommodation_type_id bigint NOT NULL,
    unit_id bigint,
    rate_plan_id bigint,
    meal_plan_id bigint,
    check_in_date date NOT NULL,
    check_out_date date NOT NULL,
    number_of_nights integer GENERATED ALWAYS AS ((check_out_date - check_in_date)) STORED,
    adults integer DEFAULT 1 NOT NULL,
    children integer DEFAULT 0 NOT NULL,
    unit_quantity integer DEFAULT 1 NOT NULL,
    guest_count integer DEFAULT 1 NOT NULL,
    pricing_basis character varying(30) NOT NULL,
    unit_rate numeric(18,2) NOT NULL,
    discount_amount numeric(18,2) DEFAULT 0 NOT NULL,
    tax_amount numeric(18,2) DEFAULT 0 NOT NULL,
    total_amount numeric(18,2) NOT NULL,
    allocation_status character varying(30) DEFAULT 'HELD'::character varying NOT NULL,
    notes text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_booking_units_allocation_status CHECK (((allocation_status)::text = ANY (ARRAY[('HELD'::character varying)::text, ('CONFIRMED'::character varying)::text, ('CHECKED_IN'::character varying)::text, ('RELEASED'::character varying)::text, ('CANCELLED'::character varying)::text]))),
    CONSTRAINT ck_booking_units_amounts CHECK (((unit_rate >= (0)::numeric) AND (discount_amount >= (0)::numeric) AND (tax_amount >= (0)::numeric) AND (total_amount >= (0)::numeric))),
    CONSTRAINT ck_booking_units_counts CHECK (((adults >= 0) AND (children >= 0) AND (unit_quantity > 0) AND (guest_count > 0))),
    CONSTRAINT ck_booking_units_dates CHECK ((check_out_date > check_in_date)),
    CONSTRAINT ck_booking_units_pricing_basis CHECK (((pricing_basis)::text = ANY (ARRAY[('PER_ROOM_PER_NIGHT'::character varying)::text, ('PER_PERSON_PER_NIGHT'::character varying)::text, ('PER_BED_PER_NIGHT'::character varying)::text, ('FLAT_PER_STAY'::character varying)::text])))
);


ALTER TABLE hotel.booking_units OWNER TO postgres;

--
-- Name: booking_units_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.booking_units ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.booking_units_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: bookings; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.bookings (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    booking_number character varying(50) NOT NULL,
    lead_guest_id bigint,
    booking_source character varying(30) DEFAULT 'DIRECT'::character varying NOT NULL,
    external_reference character varying(100),
    status character varying(30) DEFAULT 'PENDING'::character varying NOT NULL,
    check_in_date date NOT NULL,
    check_out_date date NOT NULL,
    number_of_nights integer GENERATED ALWAYS AS ((check_out_date - check_in_date)) STORED,
    adults integer DEFAULT 1 NOT NULL,
    children integer DEFAULT 0 NOT NULL,
    infants integer DEFAULT 0 NOT NULL,
    currency character(3) DEFAULT 'LKR'::bpchar NOT NULL,
    discount_amount numeric(18,2) DEFAULT 0 NOT NULL,
    tax_amount numeric(18,2) DEFAULT 0 NOT NULL,
    service_charge numeric(18,2) DEFAULT 0 NOT NULL,
    quoted_total numeric(18,2),
    arrival_time time without time zone,
    departure_time time without time zone,
    special_requests text,
    internal_notes text,
    confirmed_at timestamp with time zone,
    checked_in_at timestamp with time zone,
    checked_out_at timestamp with time zone,
    cancelled_at timestamp with time zone,
    cancellation_reason text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_bookings_amounts CHECK (((discount_amount >= (0)::numeric) AND (tax_amount >= (0)::numeric) AND (service_charge >= (0)::numeric) AND (COALESCE(quoted_total, (0)::numeric) >= (0)::numeric))),
    CONSTRAINT ck_bookings_dates CHECK ((check_out_date > check_in_date)),
    CONSTRAINT ck_bookings_guests CHECK (((adults > 0) AND (children >= 0) AND (infants >= 0))),
    CONSTRAINT ck_bookings_source CHECK (((booking_source)::text = ANY (ARRAY[('WALK_IN'::character varying)::text, ('PHONE'::character varying)::text, ('WHATSAPP'::character varying)::text, ('DIRECT'::character varying)::text, ('WEBSITE'::character varying)::text, ('TRAVEL_AGENT'::character varying)::text, ('BOOKING_COM'::character varying)::text, ('AIRBNB'::character varying)::text, ('OTHER'::character varying)::text]))),
    CONSTRAINT ck_bookings_status CHECK (((status)::text = ANY (ARRAY[('INQUIRY'::character varying)::text, ('PENDING'::character varying)::text, ('TENTATIVE'::character varying)::text, ('CONFIRMED'::character varying)::text, ('CHECKED_IN'::character varying)::text, ('CHECKED_OUT'::character varying)::text, ('COMPLETED'::character varying)::text, ('CANCELLED'::character varying)::text, ('NO_SHOW'::character varying)::text])))
);


ALTER TABLE hotel.bookings OWNER TO postgres;

--
-- Name: bookings_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.bookings ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.bookings_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: expense_categories; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.expense_categories (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    parent_id bigint,
    code character varying(30) NOT NULL,
    name character varying(100) NOT NULL,
    expense_group character varying(30) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_expense_categories_group CHECK (((expense_group)::text = ANY (ARRAY[('STAFF'::character varying)::text, ('UTILITY'::character varying)::text, ('FOOD'::character varying)::text, ('MAINTENANCE'::character varying)::text, ('TRANSPORT'::character varying)::text, ('MARKETING'::character varying)::text, ('SUPPLIES'::character varying)::text, ('ADMINISTRATION'::character varying)::text, ('OTHER'::character varying)::text])))
);


ALTER TABLE hotel.expense_categories OWNER TO postgres;

--
-- Name: expense_categories_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.expense_categories ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.expense_categories_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: expenses; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.expenses (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    expense_category_id bigint NOT NULL,
    supplier_id bigint,
    booking_id bigint,
    expense_date date NOT NULL,
    description character varying(300) NOT NULL,
    amount numeric(18,2) NOT NULL,
    currency character(3) DEFAULT 'LKR'::bpchar NOT NULL,
    payment_method character varying(30),
    reference_number character varying(150),
    receipt_url text,
    notes text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_expenses_amount CHECK ((amount > (0)::numeric)),
    CONSTRAINT ck_expenses_payment_method CHECK (((payment_method IS NULL) OR ((payment_method)::text = ANY (ARRAY[('CASH'::character varying)::text, ('BANK_TRANSFER'::character varying)::text, ('CARD'::character varying)::text, ('CHEQUE'::character varying)::text, ('OTHER'::character varying)::text]))))
);


ALTER TABLE hotel.expenses OWNER TO postgres;

--
-- Name: expenses_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.expenses ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.expenses_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: guest_documents; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.guest_documents (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    guest_id bigint NOT NULL,
    document_type character varying(30) NOT NULL,
    document_number character varying(100) NOT NULL,
    issuing_country character(2),
    issued_date date,
    expiry_date date,
    file_url text,
    is_verified boolean DEFAULT false NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_guest_documents_expiry CHECK (((expiry_date IS NULL) OR (issued_date IS NULL) OR (expiry_date >= issued_date))),
    CONSTRAINT ck_guest_documents_type CHECK (((document_type)::text = ANY (ARRAY[('NIC'::character varying)::text, ('PASSPORT'::character varying)::text, ('DRIVING_LICENCE'::character varying)::text, ('OTHER'::character varying)::text])))
);


ALTER TABLE hotel.guest_documents OWNER TO postgres;

--
-- Name: guest_documents_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.guest_documents ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.guest_documents_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: guests; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.guests (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    guest_type character varying(30) DEFAULT 'INDIVIDUAL'::character varying NOT NULL,
    title character varying(20),
    first_name character varying(100),
    last_name character varying(100),
    display_name character varying(200) NOT NULL,
    phone character varying(30),
    alternate_phone character varying(30),
    email character varying(254),
    nationality_code character(2),
    date_of_birth date,
    preferred_language character varying(10),
    address text,
    city character varying(100),
    country_code character(2),
    notes text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_guests_type CHECK (((guest_type)::text = ANY (ARRAY[('INDIVIDUAL'::character varying)::text, ('COUPLE'::character varying)::text, ('FAMILY'::character varying)::text, ('GROUP'::character varying)::text, ('CORPORATE'::character varying)::text, ('TRAVEL_AGENT'::character varying)::text])))
);


ALTER TABLE hotel.guests OWNER TO postgres;

--
-- Name: guests_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.guests ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.guests_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: income_categories; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.income_categories (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    code character varying(30) NOT NULL,
    name character varying(100) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100)
);


ALTER TABLE hotel.income_categories OWNER TO postgres;

--
-- Name: income_categories_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.income_categories ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.income_categories_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: meal_plans; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.meal_plans (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    code character varying(30) NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    includes_breakfast boolean DEFAULT false NOT NULL,
    includes_lunch boolean DEFAULT false NOT NULL,
    includes_dinner boolean DEFAULT false NOT NULL,
    allow_byo boolean DEFAULT false NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100)
);


ALTER TABLE hotel.meal_plans OWNER TO postgres;

--
-- Name: meal_plans_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.meal_plans ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.meal_plans_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: organizations; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.organizations (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    code character varying(30) NOT NULL,
    name character varying(200) NOT NULL,
    legal_name character varying(250),
    default_currency character(3) DEFAULT 'LKR'::bpchar NOT NULL,
    timezone character varying(100) DEFAULT 'Asia/Colombo'::character varying NOT NULL,
    status character varying(30) DEFAULT 'ACTIVE'::character varying NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_organizations_status CHECK (((status)::text = ANY (ARRAY[('TRIAL'::character varying)::text, ('ACTIVE'::character varying)::text, ('SUSPENDED'::character varying)::text, ('CLOSED'::character varying)::text])))
);


ALTER TABLE hotel.organizations OWNER TO postgres;

--
-- Name: organizations_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.organizations ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.organizations_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: other_income; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.other_income (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    income_category_id bigint NOT NULL,
    booking_id bigint,
    income_date date NOT NULL,
    description character varying(300) NOT NULL,
    amount numeric(18,2) NOT NULL,
    currency character(3) DEFAULT 'LKR'::bpchar NOT NULL,
    payment_method character varying(30),
    reference_number character varying(150),
    notes text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_other_income_amount CHECK ((amount > (0)::numeric))
);


ALTER TABLE hotel.other_income OWNER TO postgres;

--
-- Name: other_income_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.other_income ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.other_income_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: properties; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.properties (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    code character varying(30) NOT NULL,
    name character varying(200) NOT NULL,
    slug character varying(200) NOT NULL,
    property_type character varying(30) NOT NULL,
    description text,
    address_line1 character varying(250),
    address_line2 character varying(250),
    city character varying(100),
    district character varying(100),
    province character varying(100),
    postal_code character varying(20),
    country_code character(2) DEFAULT 'LK'::bpchar NOT NULL,
    latitude numeric(10,7),
    longitude numeric(10,7),
    phone character varying(30),
    email character varying(254),
    timezone character varying(100) DEFAULT 'Asia/Colombo'::character varying NOT NULL,
    default_currency character(3) DEFAULT 'LKR'::bpchar NOT NULL,
    status character varying(30) DEFAULT 'ACTIVE'::character varying NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_properties_latitude CHECK (((latitude IS NULL) OR ((latitude >= ('-90'::integer)::numeric) AND (latitude <= (90)::numeric)))),
    CONSTRAINT ck_properties_longitude CHECK (((longitude IS NULL) OR ((longitude >= ('-180'::integer)::numeric) AND (longitude <= (180)::numeric)))),
    CONSTRAINT ck_properties_status CHECK (((status)::text = ANY (ARRAY[('DRAFT'::character varying)::text, ('ACTIVE'::character varying)::text, ('SUSPENDED'::character varying)::text, ('CLOSED'::character varying)::text]))),
    CONSTRAINT ck_properties_type CHECK (((property_type)::text = ANY (ARRAY[('HOTEL'::character varying)::text, ('GUESTHOUSE'::character varying)::text, ('VILLA'::character varying)::text, ('APARTMENT'::character varying)::text, ('HOSTEL'::character varying)::text, ('HOMESTAY'::character varying)::text, ('RESORT'::character varying)::text, ('CAMPSITE'::character varying)::text, ('LODGE'::character varying)::text, ('BUNGALOW'::character varying)::text, ('OTHER'::character varying)::text])))
);


ALTER TABLE hotel.properties OWNER TO postgres;

--
-- Name: properties_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.properties ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.properties_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: property_settings; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.property_settings (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    check_in_time time without time zone DEFAULT '14:00:00'::time without time zone NOT NULL,
    check_out_time time without time zone DEFAULT '11:00:00'::time without time zone NOT NULL,
    booking_number_prefix character varying(20) DEFAULT 'BKG'::character varying NOT NULL,
    invoice_number_prefix character varying(20) DEFAULT 'INV'::character varying NOT NULL,
    tax_rate numeric(7,4) DEFAULT 0 NOT NULL,
    service_charge_rate numeric(7,4) DEFAULT 0 NOT NULL,
    allow_overbooking boolean DEFAULT false NOT NULL,
    extra_settings jsonb DEFAULT '{}'::jsonb NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_property_settings_service_charge CHECK (((service_charge_rate >= (0)::numeric) AND (service_charge_rate <= (100)::numeric))),
    CONSTRAINT ck_property_settings_tax CHECK (((tax_rate >= (0)::numeric) AND (tax_rate <= (100)::numeric)))
);


ALTER TABLE hotel.property_settings OWNER TO postgres;

--
-- Name: property_settings_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.property_settings ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.property_settings_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: rate_plan_prices; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.rate_plan_prices (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    rate_plan_id bigint NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    day_of_week smallint,
    adult_rate numeric(18,2),
    child_rate numeric(18,2),
    unit_rate numeric(18,2) NOT NULL,
    minimum_stay integer DEFAULT 1 NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_rate_plan_prices_amounts CHECK (((unit_rate >= (0)::numeric) AND (COALESCE(adult_rate, (0)::numeric) >= (0)::numeric) AND (COALESCE(child_rate, (0)::numeric) >= (0)::numeric))),
    CONSTRAINT ck_rate_plan_prices_dates CHECK ((end_date >= start_date)),
    CONSTRAINT ck_rate_plan_prices_day CHECK (((day_of_week IS NULL) OR ((day_of_week >= 0) AND (day_of_week <= 6)))),
    CONSTRAINT ck_rate_plan_prices_minimum_stay CHECK ((minimum_stay > 0))
);


ALTER TABLE hotel.rate_plan_prices OWNER TO postgres;

--
-- Name: rate_plan_prices_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.rate_plan_prices ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.rate_plan_prices_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: rate_plans; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.rate_plans (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    accommodation_type_id bigint NOT NULL,
    meal_plan_id bigint,
    code character varying(30) NOT NULL,
    name character varying(150) NOT NULL,
    pricing_basis character varying(30) NOT NULL,
    currency character(3) DEFAULT 'LKR'::bpchar NOT NULL,
    description text,
    is_refundable boolean DEFAULT true NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_rate_plans_pricing_basis CHECK (((pricing_basis)::text = ANY (ARRAY[('PER_ROOM_PER_NIGHT'::character varying)::text, ('PER_PERSON_PER_NIGHT'::character varying)::text, ('PER_BED_PER_NIGHT'::character varying)::text, ('FLAT_PER_STAY'::character varying)::text])))
);


ALTER TABLE hotel.rate_plans OWNER TO postgres;

--
-- Name: rate_plans_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.rate_plans ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.rate_plans_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: staff_members; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.staff_members (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    staff_role_id bigint NOT NULL,
    employee_number character varying(50) NOT NULL,
    first_name character varying(100) NOT NULL,
    last_name character varying(100),
    phone character varying(30),
    email character varying(254),
    employment_type character varying(30) NOT NULL,
    basic_salary numeric(18,2),
    daily_rate numeric(18,2),
    hourly_rate numeric(18,2),
    joined_date date,
    left_date date,
    status character varying(30) DEFAULT 'ACTIVE'::character varying NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_staff_members_dates CHECK (((left_date IS NULL) OR (joined_date IS NULL) OR (left_date >= joined_date))),
    CONSTRAINT ck_staff_members_employment CHECK (((employment_type)::text = ANY (ARRAY[('MONTHLY'::character varying)::text, ('DAILY'::character varying)::text, ('HOURLY'::character varying)::text, ('CONTRACT'::character varying)::text]))),
    CONSTRAINT ck_staff_members_rates CHECK (((COALESCE(basic_salary, (0)::numeric) >= (0)::numeric) AND (COALESCE(daily_rate, (0)::numeric) >= (0)::numeric) AND (COALESCE(hourly_rate, (0)::numeric) >= (0)::numeric))),
    CONSTRAINT ck_staff_members_status CHECK (((status)::text = ANY (ARRAY[('ACTIVE'::character varying)::text, ('ON_LEAVE'::character varying)::text, ('LEFT'::character varying)::text, ('SUSPENDED'::character varying)::text])))
);


ALTER TABLE hotel.staff_members OWNER TO postgres;

--
-- Name: staff_members_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.staff_members ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.staff_members_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: staff_payments; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.staff_payments (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    staff_id bigint NOT NULL,
    period_start date NOT NULL,
    period_end date NOT NULL,
    basic_amount numeric(18,2) DEFAULT 0 NOT NULL,
    overtime_amount numeric(18,2) DEFAULT 0 NOT NULL,
    bonus_amount numeric(18,2) DEFAULT 0 NOT NULL,
    deduction_amount numeric(18,2) DEFAULT 0 NOT NULL,
    total_paid numeric(18,2) NOT NULL,
    payment_method character varying(30) NOT NULL,
    paid_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    reference_number character varying(150),
    notes text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_staff_payments_amounts CHECK (((basic_amount >= (0)::numeric) AND (overtime_amount >= (0)::numeric) AND (bonus_amount >= (0)::numeric) AND (deduction_amount >= (0)::numeric) AND (total_paid >= (0)::numeric))),
    CONSTRAINT ck_staff_payments_method CHECK (((payment_method)::text = ANY (ARRAY[('CASH'::character varying)::text, ('BANK_TRANSFER'::character varying)::text, ('CARD'::character varying)::text, ('CHEQUE'::character varying)::text, ('OTHER'::character varying)::text]))),
    CONSTRAINT ck_staff_payments_period CHECK ((period_end >= period_start))
);


ALTER TABLE hotel.staff_payments OWNER TO postgres;

--
-- Name: staff_payments_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.staff_payments ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.staff_payments_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: staff_roles; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.staff_roles (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    code character varying(30) NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100)
);


ALTER TABLE hotel.staff_roles OWNER TO postgres;

--
-- Name: staff_roles_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.staff_roles ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.staff_roles_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: staff_work_logs; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.staff_work_logs (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    staff_id bigint NOT NULL,
    booking_id bigint,
    work_date date NOT NULL,
    start_time time without time zone,
    end_time time without time zone,
    hours_worked numeric(8,2) DEFAULT 0 NOT NULL,
    overtime_hours numeric(8,2) DEFAULT 0 NOT NULL,
    notes text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_staff_work_logs_hours CHECK (((hours_worked >= (0)::numeric) AND (overtime_hours >= (0)::numeric)))
);


ALTER TABLE hotel.staff_work_logs OWNER TO postgres;

--
-- Name: staff_work_logs_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.staff_work_logs ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.staff_work_logs_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: suppliers; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.suppliers (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    name character varying(200) NOT NULL,
    phone character varying(30),
    email character varying(254),
    address text,
    tax_number character varying(100),
    notes text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100)
);


ALTER TABLE hotel.suppliers OWNER TO postgres;

--
-- Name: suppliers_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.suppliers ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.suppliers_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: unit_blocks; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.unit_blocks (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    unit_id bigint NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    block_type character varying(30) NOT NULL,
    reason text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_unit_blocks_dates CHECK ((end_date > start_date)),
    CONSTRAINT ck_unit_blocks_type CHECK (((block_type)::text = ANY (ARRAY[('MAINTENANCE'::character varying)::text, ('OWNER_USE'::character varying)::text, ('CLOSED'::character varying)::text, ('OTHER'::character varying)::text])))
);


ALTER TABLE hotel.unit_blocks OWNER TO postgres;

--
-- Name: unit_blocks_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.unit_blocks ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.unit_blocks_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: user_property_access; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.user_property_access (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id bigint NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint,
    role_code character varying(50) NOT NULL,
    is_default_property boolean DEFAULT false NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100)
);


ALTER TABLE hotel.user_property_access OWNER TO postgres;

--
-- Name: user_property_access_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.user_property_access ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.user_property_access_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: utility_bills; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.utility_bills (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    property_id bigint NOT NULL,
    utility_type_id bigint NOT NULL,
    period_start date NOT NULL,
    period_end date NOT NULL,
    previous_reading numeric(18,3),
    current_reading numeric(18,3),
    units_used numeric(18,3),
    amount numeric(18,2) NOT NULL,
    currency character(3) DEFAULT 'LKR'::bpchar NOT NULL,
    due_date date,
    paid_at timestamp with time zone,
    reference_number character varying(150),
    notes text,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100),
    CONSTRAINT ck_utility_bills_amount CHECK ((amount >= (0)::numeric)),
    CONSTRAINT ck_utility_bills_period CHECK ((period_end >= period_start)),
    CONSTRAINT ck_utility_bills_readings CHECK (((previous_reading IS NULL) OR (current_reading IS NULL) OR (current_reading >= previous_reading)))
);


ALTER TABLE hotel.utility_bills OWNER TO postgres;

--
-- Name: utility_bills_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.utility_bills ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.utility_bills_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: utility_types; Type: TABLE; Schema: hotel; Owner: postgres
--

CREATE TABLE hotel.utility_types (
    id bigint NOT NULL,
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    organization_id bigint NOT NULL,
    code character varying(30) NOT NULL,
    name character varying(100) NOT NULL,
    unit_of_measure character varying(30),
    is_metered boolean DEFAULT true NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_archived boolean DEFAULT false NOT NULL,
    creation_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by character varying(100),
    modified_date timestamp with time zone,
    modified_by character varying(100)
);


ALTER TABLE hotel.utility_types OWNER TO postgres;

--
-- Name: utility_types_id_seq; Type: SEQUENCE; Schema: hotel; Owner: postgres
--

ALTER TABLE hotel.utility_types ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME hotel.utility_types_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: vw_booking_financial_summary; Type: VIEW; Schema: hotel; Owner: postgres
--

CREATE VIEW hotel.vw_booking_financial_summary AS
 WITH unit_totals AS (
         SELECT booking_units.booking_id,
            sum(booking_units.total_amount) AS room_revenue
           FROM hotel.booking_units
          WHERE ((booking_units.is_active = true) AND (booking_units.is_archived = false) AND ((booking_units.allocation_status)::text <> 'CANCELLED'::text))
          GROUP BY booking_units.booking_id
        ), charge_totals AS (
         SELECT booking_charges.booking_id,
            sum(booking_charges.total_amount) AS extra_income
           FROM hotel.booking_charges
          WHERE ((booking_charges.is_active = true) AND (booking_charges.is_archived = false))
          GROUP BY booking_charges.booking_id
        ), payment_totals AS (
         SELECT booking_payments.booking_id,
            sum(booking_payments.amount) FILTER (WHERE ((booking_payments.status)::text = 'COMPLETED'::text)) AS payments_received
           FROM hotel.booking_payments
          WHERE ((booking_payments.is_active = true) AND (booking_payments.is_archived = false))
          GROUP BY booking_payments.booking_id
        ), refund_totals AS (
         SELECT booking_refunds.booking_id,
            sum(booking_refunds.amount) FILTER (WHERE ((booking_refunds.status)::text = 'COMPLETED'::text)) AS refunds_paid
           FROM hotel.booking_refunds
          WHERE ((booking_refunds.is_active = true) AND (booking_refunds.is_archived = false))
          GROUP BY booking_refunds.booking_id
        )
 SELECT b.id AS booking_id,
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
    (COALESCE(ut.room_revenue, (0)::numeric))::numeric(18,2) AS room_revenue,
    (COALESCE(ct.extra_income, (0)::numeric))::numeric(18,2) AS extra_income,
    b.discount_amount,
    b.tax_amount,
    b.service_charge,
    (((((COALESCE(ut.room_revenue, (0)::numeric) + COALESCE(ct.extra_income, (0)::numeric)) - b.discount_amount) + b.tax_amount) + b.service_charge))::numeric(18,2) AS total_booking_value,
    (COALESCE(pt.payments_received, (0)::numeric))::numeric(18,2) AS payments_received,
    (COALESCE(rt.refunds_paid, (0)::numeric))::numeric(18,2) AS refunds_paid,
    ((COALESCE(pt.payments_received, (0)::numeric) - COALESCE(rt.refunds_paid, (0)::numeric)))::numeric(18,2) AS net_paid,
    (((((((COALESCE(ut.room_revenue, (0)::numeric) + COALESCE(ct.extra_income, (0)::numeric)) - b.discount_amount) + b.tax_amount) + b.service_charge) - COALESCE(pt.payments_received, (0)::numeric)) + COALESCE(rt.refunds_paid, (0)::numeric)))::numeric(18,2) AS outstanding_balance
   FROM ((((hotel.bookings b
     LEFT JOIN unit_totals ut ON ((ut.booking_id = b.id)))
     LEFT JOIN charge_totals ct ON ((ct.booking_id = b.id)))
     LEFT JOIN payment_totals pt ON ((pt.booking_id = b.id)))
     LEFT JOIN refund_totals rt ON ((rt.booking_id = b.id)))
  WHERE ((b.is_active = true) AND (b.is_archived = false));


ALTER VIEW hotel.vw_booking_financial_summary OWNER TO postgres;

--
-- Name: vw_booking_profitability; Type: VIEW; Schema: hotel; Owner: postgres
--

CREATE VIEW hotel.vw_booking_profitability AS
 WITH allocated_cost AS (
         SELECT booking_cost_allocations.booking_id,
            sum(booking_cost_allocations.amount) AS allocated_cost
           FROM hotel.booking_cost_allocations
          WHERE ((booking_cost_allocations.is_active = true) AND (booking_cost_allocations.is_archived = false) AND (((booking_cost_allocations.allocation_type)::text = 'SHARED_OVERHEAD'::text) OR (((booking_cost_allocations.allocation_type)::text = 'DIRECT'::text) AND (booking_cost_allocations.expense_id IS NULL))))
          GROUP BY booking_cost_allocations.booking_id
        ), direct_expense AS (
         SELECT expenses.booking_id,
            sum(expenses.amount) AS direct_expense
           FROM hotel.expenses
          WHERE ((expenses.booking_id IS NOT NULL) AND (expenses.is_active = true) AND (expenses.is_archived = false))
          GROUP BY expenses.booking_id
        )
 SELECT f.booking_id,
    f.booking_uid,
    f.organization_id,
    f.property_id,
    f.booking_number,
    f.status,
    f.booking_source,
    f.check_in_date,
    f.check_out_date,
    f.number_of_nights,
    f.adults,
    f.children,
    f.infants,
    f.currency,
    f.room_revenue,
    f.extra_income,
    f.discount_amount,
    f.tax_amount,
    f.service_charge,
    f.total_booking_value,
    f.payments_received,
    f.refunds_paid,
    f.net_paid,
    f.outstanding_balance,
    (COALESCE(de.direct_expense, (0)::numeric))::numeric(18,2) AS direct_expense,
    (COALESCE(ac.allocated_cost, (0)::numeric))::numeric(18,2) AS allocated_overhead,
    (((f.total_booking_value - COALESCE(de.direct_expense, (0)::numeric)) - COALESCE(ac.allocated_cost, (0)::numeric)))::numeric(18,2) AS estimated_profit,
        CASE
            WHEN (((f.adults + f.children) > 0) AND (f.number_of_nights > 0)) THEN round((((f.total_booking_value - COALESCE(de.direct_expense, (0)::numeric)) - COALESCE(ac.allocated_cost, (0)::numeric)) / (((f.adults + f.children) * f.number_of_nights))::numeric), 2)
            ELSE NULL::numeric
        END AS profit_per_guest_night
   FROM ((hotel.vw_booking_financial_summary f
     LEFT JOIN direct_expense de ON ((de.booking_id = f.booking_id)))
     LEFT JOIN allocated_cost ac ON ((ac.booking_id = f.booking_id)));


ALTER VIEW hotel.vw_booking_profitability OWNER TO postgres;

--
-- Name: vw_guest_booking_history; Type: VIEW; Schema: hotel; Owner: postgres
--

CREATE VIEW hotel.vw_guest_booking_history AS
 SELECT bg.organization_id,
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
   FROM (((hotel.booking_guests bg
     JOIN hotel.guests g ON ((g.id = bg.guest_id)))
     JOIN hotel.bookings b ON ((b.id = bg.booking_id)))
     JOIN hotel.vw_booking_financial_summary f ON ((f.booking_id = b.id)))
  WHERE ((bg.is_active = true) AND (bg.is_archived = false));


ALTER VIEW hotel.vw_guest_booking_history OWNER TO postgres;

--
-- Name: vw_monthly_property_summary; Type: VIEW; Schema: hotel; Owner: postgres
--

CREATE VIEW hotel.vw_monthly_property_summary AS
 WITH months AS (
         SELECT bookings.organization_id,
            bookings.property_id,
            (date_trunc('month'::text, (bookings.check_in_date)::timestamp with time zone))::date AS report_month
           FROM hotel.bookings
          WHERE ((bookings.is_active = true) AND (bookings.is_archived = false))
        UNION
         SELECT expenses.organization_id,
            expenses.property_id,
            (date_trunc('month'::text, (expenses.expense_date)::timestamp with time zone))::date AS date_trunc
           FROM hotel.expenses
          WHERE ((expenses.is_active = true) AND (expenses.is_archived = false))
        UNION
         SELECT utility_bills.organization_id,
            utility_bills.property_id,
            (date_trunc('month'::text, (utility_bills.period_end)::timestamp with time zone))::date AS date_trunc
           FROM hotel.utility_bills
          WHERE ((utility_bills.is_active = true) AND (utility_bills.is_archived = false))
        UNION
         SELECT staff_payments.organization_id,
            staff_payments.property_id,
            (date_trunc('month'::text, staff_payments.paid_at))::date AS date_trunc
           FROM hotel.staff_payments
          WHERE ((staff_payments.is_active = true) AND (staff_payments.is_archived = false))
        UNION
         SELECT other_income.organization_id,
            other_income.property_id,
            (date_trunc('month'::text, (other_income.income_date)::timestamp with time zone))::date AS date_trunc
           FROM hotel.other_income
          WHERE ((other_income.is_active = true) AND (other_income.is_archived = false))
        ), booking_income AS (
         SELECT vw_booking_financial_summary.organization_id,
            vw_booking_financial_summary.property_id,
            (date_trunc('month'::text, (vw_booking_financial_summary.check_in_date)::timestamp with time zone))::date AS report_month,
            sum(vw_booking_financial_summary.room_revenue) AS booking_revenue,
            sum(vw_booking_financial_summary.extra_income) AS booking_extra_income,
            sum(vw_booking_financial_summary.total_booking_value) AS total_booking_income
           FROM hotel.vw_booking_financial_summary
          WHERE ((vw_booking_financial_summary.status)::text = ANY (ARRAY[('CONFIRMED'::character varying)::text, ('CHECKED_IN'::character varying)::text, ('CHECKED_OUT'::character varying)::text, ('COMPLETED'::character varying)::text]))
          GROUP BY vw_booking_financial_summary.organization_id, vw_booking_financial_summary.property_id, ((date_trunc('month'::text, (vw_booking_financial_summary.check_in_date)::timestamp with time zone))::date)
        ), general_expense AS (
         SELECT e.organization_id,
            e.property_id,
            (date_trunc('month'::text, (e.expense_date)::timestamp with time zone))::date AS report_month,
            sum(e.amount) AS general_expenses
           FROM (hotel.expenses e
             JOIN hotel.expense_categories ec ON ((ec.id = e.expense_category_id)))
          WHERE ((e.is_active = true) AND (e.is_archived = false) AND ((ec.expense_group)::text <> ALL (ARRAY[('STAFF'::character varying)::text, ('UTILITY'::character varying)::text])))
          GROUP BY e.organization_id, e.property_id, ((date_trunc('month'::text, (e.expense_date)::timestamp with time zone))::date)
        ), utility_expense AS (
         SELECT utility_bills.organization_id,
            utility_bills.property_id,
            (date_trunc('month'::text, (utility_bills.period_end)::timestamp with time zone))::date AS report_month,
            sum(utility_bills.amount) AS utility_expenses
           FROM hotel.utility_bills
          WHERE ((utility_bills.is_active = true) AND (utility_bills.is_archived = false))
          GROUP BY utility_bills.organization_id, utility_bills.property_id, ((date_trunc('month'::text, (utility_bills.period_end)::timestamp with time zone))::date)
        ), staff_expense AS (
         SELECT staff_payments.organization_id,
            staff_payments.property_id,
            (date_trunc('month'::text, staff_payments.paid_at))::date AS report_month,
            sum(staff_payments.total_paid) AS staff_expenses
           FROM hotel.staff_payments
          WHERE ((staff_payments.is_active = true) AND (staff_payments.is_archived = false))
          GROUP BY staff_payments.organization_id, staff_payments.property_id, ((date_trunc('month'::text, staff_payments.paid_at))::date)
        ), additional_income AS (
         SELECT other_income.organization_id,
            other_income.property_id,
            (date_trunc('month'::text, (other_income.income_date)::timestamp with time zone))::date AS report_month,
            sum(other_income.amount) AS other_income
           FROM hotel.other_income
          WHERE ((other_income.is_active = true) AND (other_income.is_archived = false))
          GROUP BY other_income.organization_id, other_income.property_id, ((date_trunc('month'::text, (other_income.income_date)::timestamp with time zone))::date)
        )
 SELECT m.organization_id,
    m.property_id,
    p.uid AS property_uid,
    p.name AS property_name,
    m.report_month,
    (COALESCE(bi.booking_revenue, (0)::numeric))::numeric(18,2) AS booking_revenue,
    (COALESCE(bi.booking_extra_income, (0)::numeric))::numeric(18,2) AS booking_extra_income,
    (COALESCE(ai.other_income, (0)::numeric))::numeric(18,2) AS other_income,
    ((COALESCE(bi.total_booking_income, (0)::numeric) + COALESCE(ai.other_income, (0)::numeric)))::numeric(18,2) AS total_income,
    (COALESCE(se.staff_expenses, (0)::numeric))::numeric(18,2) AS staff_expenses,
    (COALESCE(ue.utility_expenses, (0)::numeric))::numeric(18,2) AS utility_expenses,
    (COALESCE(ge.general_expenses, (0)::numeric))::numeric(18,2) AS general_expenses,
    (((COALESCE(se.staff_expenses, (0)::numeric) + COALESCE(ue.utility_expenses, (0)::numeric)) + COALESCE(ge.general_expenses, (0)::numeric)))::numeric(18,2) AS total_expenses,
    (((((COALESCE(bi.total_booking_income, (0)::numeric) + COALESCE(ai.other_income, (0)::numeric)) - COALESCE(se.staff_expenses, (0)::numeric)) - COALESCE(ue.utility_expenses, (0)::numeric)) - COALESCE(ge.general_expenses, (0)::numeric)))::numeric(18,2) AS net_profit
   FROM ((((((months m
     JOIN hotel.properties p ON ((p.id = m.property_id)))
     LEFT JOIN booking_income bi ON (((bi.organization_id = m.organization_id) AND (bi.property_id = m.property_id) AND (bi.report_month = m.report_month))))
     LEFT JOIN general_expense ge ON (((ge.organization_id = m.organization_id) AND (ge.property_id = m.property_id) AND (ge.report_month = m.report_month))))
     LEFT JOIN utility_expense ue ON (((ue.organization_id = m.organization_id) AND (ue.property_id = m.property_id) AND (ue.report_month = m.report_month))))
     LEFT JOIN staff_expense se ON (((se.organization_id = m.organization_id) AND (se.property_id = m.property_id) AND (se.report_month = m.report_month))))
     LEFT JOIN additional_income ai ON (((ai.organization_id = m.organization_id) AND (ai.property_id = m.property_id) AND (ai.report_month = m.report_month))));


ALTER VIEW hotel.vw_monthly_property_summary OWNER TO postgres;

--
-- Name: vw_monthly_utility_summary; Type: VIEW; Schema: hotel; Owner: postgres
--

CREATE VIEW hotel.vw_monthly_utility_summary AS
 SELECT ub.organization_id,
    ub.property_id,
    p.uid AS property_uid,
    (date_trunc('month'::text, (ub.period_end)::timestamp with time zone))::date AS report_month,
    ut.code AS utility_code,
    ut.name AS utility_name,
    ut.unit_of_measure,
    (sum(COALESCE(ub.units_used, (0)::numeric)))::numeric(18,3) AS total_units,
    (sum(ub.amount))::numeric(18,2) AS total_cost
   FROM ((hotel.utility_bills ub
     JOIN hotel.utility_types ut ON ((ut.id = ub.utility_type_id)))
     JOIN hotel.properties p ON ((p.id = ub.property_id)))
  WHERE ((ub.is_active = true) AND (ub.is_archived = false))
  GROUP BY ub.organization_id, ub.property_id, p.uid, ((date_trunc('month'::text, (ub.period_end)::timestamp with time zone))::date), ut.code, ut.name, ut.unit_of_measure;


ALTER VIEW hotel.vw_monthly_utility_summary OWNER TO postgres;

--
-- Name: vw_payment_method_summary; Type: VIEW; Schema: hotel; Owner: postgres
--

CREATE VIEW hotel.vw_payment_method_summary AS
 SELECT bp.organization_id,
    bp.property_id,
    p.uid AS property_uid,
    (date_trunc('month'::text, bp.paid_at))::date AS report_month,
    bp.payment_method,
    bp.currency,
    count(*) AS payment_count,
    (sum(bp.amount))::numeric(18,2) AS amount_received
   FROM (hotel.booking_payments bp
     JOIN hotel.properties p ON ((p.id = bp.property_id)))
  WHERE (((bp.status)::text = 'COMPLETED'::text) AND (bp.is_active = true) AND (bp.is_archived = false))
  GROUP BY bp.organization_id, bp.property_id, p.uid, ((date_trunc('month'::text, bp.paid_at))::date), bp.payment_method, bp.currency;


ALTER VIEW hotel.vw_payment_method_summary OWNER TO postgres;

--
-- Name: staff_property; Type: TABLE; Schema: identity; Owner: postgres
--

CREATE TABLE identity.staff_property (
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    staff_uid uuid NOT NULL,
    property_uid uuid NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    assigned_at_utc timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE identity.staff_property OWNER TO postgres;

--
-- Name: staff_refresh_token; Type: TABLE; Schema: identity; Owner: postgres
--

CREATE TABLE identity.staff_refresh_token (
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    staff_uid uuid NOT NULL,
    property_uid uuid,
    token_hash character varying(128) NOT NULL,
    expires_at_utc timestamp with time zone NOT NULL,
    revoked_at_utc timestamp with time zone,
    created_at_utc timestamp with time zone DEFAULT now() NOT NULL,
    created_ip_address character varying(100)
);


ALTER TABLE identity.staff_refresh_token OWNER TO postgres;

--
-- Name: staff_role; Type: TABLE; Schema: identity; Owner: postgres
--

CREATE TABLE identity.staff_role (
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    code character varying(100) NOT NULL,
    name character varying(150) NOT NULL,
    description character varying(500),
    is_active boolean DEFAULT true NOT NULL,
    created_at_utc timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE identity.staff_role OWNER TO postgres;

--
-- Name: staff_user; Type: TABLE; Schema: identity; Owner: postgres
--

CREATE TABLE identity.staff_user (
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    username character varying(100) NOT NULL,
    normalized_username character varying(100) NOT NULL,
    email character varying(255),
    normalized_email character varying(255),
    first_name character varying(100) NOT NULL,
    last_name character varying(100),
    password_hash character varying(1000) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_locked boolean DEFAULT false NOT NULL,
    failed_login_count integer DEFAULT 0 NOT NULL,
    locked_until_utc timestamp with time zone,
    last_login_utc timestamp with time zone,
    created_at_utc timestamp with time zone DEFAULT now() NOT NULL,
    created_by uuid,
    updated_at_utc timestamp with time zone,
    updated_by uuid,
    CONSTRAINT ck_staff_user_failed_login_count CHECK ((failed_login_count >= 0))
);


ALTER TABLE identity.staff_user OWNER TO postgres;

--
-- Name: staff_user_role; Type: TABLE; Schema: identity; Owner: postgres
--

CREATE TABLE identity.staff_user_role (
    uid uuid DEFAULT gen_random_uuid() NOT NULL,
    staff_property_uid uuid NOT NULL,
    role_uid uuid NOT NULL,
    assigned_at_utc timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE identity.staff_user_role OWNER TO postgres;

--
-- Data for Name: accommodation_types; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.accommodation_types (id, uid, organization_id, property_id, code, name, unit_kind, description, max_adults, max_children, max_occupancy, default_quantity, base_rate, sort_order, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: accommodation_units; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.accommodation_units (id, uid, organization_id, property_id, accommodation_type_id, unit_code, unit_name, floor_or_area, status, housekeeping_status, notes, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: app_users; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.app_users (id, uid, auth_subject, user_name, first_name, last_name, email, phone, status, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
1	0e274715-57a3-431f-b894-93d9933cb433	d3760743-86fa-4395-9287-b9d9b810fcb2	hoteladmin	Hotel	Admin	jananijayasuriya330@gmail.com	\N	ACTIVE	t	f	2026-09-24 08:21:37.232666+00	00000000-0000-0000-0000-000000000001	2026-09-24 08:35:51.236017+00	d3760743-86fa-4395-9287-b9d9b810fcb2
4	d02d6d1c-966d-440c-986e-e674fe73c51b	509b1725-c08e-4a41-838e-e6c0b9578f69	bhanuka.lakmal29	Hotel	Admin	bhanuka.lakmal29@gmail.com	\N	ACTIVE	t	f	2026-09-24 08:50:34.868076+00	00000000-0000-0000-0000-000000000001	2026-09-24 10:09:30.538657+00	509b1725-c08e-4a41-838e-e6c0b9578f69
\.


--
-- Data for Name: audit_logs; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.audit_logs (id, uid, organization_id, property_id, actor_subject, action, entity_name, entity_uid, old_values, new_values, ip_address, user_agent, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: booking_charge_types; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.booking_charge_types (id, uid, organization_id, property_id, code, name, category, is_taxable, default_price, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: booking_charges; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.booking_charges (id, uid, organization_id, property_id, booking_id, booking_unit_id, charge_type_id, service_date, description, quantity, unit_price, discount_amount, tax_amount, notes, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: booking_cost_allocations; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.booking_cost_allocations (id, uid, organization_id, property_id, booking_id, expense_id, allocation_month, allocation_type, calculation_basis, amount, notes, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: booking_guests; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.booking_guests (id, uid, organization_id, property_id, booking_id, booking_unit_id, guest_id, is_lead_guest, checked_in_at, checked_out_at, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: booking_payments; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.booking_payments (id, uid, organization_id, property_id, booking_id, payment_method, payment_type, amount, currency, status, reference_number, paid_at, notes, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: booking_refunds; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.booking_refunds (id, uid, organization_id, property_id, booking_id, payment_id, amount, status, reason, refunded_at, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: booking_status_history; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.booking_status_history (id, uid, organization_id, property_id, booking_id, old_status, new_status, reason, changed_at, changed_by, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: booking_units; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.booking_units (id, uid, organization_id, property_id, booking_id, accommodation_type_id, unit_id, rate_plan_id, meal_plan_id, check_in_date, check_out_date, adults, children, unit_quantity, guest_count, pricing_basis, unit_rate, discount_amount, tax_amount, total_amount, allocation_status, notes, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: bookings; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.bookings (id, uid, organization_id, property_id, booking_number, lead_guest_id, booking_source, external_reference, status, check_in_date, check_out_date, adults, children, infants, currency, discount_amount, tax_amount, service_charge, quoted_total, arrival_time, departure_time, special_requests, internal_notes, confirmed_at, checked_in_at, checked_out_at, cancelled_at, cancellation_reason, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: expense_categories; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.expense_categories (id, uid, organization_id, parent_id, code, name, expense_group, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: expenses; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.expenses (id, uid, organization_id, property_id, expense_category_id, supplier_id, booking_id, expense_date, description, amount, currency, payment_method, reference_number, receipt_url, notes, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: guest_documents; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.guest_documents (id, uid, organization_id, guest_id, document_type, document_number, issuing_country, issued_date, expiry_date, file_url, is_verified, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: guests; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.guests (id, uid, organization_id, guest_type, title, first_name, last_name, display_name, phone, alternate_phone, email, nationality_code, date_of_birth, preferred_language, address, city, country_code, notes, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: income_categories; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.income_categories (id, uid, organization_id, code, name, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: meal_plans; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.meal_plans (id, uid, organization_id, property_id, code, name, description, includes_breakfast, includes_lunch, includes_dinner, allow_byo, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: organizations; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.organizations (id, uid, code, name, legal_name, default_currency, timezone, status, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
1	1b04de12-b4ef-4eca-9027-f93b11aff3b0	ABC	ABC Hotels	ABC Hotels Pvt Ltd	LKR	Asia/Colombo	ACTIVE	t	f	2026-09-24 07:56:39.161089+00	00000000-0000-0000-0000-000000000001	\N	\N
\.


--
-- Data for Name: other_income; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.other_income (id, uid, organization_id, property_id, income_category_id, booking_id, income_date, description, amount, currency, payment_method, reference_number, notes, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: properties; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.properties (id, uid, organization_id, code, name, slug, property_type, description, address_line1, address_line2, city, district, province, postal_code, country_code, latitude, longitude, phone, email, timezone, default_currency, status, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
1	77742ab5-09f2-4a1c-89a8-ea3692def515	1	ABC-CMB	ABC Hotel Colombo	abc-hotel-colombo	HOTEL	City hotel	123 Galle Road	\N	Colombo	Colombo	Western	00300	LK	\N	\N	+94112223344	info@abchotel.com	Asia/Colombo	LKR	ACTIVE	t	f	2026-09-24 07:57:33.711193+00	00000000-0000-0000-0000-000000000001	\N	\N
\.


--
-- Data for Name: property_settings; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.property_settings (id, uid, organization_id, property_id, check_in_time, check_out_time, booking_number_prefix, invoice_number_prefix, tax_rate, service_charge_rate, allow_overbooking, extra_settings, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
1	9006a36c-6619-4ebc-9e50-216d3e252186	1	1	14:00:00	11:00:00	BKG	INV	0.0000	0.0000	f	{}	t	f	2026-09-24 07:57:33.711576+00	00000000-0000-0000-0000-000000000001	\N	\N
\.


--
-- Data for Name: rate_plan_prices; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.rate_plan_prices (id, uid, organization_id, property_id, rate_plan_id, start_date, end_date, day_of_week, adult_rate, child_rate, unit_rate, minimum_stay, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: rate_plans; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.rate_plans (id, uid, organization_id, property_id, accommodation_type_id, meal_plan_id, code, name, pricing_basis, currency, description, is_refundable, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: staff_members; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.staff_members (id, uid, organization_id, property_id, staff_role_id, employee_number, first_name, last_name, phone, email, employment_type, basic_salary, daily_rate, hourly_rate, joined_date, left_date, status, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: staff_payments; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.staff_payments (id, uid, organization_id, property_id, staff_id, period_start, period_end, basic_amount, overtime_amount, bonus_amount, deduction_amount, total_paid, payment_method, paid_at, reference_number, notes, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: staff_roles; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.staff_roles (id, uid, organization_id, code, name, description, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: staff_work_logs; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.staff_work_logs (id, uid, organization_id, property_id, staff_id, booking_id, work_date, start_time, end_time, hours_worked, overtime_hours, notes, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: suppliers; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.suppliers (id, uid, organization_id, name, phone, email, address, tax_number, notes, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: unit_blocks; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.unit_blocks (id, uid, organization_id, property_id, unit_id, start_date, end_date, block_type, reason, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: user_property_access; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.user_property_access (id, uid, user_id, organization_id, property_id, role_code, is_default_property, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
1	a6e5f042-4ef2-47a1-a8b3-431e307cbb25	1	1	1	PROPERTY_ADMIN	t	t	f	2026-09-24 08:21:37.232666+00	00000000-0000-0000-0000-000000000001	\N	\N
4	99468a54-ba4c-4b94-9275-9d4a7caaa0bc	4	1	1	PROPERTY_ADMIN	t	t	f	2026-09-24 08:50:34.868076+00	00000000-0000-0000-0000-000000000001	\N	\N
\.


--
-- Data for Name: utility_bills; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.utility_bills (id, uid, organization_id, property_id, utility_type_id, period_start, period_end, previous_reading, current_reading, units_used, amount, currency, due_date, paid_at, reference_number, notes, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: utility_types; Type: TABLE DATA; Schema: hotel; Owner: postgres
--

COPY hotel.utility_types (id, uid, organization_id, code, name, unit_of_measure, is_metered, is_active, is_archived, creation_date, created_by, modified_date, modified_by) FROM stdin;
\.


--
-- Data for Name: staff_property; Type: TABLE DATA; Schema: identity; Owner: postgres
--

COPY identity.staff_property (uid, staff_uid, property_uid, is_active, assigned_at_utc) FROM stdin;
075d6410-b950-48a8-b904-9f1deed418e6	d3760743-86fa-4395-9287-b9d9b810fcb2	77742ab5-09f2-4a1c-89a8-ea3692def515	t	2026-09-24 08:21:37.232666+00
f2e2429b-d44a-418b-8e1e-f08ed0acf05d	509b1725-c08e-4a41-838e-e6c0b9578f69	77742ab5-09f2-4a1c-89a8-ea3692def515	t	2026-09-24 08:50:34.868076+00
\.


--
-- Data for Name: staff_refresh_token; Type: TABLE DATA; Schema: identity; Owner: postgres
--

COPY identity.staff_refresh_token (uid, staff_uid, property_uid, token_hash, expires_at_utc, revoked_at_utc, created_at_utc, created_ip_address) FROM stdin;
\.


--
-- Data for Name: staff_role; Type: TABLE DATA; Schema: identity; Owner: postgres
--

COPY identity.staff_role (uid, code, name, description, is_active, created_at_utc) FROM stdin;
deb40e07-9475-48b4-97b1-b3be55c7d83a	HOTEL_ADMIN	Hotel Administrator	Full hotel administration access	t	2026-09-24 08:21:37.150558+00
\.


--
-- Data for Name: staff_user; Type: TABLE DATA; Schema: identity; Owner: postgres
--

COPY identity.staff_user (uid, username, normalized_username, email, normalized_email, first_name, last_name, password_hash, is_active, is_locked, failed_login_count, locked_until_utc, last_login_utc, created_at_utc, created_by, updated_at_utc, updated_by) FROM stdin;
d3760743-86fa-4395-9287-b9d9b810fcb2	hoteladmin	HOTELADMIN	jananijayasuriya330@gmail.com	JANANIJAYASURIYA330@GMAIL.COM	Hotel	Admin	AQAAAAIAAYagAAAAECgRsQqFnGm8lNJHDCSgh4w3cX/litpbV/FsJ1o++29qDjdDz3XfGp/Zbja1PIEFRQ==	t	f	1	\N	\N	2026-09-24 08:21:37.232666+00	00000000-0000-0000-0000-000000000001	2026-09-24 08:35:51.236017+00	\N
509b1725-c08e-4a41-838e-e6c0b9578f69	bhanuka.lakmal29	BHANUKA.LAKMAL29	bhanuka.lakmal29@gmail.com	BHANUKA.LAKMAL29@GMAIL.COM	Hotel	Admin	AQAAAAIAAYagAAAAEEJtxOdNd7GXoYWblkQXsJoW6rx2XCzdtZ89u86knpsMFSzP3+neKRpd3QOImWERtA==	t	f	0	\N	2026-09-24 08:54:40.060932+00	2026-09-24 08:50:34.868076+00	00000000-0000-0000-0000-000000000001	2026-09-24 10:09:30.538657+00	\N
\.


--
-- Data for Name: staff_user_role; Type: TABLE DATA; Schema: identity; Owner: postgres
--

COPY identity.staff_user_role (uid, staff_property_uid, role_uid, assigned_at_utc) FROM stdin;
692c4206-3ff4-431c-ae07-7c445b2ade57	075d6410-b950-48a8-b904-9f1deed418e6	deb40e07-9475-48b4-97b1-b3be55c7d83a	2026-09-24 08:21:37.232666+00
79eba519-4723-4ee6-8bf9-8b287b19442e	f2e2429b-d44a-418b-8e1e-f08ed0acf05d	deb40e07-9475-48b4-97b1-b3be55c7d83a	2026-09-24 08:50:34.868076+00
\.


--
-- Name: accommodation_types_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.accommodation_types_id_seq', 1, false);


--
-- Name: accommodation_units_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.accommodation_units_id_seq', 1, false);


--
-- Name: app_users_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.app_users_id_seq', 5, true);


--
-- Name: audit_logs_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.audit_logs_id_seq', 1, false);


--
-- Name: booking_charge_types_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.booking_charge_types_id_seq', 1, false);


--
-- Name: booking_charges_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.booking_charges_id_seq', 1, false);


--
-- Name: booking_cost_allocations_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.booking_cost_allocations_id_seq', 1, false);


--
-- Name: booking_guests_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.booking_guests_id_seq', 1, false);


--
-- Name: booking_payments_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.booking_payments_id_seq', 1, false);


--
-- Name: booking_refunds_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.booking_refunds_id_seq', 1, false);


--
-- Name: booking_status_history_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.booking_status_history_id_seq', 1, false);


--
-- Name: booking_units_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.booking_units_id_seq', 1, false);


--
-- Name: bookings_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.bookings_id_seq', 1, false);


--
-- Name: expense_categories_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.expense_categories_id_seq', 1, false);


--
-- Name: expenses_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.expenses_id_seq', 1, false);


--
-- Name: guest_documents_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.guest_documents_id_seq', 1, false);


--
-- Name: guests_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.guests_id_seq', 1, false);


--
-- Name: income_categories_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.income_categories_id_seq', 1, false);


--
-- Name: meal_plans_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.meal_plans_id_seq', 1, false);


--
-- Name: organizations_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.organizations_id_seq', 1, true);


--
-- Name: other_income_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.other_income_id_seq', 1, false);


--
-- Name: properties_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.properties_id_seq', 1, true);


--
-- Name: property_settings_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.property_settings_id_seq', 1, true);


--
-- Name: rate_plan_prices_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.rate_plan_prices_id_seq', 1, false);


--
-- Name: rate_plans_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.rate_plans_id_seq', 1, false);


--
-- Name: staff_members_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.staff_members_id_seq', 1, false);


--
-- Name: staff_payments_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.staff_payments_id_seq', 1, false);


--
-- Name: staff_roles_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.staff_roles_id_seq', 1, false);


--
-- Name: staff_work_logs_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.staff_work_logs_id_seq', 1, false);


--
-- Name: suppliers_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.suppliers_id_seq', 1, false);


--
-- Name: unit_blocks_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.unit_blocks_id_seq', 1, false);


--
-- Name: user_property_access_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.user_property_access_id_seq', 5, true);


--
-- Name: utility_bills_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.utility_bills_id_seq', 1, false);


--
-- Name: utility_types_id_seq; Type: SEQUENCE SET; Schema: hotel; Owner: postgres
--

SELECT pg_catalog.setval('hotel.utility_types_id_seq', 1, false);


--
-- Name: accommodation_types accommodation_types_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.accommodation_types
    ADD CONSTRAINT accommodation_types_pkey PRIMARY KEY (id);


--
-- Name: accommodation_units accommodation_units_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.accommodation_units
    ADD CONSTRAINT accommodation_units_pkey PRIMARY KEY (id);


--
-- Name: app_users app_users_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.app_users
    ADD CONSTRAINT app_users_pkey PRIMARY KEY (id);


--
-- Name: audit_logs audit_logs_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.audit_logs
    ADD CONSTRAINT audit_logs_pkey PRIMARY KEY (id);


--
-- Name: booking_charge_types booking_charge_types_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_charge_types
    ADD CONSTRAINT booking_charge_types_pkey PRIMARY KEY (id);


--
-- Name: booking_charges booking_charges_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_charges
    ADD CONSTRAINT booking_charges_pkey PRIMARY KEY (id);


--
-- Name: booking_cost_allocations booking_cost_allocations_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_cost_allocations
    ADD CONSTRAINT booking_cost_allocations_pkey PRIMARY KEY (id);


--
-- Name: booking_guests booking_guests_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_guests
    ADD CONSTRAINT booking_guests_pkey PRIMARY KEY (id);


--
-- Name: booking_payments booking_payments_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_payments
    ADD CONSTRAINT booking_payments_pkey PRIMARY KEY (id);


--
-- Name: booking_refunds booking_refunds_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_refunds
    ADD CONSTRAINT booking_refunds_pkey PRIMARY KEY (id);


--
-- Name: booking_status_history booking_status_history_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_status_history
    ADD CONSTRAINT booking_status_history_pkey PRIMARY KEY (id);


--
-- Name: booking_units booking_units_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_units
    ADD CONSTRAINT booking_units_pkey PRIMARY KEY (id);


--
-- Name: bookings bookings_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.bookings
    ADD CONSTRAINT bookings_pkey PRIMARY KEY (id);


--
-- Name: booking_units ex_booking_units_no_overlap; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_units
    ADD CONSTRAINT ex_booking_units_no_overlap EXCLUDE USING gist (unit_id WITH =, daterange(check_in_date, check_out_date, '[)'::text) WITH &&) WHERE (((unit_id IS NOT NULL) AND ((allocation_status)::text = ANY (ARRAY[('HELD'::character varying)::text, ('CONFIRMED'::character varying)::text, ('CHECKED_IN'::character varying)::text])) AND (is_archived = false)));


--
-- Name: expense_categories expense_categories_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expense_categories
    ADD CONSTRAINT expense_categories_pkey PRIMARY KEY (id);


--
-- Name: expenses expenses_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expenses
    ADD CONSTRAINT expenses_pkey PRIMARY KEY (id);


--
-- Name: guest_documents guest_documents_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.guest_documents
    ADD CONSTRAINT guest_documents_pkey PRIMARY KEY (id);


--
-- Name: guests guests_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.guests
    ADD CONSTRAINT guests_pkey PRIMARY KEY (id);


--
-- Name: income_categories income_categories_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.income_categories
    ADD CONSTRAINT income_categories_pkey PRIMARY KEY (id);


--
-- Name: meal_plans meal_plans_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.meal_plans
    ADD CONSTRAINT meal_plans_pkey PRIMARY KEY (id);


--
-- Name: organizations organizations_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.organizations
    ADD CONSTRAINT organizations_pkey PRIMARY KEY (id);


--
-- Name: other_income other_income_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.other_income
    ADD CONSTRAINT other_income_pkey PRIMARY KEY (id);


--
-- Name: properties properties_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.properties
    ADD CONSTRAINT properties_pkey PRIMARY KEY (id);


--
-- Name: property_settings property_settings_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.property_settings
    ADD CONSTRAINT property_settings_pkey PRIMARY KEY (id);


--
-- Name: rate_plan_prices rate_plan_prices_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.rate_plan_prices
    ADD CONSTRAINT rate_plan_prices_pkey PRIMARY KEY (id);


--
-- Name: rate_plans rate_plans_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.rate_plans
    ADD CONSTRAINT rate_plans_pkey PRIMARY KEY (id);


--
-- Name: staff_members staff_members_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_members
    ADD CONSTRAINT staff_members_pkey PRIMARY KEY (id);


--
-- Name: staff_payments staff_payments_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_payments
    ADD CONSTRAINT staff_payments_pkey PRIMARY KEY (id);


--
-- Name: staff_roles staff_roles_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_roles
    ADD CONSTRAINT staff_roles_pkey PRIMARY KEY (id);


--
-- Name: staff_work_logs staff_work_logs_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_work_logs
    ADD CONSTRAINT staff_work_logs_pkey PRIMARY KEY (id);


--
-- Name: suppliers suppliers_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.suppliers
    ADD CONSTRAINT suppliers_pkey PRIMARY KEY (id);


--
-- Name: unit_blocks unit_blocks_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.unit_blocks
    ADD CONSTRAINT unit_blocks_pkey PRIMARY KEY (id);


--
-- Name: accommodation_types uq_accommodation_types_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.accommodation_types
    ADD CONSTRAINT uq_accommodation_types_org_id UNIQUE (id, organization_id);


--
-- Name: accommodation_types uq_accommodation_types_property_code; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.accommodation_types
    ADD CONSTRAINT uq_accommodation_types_property_code UNIQUE (property_id, code);


--
-- Name: accommodation_types uq_accommodation_types_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.accommodation_types
    ADD CONSTRAINT uq_accommodation_types_uid UNIQUE (uid);


--
-- Name: accommodation_units uq_accommodation_units_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.accommodation_units
    ADD CONSTRAINT uq_accommodation_units_org_id UNIQUE (id, organization_id);


--
-- Name: accommodation_units uq_accommodation_units_property_code; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.accommodation_units
    ADD CONSTRAINT uq_accommodation_units_property_code UNIQUE (property_id, unit_code);


--
-- Name: accommodation_units uq_accommodation_units_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.accommodation_units
    ADD CONSTRAINT uq_accommodation_units_uid UNIQUE (uid);


--
-- Name: app_users uq_app_users_auth_subject; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.app_users
    ADD CONSTRAINT uq_app_users_auth_subject UNIQUE (auth_subject);


--
-- Name: app_users uq_app_users_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.app_users
    ADD CONSTRAINT uq_app_users_uid UNIQUE (uid);


--
-- Name: audit_logs uq_audit_logs_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.audit_logs
    ADD CONSTRAINT uq_audit_logs_uid UNIQUE (uid);


--
-- Name: booking_charge_types uq_booking_charge_types_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_charge_types
    ADD CONSTRAINT uq_booking_charge_types_org_id UNIQUE (id, organization_id);


--
-- Name: booking_charge_types uq_booking_charge_types_property_code; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_charge_types
    ADD CONSTRAINT uq_booking_charge_types_property_code UNIQUE (property_id, code);


--
-- Name: booking_charge_types uq_booking_charge_types_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_charge_types
    ADD CONSTRAINT uq_booking_charge_types_uid UNIQUE (uid);


--
-- Name: booking_charges uq_booking_charges_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_charges
    ADD CONSTRAINT uq_booking_charges_uid UNIQUE (uid);


--
-- Name: booking_cost_allocations uq_booking_cost_allocations_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_cost_allocations
    ADD CONSTRAINT uq_booking_cost_allocations_uid UNIQUE (uid);


--
-- Name: booking_guests uq_booking_guests_booking_guest; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_guests
    ADD CONSTRAINT uq_booking_guests_booking_guest UNIQUE (booking_id, guest_id);


--
-- Name: booking_guests uq_booking_guests_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_guests
    ADD CONSTRAINT uq_booking_guests_uid UNIQUE (uid);


--
-- Name: booking_payments uq_booking_payments_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_payments
    ADD CONSTRAINT uq_booking_payments_org_id UNIQUE (id, organization_id);


--
-- Name: booking_payments uq_booking_payments_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_payments
    ADD CONSTRAINT uq_booking_payments_uid UNIQUE (uid);


--
-- Name: booking_refunds uq_booking_refunds_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_refunds
    ADD CONSTRAINT uq_booking_refunds_uid UNIQUE (uid);


--
-- Name: booking_status_history uq_booking_status_history_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_status_history
    ADD CONSTRAINT uq_booking_status_history_uid UNIQUE (uid);


--
-- Name: booking_units uq_booking_units_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_units
    ADD CONSTRAINT uq_booking_units_org_id UNIQUE (id, organization_id);


--
-- Name: booking_units uq_booking_units_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_units
    ADD CONSTRAINT uq_booking_units_uid UNIQUE (uid);


--
-- Name: bookings uq_bookings_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.bookings
    ADD CONSTRAINT uq_bookings_org_id UNIQUE (id, organization_id);


--
-- Name: bookings uq_bookings_property_number; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.bookings
    ADD CONSTRAINT uq_bookings_property_number UNIQUE (property_id, booking_number);


--
-- Name: bookings uq_bookings_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.bookings
    ADD CONSTRAINT uq_bookings_uid UNIQUE (uid);


--
-- Name: expense_categories uq_expense_categories_org_code; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expense_categories
    ADD CONSTRAINT uq_expense_categories_org_code UNIQUE (organization_id, code);


--
-- Name: expense_categories uq_expense_categories_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expense_categories
    ADD CONSTRAINT uq_expense_categories_org_id UNIQUE (id, organization_id);


--
-- Name: expense_categories uq_expense_categories_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expense_categories
    ADD CONSTRAINT uq_expense_categories_uid UNIQUE (uid);


--
-- Name: expenses uq_expenses_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expenses
    ADD CONSTRAINT uq_expenses_org_id UNIQUE (id, organization_id);


--
-- Name: expenses uq_expenses_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expenses
    ADD CONSTRAINT uq_expenses_uid UNIQUE (uid);


--
-- Name: guest_documents uq_guest_documents_number; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.guest_documents
    ADD CONSTRAINT uq_guest_documents_number UNIQUE (organization_id, document_type, document_number);


--
-- Name: guest_documents uq_guest_documents_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.guest_documents
    ADD CONSTRAINT uq_guest_documents_uid UNIQUE (uid);


--
-- Name: guests uq_guests_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.guests
    ADD CONSTRAINT uq_guests_org_id UNIQUE (id, organization_id);


--
-- Name: guests uq_guests_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.guests
    ADD CONSTRAINT uq_guests_uid UNIQUE (uid);


--
-- Name: income_categories uq_income_categories_org_code; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.income_categories
    ADD CONSTRAINT uq_income_categories_org_code UNIQUE (organization_id, code);


--
-- Name: income_categories uq_income_categories_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.income_categories
    ADD CONSTRAINT uq_income_categories_org_id UNIQUE (id, organization_id);


--
-- Name: income_categories uq_income_categories_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.income_categories
    ADD CONSTRAINT uq_income_categories_uid UNIQUE (uid);


--
-- Name: meal_plans uq_meal_plans_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.meal_plans
    ADD CONSTRAINT uq_meal_plans_org_id UNIQUE (id, organization_id);


--
-- Name: meal_plans uq_meal_plans_property_code; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.meal_plans
    ADD CONSTRAINT uq_meal_plans_property_code UNIQUE (property_id, code);


--
-- Name: meal_plans uq_meal_plans_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.meal_plans
    ADD CONSTRAINT uq_meal_plans_uid UNIQUE (uid);


--
-- Name: organizations uq_organizations_code; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.organizations
    ADD CONSTRAINT uq_organizations_code UNIQUE (code);


--
-- Name: organizations uq_organizations_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.organizations
    ADD CONSTRAINT uq_organizations_uid UNIQUE (uid);


--
-- Name: other_income uq_other_income_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.other_income
    ADD CONSTRAINT uq_other_income_uid UNIQUE (uid);


--
-- Name: properties uq_properties_org_code; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.properties
    ADD CONSTRAINT uq_properties_org_code UNIQUE (organization_id, code);


--
-- Name: properties uq_properties_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.properties
    ADD CONSTRAINT uq_properties_org_id UNIQUE (id, organization_id);


--
-- Name: properties uq_properties_slug; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.properties
    ADD CONSTRAINT uq_properties_slug UNIQUE (slug);


--
-- Name: properties uq_properties_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.properties
    ADD CONSTRAINT uq_properties_uid UNIQUE (uid);


--
-- Name: property_settings uq_property_settings_property; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.property_settings
    ADD CONSTRAINT uq_property_settings_property UNIQUE (property_id);


--
-- Name: property_settings uq_property_settings_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.property_settings
    ADD CONSTRAINT uq_property_settings_uid UNIQUE (uid);


--
-- Name: rate_plan_prices uq_rate_plan_prices_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.rate_plan_prices
    ADD CONSTRAINT uq_rate_plan_prices_uid UNIQUE (uid);


--
-- Name: rate_plans uq_rate_plans_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.rate_plans
    ADD CONSTRAINT uq_rate_plans_org_id UNIQUE (id, organization_id);


--
-- Name: rate_plans uq_rate_plans_property_code; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.rate_plans
    ADD CONSTRAINT uq_rate_plans_property_code UNIQUE (property_id, code);


--
-- Name: rate_plans uq_rate_plans_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.rate_plans
    ADD CONSTRAINT uq_rate_plans_uid UNIQUE (uid);


--
-- Name: staff_members uq_staff_members_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_members
    ADD CONSTRAINT uq_staff_members_org_id UNIQUE (id, organization_id);


--
-- Name: staff_members uq_staff_members_property_number; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_members
    ADD CONSTRAINT uq_staff_members_property_number UNIQUE (property_id, employee_number);


--
-- Name: staff_members uq_staff_members_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_members
    ADD CONSTRAINT uq_staff_members_uid UNIQUE (uid);


--
-- Name: staff_payments uq_staff_payments_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_payments
    ADD CONSTRAINT uq_staff_payments_uid UNIQUE (uid);


--
-- Name: staff_roles uq_staff_roles_org_code; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_roles
    ADD CONSTRAINT uq_staff_roles_org_code UNIQUE (organization_id, code);


--
-- Name: staff_roles uq_staff_roles_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_roles
    ADD CONSTRAINT uq_staff_roles_org_id UNIQUE (id, organization_id);


--
-- Name: staff_roles uq_staff_roles_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_roles
    ADD CONSTRAINT uq_staff_roles_uid UNIQUE (uid);


--
-- Name: staff_work_logs uq_staff_work_logs_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_work_logs
    ADD CONSTRAINT uq_staff_work_logs_uid UNIQUE (uid);


--
-- Name: suppliers uq_suppliers_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.suppliers
    ADD CONSTRAINT uq_suppliers_org_id UNIQUE (id, organization_id);


--
-- Name: suppliers uq_suppliers_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.suppliers
    ADD CONSTRAINT uq_suppliers_uid UNIQUE (uid);


--
-- Name: unit_blocks uq_unit_blocks_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.unit_blocks
    ADD CONSTRAINT uq_unit_blocks_uid UNIQUE (uid);


--
-- Name: user_property_access uq_user_property_access_assignment; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.user_property_access
    ADD CONSTRAINT uq_user_property_access_assignment UNIQUE NULLS NOT DISTINCT (user_id, organization_id, property_id, role_code);


--
-- Name: user_property_access uq_user_property_access_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.user_property_access
    ADD CONSTRAINT uq_user_property_access_uid UNIQUE (uid);


--
-- Name: utility_bills uq_utility_bills_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.utility_bills
    ADD CONSTRAINT uq_utility_bills_uid UNIQUE (uid);


--
-- Name: utility_types uq_utility_types_org_code; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.utility_types
    ADD CONSTRAINT uq_utility_types_org_code UNIQUE (organization_id, code);


--
-- Name: utility_types uq_utility_types_org_id; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.utility_types
    ADD CONSTRAINT uq_utility_types_org_id UNIQUE (id, organization_id);


--
-- Name: utility_types uq_utility_types_uid; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.utility_types
    ADD CONSTRAINT uq_utility_types_uid UNIQUE (uid);


--
-- Name: user_property_access user_property_access_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.user_property_access
    ADD CONSTRAINT user_property_access_pkey PRIMARY KEY (id);


--
-- Name: utility_bills utility_bills_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.utility_bills
    ADD CONSTRAINT utility_bills_pkey PRIMARY KEY (id);


--
-- Name: utility_types utility_types_pkey; Type: CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.utility_types
    ADD CONSTRAINT utility_types_pkey PRIMARY KEY (id);


--
-- Name: staff_property staff_property_pkey; Type: CONSTRAINT; Schema: identity; Owner: postgres
--

ALTER TABLE ONLY identity.staff_property
    ADD CONSTRAINT staff_property_pkey PRIMARY KEY (uid);


--
-- Name: staff_refresh_token staff_refresh_token_pkey; Type: CONSTRAINT; Schema: identity; Owner: postgres
--

ALTER TABLE ONLY identity.staff_refresh_token
    ADD CONSTRAINT staff_refresh_token_pkey PRIMARY KEY (uid);


--
-- Name: staff_role staff_role_pkey; Type: CONSTRAINT; Schema: identity; Owner: postgres
--

ALTER TABLE ONLY identity.staff_role
    ADD CONSTRAINT staff_role_pkey PRIMARY KEY (uid);


--
-- Name: staff_user staff_user_pkey; Type: CONSTRAINT; Schema: identity; Owner: postgres
--

ALTER TABLE ONLY identity.staff_user
    ADD CONSTRAINT staff_user_pkey PRIMARY KEY (uid);


--
-- Name: staff_user_role staff_user_role_pkey; Type: CONSTRAINT; Schema: identity; Owner: postgres
--

ALTER TABLE ONLY identity.staff_user_role
    ADD CONSTRAINT staff_user_role_pkey PRIMARY KEY (uid);


--
-- Name: staff_property uq_staff_property; Type: CONSTRAINT; Schema: identity; Owner: postgres
--

ALTER TABLE ONLY identity.staff_property
    ADD CONSTRAINT uq_staff_property UNIQUE (staff_uid, property_uid);


--
-- Name: staff_role uq_staff_role_code; Type: CONSTRAINT; Schema: identity; Owner: postgres
--

ALTER TABLE ONLY identity.staff_role
    ADD CONSTRAINT uq_staff_role_code UNIQUE (code);


--
-- Name: staff_user uq_staff_user_normalized_username; Type: CONSTRAINT; Schema: identity; Owner: postgres
--

ALTER TABLE ONLY identity.staff_user
    ADD CONSTRAINT uq_staff_user_normalized_username UNIQUE (normalized_username);


--
-- Name: staff_user_role uq_staff_user_role; Type: CONSTRAINT; Schema: identity; Owner: postgres
--

ALTER TABLE ONLY identity.staff_user_role
    ADD CONSTRAINT uq_staff_user_role UNIQUE (staff_property_uid, role_uid);


--
-- Name: ix_accommodation_types_property; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_accommodation_types_property ON hotel.accommodation_types USING btree (organization_id, property_id) WHERE ((is_active = true) AND (is_archived = false));


--
-- Name: ix_accommodation_units_property_status; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_accommodation_units_property_status ON hotel.accommodation_units USING btree (organization_id, property_id, status) WHERE (is_archived = false);


--
-- Name: ix_audit_logs_entity; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_audit_logs_entity ON hotel.audit_logs USING btree (organization_id, entity_name, entity_uid, creation_date DESC);


--
-- Name: ix_booking_charges_booking; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_booking_charges_booking ON hotel.booking_charges USING btree (booking_id, service_date) WHERE (is_archived = false);


--
-- Name: ix_booking_guests_guest; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_booking_guests_guest ON hotel.booking_guests USING btree (organization_id, guest_id) WHERE (is_archived = false);


--
-- Name: ix_booking_payments_booking; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_booking_payments_booking ON hotel.booking_payments USING btree (booking_id, paid_at) WHERE (is_archived = false);


--
-- Name: ix_booking_units_booking; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_booking_units_booking ON hotel.booking_units USING btree (booking_id) WHERE (is_archived = false);


--
-- Name: ix_booking_units_unit_dates; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_booking_units_unit_dates ON hotel.booking_units USING btree (unit_id, check_in_date, check_out_date) WHERE ((unit_id IS NOT NULL) AND (is_archived = false));


--
-- Name: ix_bookings_lead_guest; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_bookings_lead_guest ON hotel.bookings USING btree (organization_id, lead_guest_id) WHERE ((lead_guest_id IS NOT NULL) AND (is_archived = false));


--
-- Name: ix_bookings_property_dates; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_bookings_property_dates ON hotel.bookings USING btree (organization_id, property_id, check_in_date, check_out_date) WHERE (is_archived = false);


--
-- Name: ix_bookings_property_status; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_bookings_property_status ON hotel.bookings USING btree (organization_id, property_id, status) WHERE (is_archived = false);


--
-- Name: ix_expenses_property_date; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_expenses_property_date ON hotel.expenses USING btree (organization_id, property_id, expense_date) WHERE (is_archived = false);


--
-- Name: ix_guests_phone; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_guests_phone ON hotel.guests USING btree (organization_id, phone) WHERE ((phone IS NOT NULL) AND (is_archived = false));


--
-- Name: ix_guests_search_name; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_guests_search_name ON hotel.guests USING btree (organization_id, lower((display_name)::text)) WHERE (is_archived = false);


--
-- Name: ix_properties_organization; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_properties_organization ON hotel.properties USING btree (organization_id) WHERE (is_archived = false);


--
-- Name: ix_rate_plan_prices_dates; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_rate_plan_prices_dates ON hotel.rate_plan_prices USING btree (rate_plan_id, start_date, end_date) WHERE ((is_active = true) AND (is_archived = false));


--
-- Name: ix_staff_payments_property_period; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_staff_payments_property_period ON hotel.staff_payments USING btree (organization_id, property_id, period_start, period_end) WHERE (is_archived = false);


--
-- Name: ix_unit_blocks_dates; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_unit_blocks_dates ON hotel.unit_blocks USING btree (unit_id, start_date, end_date) WHERE ((is_active = true) AND (is_archived = false));


--
-- Name: ix_user_property_access_user; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_user_property_access_user ON hotel.user_property_access USING btree (user_id, organization_id, property_id) WHERE ((is_active = true) AND (is_archived = false));


--
-- Name: ix_utility_bills_property_period; Type: INDEX; Schema: hotel; Owner: postgres
--

CREATE INDEX ix_utility_bills_property_period ON hotel.utility_bills USING btree (organization_id, property_id, period_start, period_end) WHERE (is_archived = false);


--
-- Name: ix_staff_property_property_uid; Type: INDEX; Schema: identity; Owner: postgres
--

CREATE INDEX ix_staff_property_property_uid ON identity.staff_property USING btree (property_uid);


--
-- Name: ix_staff_property_staff_uid; Type: INDEX; Schema: identity; Owner: postgres
--

CREATE INDEX ix_staff_property_staff_uid ON identity.staff_property USING btree (staff_uid);


--
-- Name: ix_staff_refresh_token_staff_uid; Type: INDEX; Schema: identity; Owner: postgres
--

CREATE INDEX ix_staff_refresh_token_staff_uid ON identity.staff_refresh_token USING btree (staff_uid);


--
-- Name: ix_staff_refresh_token_token_hash; Type: INDEX; Schema: identity; Owner: postgres
--

CREATE INDEX ix_staff_refresh_token_token_hash ON identity.staff_refresh_token USING btree (token_hash);


--
-- Name: ux_staff_refresh_token_hash; Type: INDEX; Schema: identity; Owner: postgres
--

CREATE UNIQUE INDEX ux_staff_refresh_token_hash ON identity.staff_refresh_token USING btree (token_hash);


--
-- Name: ux_staff_user_normalized_email; Type: INDEX; Schema: identity; Owner: postgres
--

CREATE UNIQUE INDEX ux_staff_user_normalized_email ON identity.staff_user USING btree (normalized_email) WHERE (normalized_email IS NOT NULL);


--
-- Name: accommodation_types trg_accommodation_types_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_accommodation_types_modified_date BEFORE UPDATE ON hotel.accommodation_types FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: accommodation_units trg_accommodation_units_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_accommodation_units_modified_date BEFORE UPDATE ON hotel.accommodation_units FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: app_users trg_app_users_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_app_users_modified_date BEFORE UPDATE ON hotel.app_users FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: audit_logs trg_audit_logs_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_audit_logs_modified_date BEFORE UPDATE ON hotel.audit_logs FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: booking_charge_types trg_booking_charge_types_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_booking_charge_types_modified_date BEFORE UPDATE ON hotel.booking_charge_types FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: booking_charges trg_booking_charges_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_booking_charges_modified_date BEFORE UPDATE ON hotel.booking_charges FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: booking_cost_allocations trg_booking_cost_allocations_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_booking_cost_allocations_modified_date BEFORE UPDATE ON hotel.booking_cost_allocations FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: booking_guests trg_booking_guests_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_booking_guests_modified_date BEFORE UPDATE ON hotel.booking_guests FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: booking_payments trg_booking_payments_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_booking_payments_modified_date BEFORE UPDATE ON hotel.booking_payments FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: booking_refunds trg_booking_refunds_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_booking_refunds_modified_date BEFORE UPDATE ON hotel.booking_refunds FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: booking_status_history trg_booking_status_history_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_booking_status_history_modified_date BEFORE UPDATE ON hotel.booking_status_history FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: booking_units trg_booking_units_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_booking_units_modified_date BEFORE UPDATE ON hotel.booking_units FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: bookings trg_bookings_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_bookings_modified_date BEFORE UPDATE ON hotel.bookings FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: expense_categories trg_expense_categories_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_expense_categories_modified_date BEFORE UPDATE ON hotel.expense_categories FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: expenses trg_expenses_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_expenses_modified_date BEFORE UPDATE ON hotel.expenses FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: guest_documents trg_guest_documents_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_guest_documents_modified_date BEFORE UPDATE ON hotel.guest_documents FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: guests trg_guests_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_guests_modified_date BEFORE UPDATE ON hotel.guests FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: income_categories trg_income_categories_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_income_categories_modified_date BEFORE UPDATE ON hotel.income_categories FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: meal_plans trg_meal_plans_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_meal_plans_modified_date BEFORE UPDATE ON hotel.meal_plans FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: organizations trg_organizations_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_organizations_modified_date BEFORE UPDATE ON hotel.organizations FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: other_income trg_other_income_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_other_income_modified_date BEFORE UPDATE ON hotel.other_income FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: properties trg_properties_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_properties_modified_date BEFORE UPDATE ON hotel.properties FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: property_settings trg_property_settings_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_property_settings_modified_date BEFORE UPDATE ON hotel.property_settings FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: rate_plan_prices trg_rate_plan_prices_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_rate_plan_prices_modified_date BEFORE UPDATE ON hotel.rate_plan_prices FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: rate_plans trg_rate_plans_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_rate_plans_modified_date BEFORE UPDATE ON hotel.rate_plans FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: staff_members trg_staff_members_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_staff_members_modified_date BEFORE UPDATE ON hotel.staff_members FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: staff_payments trg_staff_payments_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_staff_payments_modified_date BEFORE UPDATE ON hotel.staff_payments FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: staff_roles trg_staff_roles_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_staff_roles_modified_date BEFORE UPDATE ON hotel.staff_roles FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: staff_work_logs trg_staff_work_logs_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_staff_work_logs_modified_date BEFORE UPDATE ON hotel.staff_work_logs FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: suppliers trg_suppliers_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_suppliers_modified_date BEFORE UPDATE ON hotel.suppliers FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: unit_blocks trg_unit_blocks_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_unit_blocks_modified_date BEFORE UPDATE ON hotel.unit_blocks FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: user_property_access trg_user_property_access_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_user_property_access_modified_date BEFORE UPDATE ON hotel.user_property_access FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: utility_bills trg_utility_bills_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_utility_bills_modified_date BEFORE UPDATE ON hotel.utility_bills FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: utility_types trg_utility_types_modified_date; Type: TRIGGER; Schema: hotel; Owner: postgres
--

CREATE TRIGGER trg_utility_types_modified_date BEFORE UPDATE ON hotel.utility_types FOR EACH ROW EXECUTE FUNCTION hotel.set_modified_date();


--
-- Name: accommodation_types fk_accommodation_types_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.accommodation_types
    ADD CONSTRAINT fk_accommodation_types_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: accommodation_units fk_accommodation_units_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.accommodation_units
    ADD CONSTRAINT fk_accommodation_units_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: accommodation_units fk_accommodation_units_type; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.accommodation_units
    ADD CONSTRAINT fk_accommodation_units_type FOREIGN KEY (accommodation_type_id, organization_id) REFERENCES hotel.accommodation_types(id, organization_id);


--
-- Name: audit_logs fk_audit_logs_org; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.audit_logs
    ADD CONSTRAINT fk_audit_logs_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id);


--
-- Name: audit_logs fk_audit_logs_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.audit_logs
    ADD CONSTRAINT fk_audit_logs_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: booking_charge_types fk_booking_charge_types_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_charge_types
    ADD CONSTRAINT fk_booking_charge_types_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: booking_charges fk_booking_charges_booking; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_charges
    ADD CONSTRAINT fk_booking_charges_booking FOREIGN KEY (booking_id, organization_id) REFERENCES hotel.bookings(id, organization_id);


--
-- Name: booking_charges fk_booking_charges_booking_unit; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_charges
    ADD CONSTRAINT fk_booking_charges_booking_unit FOREIGN KEY (booking_unit_id, organization_id) REFERENCES hotel.booking_units(id, organization_id);


--
-- Name: booking_charges fk_booking_charges_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_charges
    ADD CONSTRAINT fk_booking_charges_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: booking_charges fk_booking_charges_type; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_charges
    ADD CONSTRAINT fk_booking_charges_type FOREIGN KEY (charge_type_id, organization_id) REFERENCES hotel.booking_charge_types(id, organization_id);


--
-- Name: booking_cost_allocations fk_booking_cost_allocations_booking; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_cost_allocations
    ADD CONSTRAINT fk_booking_cost_allocations_booking FOREIGN KEY (booking_id, organization_id) REFERENCES hotel.bookings(id, organization_id);


--
-- Name: booking_cost_allocations fk_booking_cost_allocations_expense; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_cost_allocations
    ADD CONSTRAINT fk_booking_cost_allocations_expense FOREIGN KEY (expense_id, organization_id) REFERENCES hotel.expenses(id, organization_id);


--
-- Name: booking_cost_allocations fk_booking_cost_allocations_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_cost_allocations
    ADD CONSTRAINT fk_booking_cost_allocations_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: booking_guests fk_booking_guests_booking; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_guests
    ADD CONSTRAINT fk_booking_guests_booking FOREIGN KEY (booking_id, organization_id) REFERENCES hotel.bookings(id, organization_id);


--
-- Name: booking_guests fk_booking_guests_booking_unit; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_guests
    ADD CONSTRAINT fk_booking_guests_booking_unit FOREIGN KEY (booking_unit_id, organization_id) REFERENCES hotel.booking_units(id, organization_id);


--
-- Name: booking_guests fk_booking_guests_guest; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_guests
    ADD CONSTRAINT fk_booking_guests_guest FOREIGN KEY (guest_id, organization_id) REFERENCES hotel.guests(id, organization_id);


--
-- Name: booking_guests fk_booking_guests_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_guests
    ADD CONSTRAINT fk_booking_guests_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: booking_payments fk_booking_payments_booking; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_payments
    ADD CONSTRAINT fk_booking_payments_booking FOREIGN KEY (booking_id, organization_id) REFERENCES hotel.bookings(id, organization_id);


--
-- Name: booking_payments fk_booking_payments_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_payments
    ADD CONSTRAINT fk_booking_payments_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: booking_refunds fk_booking_refunds_booking; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_refunds
    ADD CONSTRAINT fk_booking_refunds_booking FOREIGN KEY (booking_id, organization_id) REFERENCES hotel.bookings(id, organization_id);


--
-- Name: booking_refunds fk_booking_refunds_payment; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_refunds
    ADD CONSTRAINT fk_booking_refunds_payment FOREIGN KEY (payment_id, organization_id) REFERENCES hotel.booking_payments(id, organization_id);


--
-- Name: booking_refunds fk_booking_refunds_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_refunds
    ADD CONSTRAINT fk_booking_refunds_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: booking_status_history fk_booking_status_history_booking; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_status_history
    ADD CONSTRAINT fk_booking_status_history_booking FOREIGN KEY (booking_id, organization_id) REFERENCES hotel.bookings(id, organization_id);


--
-- Name: booking_status_history fk_booking_status_history_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_status_history
    ADD CONSTRAINT fk_booking_status_history_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: booking_units fk_booking_units_booking; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_units
    ADD CONSTRAINT fk_booking_units_booking FOREIGN KEY (booking_id, organization_id) REFERENCES hotel.bookings(id, organization_id);


--
-- Name: booking_units fk_booking_units_meal; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_units
    ADD CONSTRAINT fk_booking_units_meal FOREIGN KEY (meal_plan_id, organization_id) REFERENCES hotel.meal_plans(id, organization_id);


--
-- Name: booking_units fk_booking_units_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_units
    ADD CONSTRAINT fk_booking_units_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: booking_units fk_booking_units_rate; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_units
    ADD CONSTRAINT fk_booking_units_rate FOREIGN KEY (rate_plan_id, organization_id) REFERENCES hotel.rate_plans(id, organization_id);


--
-- Name: booking_units fk_booking_units_type; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_units
    ADD CONSTRAINT fk_booking_units_type FOREIGN KEY (accommodation_type_id, organization_id) REFERENCES hotel.accommodation_types(id, organization_id);


--
-- Name: booking_units fk_booking_units_unit; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.booking_units
    ADD CONSTRAINT fk_booking_units_unit FOREIGN KEY (unit_id, organization_id) REFERENCES hotel.accommodation_units(id, organization_id);


--
-- Name: bookings fk_bookings_lead_guest; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.bookings
    ADD CONSTRAINT fk_bookings_lead_guest FOREIGN KEY (lead_guest_id, organization_id) REFERENCES hotel.guests(id, organization_id);


--
-- Name: bookings fk_bookings_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.bookings
    ADD CONSTRAINT fk_bookings_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: expense_categories fk_expense_categories_org; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expense_categories
    ADD CONSTRAINT fk_expense_categories_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id);


--
-- Name: expense_categories fk_expense_categories_parent; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expense_categories
    ADD CONSTRAINT fk_expense_categories_parent FOREIGN KEY (parent_id) REFERENCES hotel.expense_categories(id);


--
-- Name: expenses fk_expenses_booking; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expenses
    ADD CONSTRAINT fk_expenses_booking FOREIGN KEY (booking_id, organization_id) REFERENCES hotel.bookings(id, organization_id);


--
-- Name: expenses fk_expenses_category; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expenses
    ADD CONSTRAINT fk_expenses_category FOREIGN KEY (expense_category_id, organization_id) REFERENCES hotel.expense_categories(id, organization_id);


--
-- Name: expenses fk_expenses_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expenses
    ADD CONSTRAINT fk_expenses_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: expenses fk_expenses_supplier; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.expenses
    ADD CONSTRAINT fk_expenses_supplier FOREIGN KEY (supplier_id, organization_id) REFERENCES hotel.suppliers(id, organization_id);


--
-- Name: guest_documents fk_guest_documents_guest; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.guest_documents
    ADD CONSTRAINT fk_guest_documents_guest FOREIGN KEY (guest_id, organization_id) REFERENCES hotel.guests(id, organization_id);


--
-- Name: guests fk_guests_organization; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.guests
    ADD CONSTRAINT fk_guests_organization FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id);


--
-- Name: income_categories fk_income_categories_org; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.income_categories
    ADD CONSTRAINT fk_income_categories_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id);


--
-- Name: meal_plans fk_meal_plans_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.meal_plans
    ADD CONSTRAINT fk_meal_plans_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: other_income fk_other_income_booking; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.other_income
    ADD CONSTRAINT fk_other_income_booking FOREIGN KEY (booking_id, organization_id) REFERENCES hotel.bookings(id, organization_id);


--
-- Name: other_income fk_other_income_category; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.other_income
    ADD CONSTRAINT fk_other_income_category FOREIGN KEY (income_category_id, organization_id) REFERENCES hotel.income_categories(id, organization_id);


--
-- Name: other_income fk_other_income_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.other_income
    ADD CONSTRAINT fk_other_income_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: properties fk_properties_organization; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.properties
    ADD CONSTRAINT fk_properties_organization FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id);


--
-- Name: property_settings fk_property_settings_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.property_settings
    ADD CONSTRAINT fk_property_settings_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: rate_plan_prices fk_rate_plan_prices_plan; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.rate_plan_prices
    ADD CONSTRAINT fk_rate_plan_prices_plan FOREIGN KEY (rate_plan_id, organization_id) REFERENCES hotel.rate_plans(id, organization_id);


--
-- Name: rate_plan_prices fk_rate_plan_prices_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.rate_plan_prices
    ADD CONSTRAINT fk_rate_plan_prices_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: rate_plans fk_rate_plans_meal; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.rate_plans
    ADD CONSTRAINT fk_rate_plans_meal FOREIGN KEY (meal_plan_id, organization_id) REFERENCES hotel.meal_plans(id, organization_id);


--
-- Name: rate_plans fk_rate_plans_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.rate_plans
    ADD CONSTRAINT fk_rate_plans_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: rate_plans fk_rate_plans_type; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.rate_plans
    ADD CONSTRAINT fk_rate_plans_type FOREIGN KEY (accommodation_type_id, organization_id) REFERENCES hotel.accommodation_types(id, organization_id);


--
-- Name: staff_members fk_staff_members_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_members
    ADD CONSTRAINT fk_staff_members_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: staff_members fk_staff_members_role; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_members
    ADD CONSTRAINT fk_staff_members_role FOREIGN KEY (staff_role_id, organization_id) REFERENCES hotel.staff_roles(id, organization_id);


--
-- Name: staff_payments fk_staff_payments_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_payments
    ADD CONSTRAINT fk_staff_payments_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: staff_payments fk_staff_payments_staff; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_payments
    ADD CONSTRAINT fk_staff_payments_staff FOREIGN KEY (staff_id, organization_id) REFERENCES hotel.staff_members(id, organization_id);


--
-- Name: staff_roles fk_staff_roles_org; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_roles
    ADD CONSTRAINT fk_staff_roles_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id);


--
-- Name: staff_work_logs fk_staff_work_logs_booking; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_work_logs
    ADD CONSTRAINT fk_staff_work_logs_booking FOREIGN KEY (booking_id, organization_id) REFERENCES hotel.bookings(id, organization_id);


--
-- Name: staff_work_logs fk_staff_work_logs_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_work_logs
    ADD CONSTRAINT fk_staff_work_logs_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: staff_work_logs fk_staff_work_logs_staff; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.staff_work_logs
    ADD CONSTRAINT fk_staff_work_logs_staff FOREIGN KEY (staff_id, organization_id) REFERENCES hotel.staff_members(id, organization_id);


--
-- Name: suppliers fk_suppliers_org; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.suppliers
    ADD CONSTRAINT fk_suppliers_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id);


--
-- Name: unit_blocks fk_unit_blocks_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.unit_blocks
    ADD CONSTRAINT fk_unit_blocks_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: unit_blocks fk_unit_blocks_unit; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.unit_blocks
    ADD CONSTRAINT fk_unit_blocks_unit FOREIGN KEY (unit_id, organization_id) REFERENCES hotel.accommodation_units(id, organization_id);


--
-- Name: user_property_access fk_user_property_access_org; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.user_property_access
    ADD CONSTRAINT fk_user_property_access_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id);


--
-- Name: user_property_access fk_user_property_access_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.user_property_access
    ADD CONSTRAINT fk_user_property_access_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: user_property_access fk_user_property_access_user; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.user_property_access
    ADD CONSTRAINT fk_user_property_access_user FOREIGN KEY (user_id) REFERENCES hotel.app_users(id);


--
-- Name: utility_bills fk_utility_bills_property; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.utility_bills
    ADD CONSTRAINT fk_utility_bills_property FOREIGN KEY (property_id, organization_id) REFERENCES hotel.properties(id, organization_id);


--
-- Name: utility_bills fk_utility_bills_type; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.utility_bills
    ADD CONSTRAINT fk_utility_bills_type FOREIGN KEY (utility_type_id, organization_id) REFERENCES hotel.utility_types(id, organization_id);


--
-- Name: utility_types fk_utility_types_org; Type: FK CONSTRAINT; Schema: hotel; Owner: postgres
--

ALTER TABLE ONLY hotel.utility_types
    ADD CONSTRAINT fk_utility_types_org FOREIGN KEY (organization_id) REFERENCES hotel.organizations(id);


--
-- Name: staff_property fk_staff_property_staff; Type: FK CONSTRAINT; Schema: identity; Owner: postgres
--

ALTER TABLE ONLY identity.staff_property
    ADD CONSTRAINT fk_staff_property_staff FOREIGN KEY (staff_uid) REFERENCES identity.staff_user(uid);


--
-- Name: staff_user_role fk_staff_user_role_role; Type: FK CONSTRAINT; Schema: identity; Owner: postgres
--

ALTER TABLE ONLY identity.staff_user_role
    ADD CONSTRAINT fk_staff_user_role_role FOREIGN KEY (role_uid) REFERENCES identity.staff_role(uid);


--
-- Name: staff_user_role fk_staff_user_role_staff_property; Type: FK CONSTRAINT; Schema: identity; Owner: postgres
--

ALTER TABLE ONLY identity.staff_user_role
    ADD CONSTRAINT fk_staff_user_role_staff_property FOREIGN KEY (staff_property_uid) REFERENCES identity.staff_property(uid) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict Mmhe4CqY2EFGMjXVMxVGVaJ7eqX5652Lgv9p4hxrBaT0XSLa3cXfdfcIrkwTeDg

