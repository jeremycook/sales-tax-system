-- FUNCTION: qry.organization_license(character varying, character varying)

-- DROP FUNCTION qry.organization_license(character varying, character varying);

CREATE OR REPLACE FUNCTION qry.organization_license(
	lower_license_title character varying,
	upper_license_title character varying)
    RETURNS SETOF jsonb 
    LANGUAGE 'sql'
    COST 100
    VOLATILE PARALLEL UNSAFE
    ROWS 1000

AS $BODY$
select jsonb_build_object(
	'license',to_json(l),
	'organization',to_json(o),
	'license_type',to_json(lt)
)
from lic.license as l
inner join org.organization as o on l.organization_id = o.id
inner join lic.license_type as lt on l.type_id = lt.id
where l.title between lower_license_title and upper_license_title
$BODY$;

ALTER FUNCTION qry.organization_license(character varying, character varying)
    OWNER TO jeremy;
