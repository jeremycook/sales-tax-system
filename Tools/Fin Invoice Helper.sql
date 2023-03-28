--select count(*), organization_id
--from fin.invoice
--group by organization_id
--order by count(*) desc

--select distinct type_id, account_id from fin.invoice_item
--select distinct type_id, account_id from fin.invoice_payment_item

select i.organization_id, ii.type_id, ii.account_id, ii.period_id, i.total_amount_due, ip.total_amount_paid, sum(ii.amount_due) amount_due, sum(ipi.amount_paid) amount_paid
from fin.invoice i
join fin.invoice_item ii on ii.invoice_id = i.id
join fin.invoice_payment ip on ip.invoice_id = i.id
left join fin.invoice_payment_item ipi on ipi.invoice_payment_id = ip.id --and ipi.type_id = ii.type_id and ipi.account_id = ii.account_id and ipi.period_id = ii.period_id
where organization_id = '000275'
group by i.organization_id, i.total_amount_due, ii.type_id, ii.account_id, ii.period_id, ip.total_amount_paid, ipi.type_id, ipi.account_id, ipi.period_id

--select * --fp.description, vendorNumber, td.description, sum(case when multiplier < 0 then amount else 0 end) debit, sum(case when multiplier > 0 then amount else 0 end) credit
--from fin.invoice i
--join fin.invoice_item ii on ii.invoice_id = i.id
--join 

--delete from fin.invoice
--delete from fin.invoice_payment
--delete from fin.invoice_payment_item

--select * from fin.invoice
--select * from fin.invoice_item
--select * from fin.invoice_payment
--select * from fin.invoice_payment_item
