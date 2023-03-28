-- View: qry.organizations_without_descriptions

-- DROP VIEW qry.organizations_without_descriptions;

CREATE OR REPLACE VIEW qry.organizations_without_descriptions
 AS
 SELECT organization.id,
    organization.organization_name,
    organization.organization_description
   FROM org.organization
  WHERE ((organization.organization_description IS NULL) OR (btrim(organization.organization_description) = ''::text));

ALTER TABLE qry.organizations_without_descriptions
    OWNER TO jeremy;

