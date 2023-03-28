-- View: qry.organization_addresses

-- DROP VIEW qry.organization_addresses;

CREATE OR REPLACE VIEW qry.organization_addresses
 AS
 SELECT organization.id,
    organization.organization_name,
    organization.dba,
    organization.physical_address_address_lines,
    organization.mailing_address_address_lines
   FROM org.organization
  ORDER BY organization.id;

ALTER TABLE qry.organization_addresses
    OWNER TO jeremy;

