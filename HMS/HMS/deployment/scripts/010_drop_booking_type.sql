ALTER TABLE hotel.bookings DROP CONSTRAINT IF EXISTS ck_bookings_type;
ALTER TABLE hotel.bookings DROP COLUMN IF EXISTS booking_type;
