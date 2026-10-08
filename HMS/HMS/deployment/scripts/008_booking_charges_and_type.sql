ALTER TABLE hotel.bookings
    ADD COLUMN IF NOT EXISTS cooking_charges numeric(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS extra_charges numeric(18,2) NOT NULL DEFAULT 0;

ALTER TABLE hotel.bookings DROP CONSTRAINT IF EXISTS ck_bookings_amounts;
ALTER TABLE hotel.bookings ADD CONSTRAINT ck_bookings_amounts CHECK
    (discount_amount >= 0 AND tax_amount >= 0 AND service_charge >= 0
     AND cooking_charges >= 0 AND extra_charges >= 0 AND COALESCE(quoted_total, 0) >= 0);
