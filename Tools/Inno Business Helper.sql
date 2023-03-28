--select *
--from INFORMATION_SCHEMA.COLUMNS c
--where c.COLUMN_NAME = 'vendor_id'

--select * 
--from dbo.FILINGSCHEDULE fs
--join dbo.FILINGPERIOD fp on fp.filingSchedule_ID = fs.ID
--order by fs.name, fp.period

--select * from dbo.TAXRETURN

select * 
from dbo.ACCOUNT a
join dbo.VENDOR v on v.ID = a.vendor_ID
join dbo.BUSINESS b on b.ID = v.business_ID
