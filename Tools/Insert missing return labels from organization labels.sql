-- SELECT * FROM fin.return;
INSERT INTO fin.return_label ( return_id, label_id, created )
SELECT
	r.ID return_id,
	 z.label_id,
	CURRENT_TIMESTAMP created 
FROM
	org.organization_label z
	JOIN fin."return" r ON r.organization_id = z.organization_id 
WHERE
	NOT EXISTS ( SELECT * FROM fin.return_label WHERE return_label.return_id = r."id" AND return_label.label_id = z.label_id );