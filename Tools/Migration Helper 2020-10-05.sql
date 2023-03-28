--delete from fin.invoice_schedule
--delete from fin.account
--delete from fin.schedule

select * from fin.account
select * from fin.schedule
select * from fin.period
select * from fin.invoice_schedule
select * from org.organization where id = '000005'

--USE [master]
--GO
--ALTER DATABASE [anywhereusa_cohub] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
--GO
--ALTER DATABASE [anywhereusa_cohub] COLLATE SQL_Latin1_General_CP1_CS_AS
--GO
--ALTER DATABASE [anywhereusa_cohub] SET  MULTI_USER WITH ROLLBACK IMMEDIATE
--GO

--select OBJECT_SCHEMA_NAME(object_id), OBJECT_NAME(object_id), 
--	'alter table '+OBJECT_SCHEMA_NAME(object_id)+'.'+OBJECT_NAME(object_id)+' drop '+name,
--	'alter table '+OBJECT_SCHEMA_NAME(object_id)+'.'+OBJECT_NAME(object_id)+' add '+name+' as '+definition,
--	* from sys.computed_columns c
--GO

--drop index if exists IX_account_category_last_moment on fin.category_gl_account_allocation
--alter table fin.category_gl_account_allocation drop column last_moment
--alter table fin.invoice_schedule drop column until_moment
--alter table fin.period drop column last_moment
--alter table fin.[transaction] drop column balanced

--alter table fin.category_gl_account_allocation add last_moment as (dateadd(day,(1),[last_date]))
--alter table fin.invoice_schedule add until_moment as (dateadd(day,(1),[until_date]))
--alter table fin.period add last_moment as (dateadd(day,(1),[last_date]))
--alter table fin.[transaction] add balanced as (case when [debit_amount]=[credit_amount] then (1) else (0) end)
