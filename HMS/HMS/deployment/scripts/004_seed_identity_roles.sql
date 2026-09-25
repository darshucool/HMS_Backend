-- Seed / repair identity roles for existing databases.
-- Safe to re-run.

BEGIN;

INSERT INTO identity.staff_role(code, name, description)
VALUES
    ('PLATFORM_ADMIN', 'Platform Administrator', 'Full platform super-admin access'),
    ('HOTEL_ADMIN', 'Hotel Administrator', 'Full hotel administration access'),
    ('FRONT_DESK', 'Front Desk', 'Reservation, check-in and checkout access'),
    ('MANAGER', 'Hotel Manager', 'Hotel operational management access'),
    ('HOUSEKEEPING', 'Housekeeping', 'Housekeeping task access'),
    ('RESTAURANT', 'Restaurant Staff', 'Restaurant POS access'),
    ('BAR', 'Bar Staff', 'Bar POS access'),
    ('CASHIER', 'Cashier', 'Billing and payment access')
ON CONFLICT (code) DO UPDATE
SET is_active = TRUE;

COMMIT;
