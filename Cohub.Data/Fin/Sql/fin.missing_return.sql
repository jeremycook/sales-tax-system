CREATE OR REPLACE VIEW fin.missing_return
AS SELECT pp.payment_configuration_id,
    pp.organization_id,
    r.period_id = p.id,
    p.due_date
   FROM fin.payment_configuration pc
     JOIN fin.payment_plan pp ON pc.id = pp.payment_configuration_id
     JOIN fin.period p ON p.frequency_id = pc.frequency_id AND p.from_date >= pp.from_date AND p.from_date <= pp.until_date
     LEFT JOIN fin.return r ON r.organization_id::text = pp.organization_id::text AND r.category_id = pc.category_id AND r.period_id = p.id
  WHERE r.id IS NULL
  ORDER BY pc.id