ALTER TABLE hotel.guests
    ADD COLUMN IF NOT EXISTS identity_number varchar(100);
