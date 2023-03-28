CREATE OR REPLACE VIEW ins.query
 AS
 SELECT t.table_name AS id
   FROM information_schema.tables t
  WHERE t.table_schema::name = 'qry'::name
  ORDER BY t.table_name;