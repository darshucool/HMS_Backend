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
        SUM(amount) FILTER (
            WHERE status IN ('COMPLETED', 'PARTIALLY_REFUNDED', 'REFUNDED')
        ) AS payments_received
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
