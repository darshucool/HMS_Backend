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

-- Recommended report filters:
-- SELECT * FROM hotel.vw_monthly_property_summary
-- WHERE property_uid = :property_uid AND report_month = DATE '2026-08-01';
--
-- SELECT * FROM hotel.vw_booking_profitability
-- WHERE property_id = :authorized_property_id ORDER BY check_in_date DESC;
