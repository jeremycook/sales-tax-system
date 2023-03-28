select *
from (
select 
	v.vendornumber AS vendornumber, 
	d.name as dbaname, 
	tr.name as taxreturname,
	tri.dueDate AS duedate,
	ib.posteddate as posteddate, 
	ib.batchnumber as batchnumber, 
	n.naicscode as naicscode, 
	gc.code as geocode,
	fp.description as filingperiod,
	fs.name as filingschedule,
	sum(tx.netTax + pi.penaltyAmount + ii.interestAmount - tx.vendorFee) as totaldue,
	sum(td.multiplier * td.AMOUNT) as paid, 
	sum(td.multiplier * td.AMOUNT) - sum(tx.netTax + pi.penaltyAmount + ii.interestAmount - tx.vendorFee) as balance,
	sum(pi.penaltyAmount) as penalty,
	sum(ii.interestAmount) as interest,
	sum(tx.netTax) as netTax,
	sum(tx.excessTax) as excesstax,
	sum(tx.vendorFee) as vendorfee,
	txt.name as taxtype
from innobatch ib
join transdetail td on (ib.id = td.batch_id and td.TRANSACTIONTYPE='PAYMENT')
join transsource ts on ts.id = td.TRANSACTIONSOURCE_ID
join taxreturninstance tri on (tri.id = td.MAINSOURCE_ID)
join taxreturn tr on tr.id = tri.taxreturn_id
join transsource ts2 on ts2.id = tri.id
join vendor v on v.id = tri.vendor_id
join naics n on n.id = v.naics_id
join geocode gc on gc.id = v.geocode_id
join business b on b.id = v.business_id
join dba d on d.id = v.primarydba_id
join filingperiod fp on fp.id = tri.PERIOD_ID
join FILINGSCHEDULE fs on fs.id = fp.filingSchedule_ID
join penaltyinstance pi on pi.id = ts2.PENALTYINSTANCE_ID
join interestinstance ii on ii.id = ts2.INTERESTINSTANCE_ID
join TAX tx on tx.taxReturnInstance_ID = tri.ID
join TAXTYPE txt on txt.ID = tx.taxType_ID

where tri.dueDate >= '2017-01-01'
AND tx.vendorFee is not null AND tx.vendorFee > 0

group by ib.postedDate,ib.BATCHNUMBER,tri.dueDate,d.name,n.label,gc.code,tr.name,v.vendorNumber, fp.description, fs.name, txt.name

) query
where abs(balance) > 9.99 and paid > 0
order by dueDate, dbaname
