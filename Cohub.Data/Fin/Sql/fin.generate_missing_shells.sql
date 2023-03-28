-- PROCEDURE: fin.generate_missing_shells(date, date)

-- DROP PROCEDURE fin.generate_missing_shells(date, date);

CREATE OR REPLACE PROCEDURE fin.generate_missing_shells(
	in_from_date date,
	in_until_date date)
LANGUAGE 'sql'
AS $BODY$
INSERT INTO fin."return"(status_id, organization_id, period_id, category_id, "values", attributes, created, updated)
select 'Due', pp.organization_id, p.id, pc.category_id, '{}', '{}', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
from fin.payment_plan pp
join fin.payment_configuration pc ON pc.id = pp.payment_configuration_id
join fin.period p on p.frequency_id = pc.frequency_id
where pc.is_active and 
	pp.from_date >= in_from_date and
	pp.until_date <= in_until_date and
	not exists (select null from fin.return r where r.organization_id = pp.organization_id and r.period_id = p.id and r.category_id = pc.category_id)
$BODY$;
