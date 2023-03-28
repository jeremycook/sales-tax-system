-- Create temp table
CREATE TABLE "anywhereusa"."zero_returns_import" (
	"organization_id" TEXT COLLATE "pg_catalog"."default",
	"category_id" TEXT COLLATE "pg_catalog"."default",
	"period_name" TEXT COLLATE "pg_catalog"."default",
	"type_id" TEXT COLLATE "pg_catalog"."default",
	"due_date" DATE,
	"filing_date" DATE,
	"paid" NUMERIC,
	"balance" NUMERIC,
	"geocode_value" TEXT COLLATE "pg_catalog"."default",
	"geocode_title" TEXT COLLATE "pg_catalog"."default" 
);
-- Insert missing returns
INSERT INTO fin.RETURN ( status_id, organization_id, period_id, category_id, created, updated ) SELECT
'Closed' status_id,
z.organization_id,
P.ID period_id,
COALESCE ( z.category_id, 'Sales Tax' ) category_id,
MAX ( z.filing_date ) created,
MAX ( z.filing_date ) updated -- SELECT *

FROM
	anywhereusa.zero_returns_import z
	JOIN fin.period P ON P.NAME = z.period_name
	LEFT JOIN fin.RETURN r ON r.organization_id = z.organization_id 
	AND r.category_id = COALESCE ( z.category_id, 'Sales Tax' ) 
	AND r.period_id = P.ID 
WHERE
	r.ID IS NULL 
GROUP BY
	z.organization_id,
	z.category_id,
	P.ID;
-- Insert missing filings
INSERT INTO fin.filing ( type_id, return_id, filing_date, created, updated, taxable_amount, excess_tax ) SELECT
'TaxFiling' type_id,
r.ID return_id,
z.filing_date filing_date,
z.filing_date created,
z.filing_date updated,
0 taxable_amount,
0 excess_tax -- SELECT *

FROM
	anywhereusa.zero_returns_import z
	JOIN fin.period P ON P.NAME = z.period_name
	JOIN fin.RETURN r ON r.organization_id = z.organization_id 
	AND r.category_id = COALESCE ( z.category_id, 'Sales Tax' ) 
	AND r.period_id = P.
	ID LEFT JOIN fin.filing f ON f.return_id = r.ID 
	AND f.filing_date = z.filing_date 
WHERE
	f.ID IS NULL;