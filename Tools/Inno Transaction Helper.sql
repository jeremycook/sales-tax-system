--select * from INFORMATION_SCHEMA.COLUMNS c where c.COLUMN_NAME like '%transaction%'

--select description, transactionType, multiplier, count(*) count, sum(amount) total_amount from TRANSDETAIL group by description, transactionType, multiplier order by description, transactionType
--select transactionType, description, case when multiplier < 0 then 0 else 1 end is_payment, count(*) count, sum(amount) total_amount from TRANSDETAIL group by transactionType, description, multiplier order by transactionType, description
--select transactionType, case when multiplier < 0 then 0 else 1 end is_payment, multiplier, count(*) count, sum(amount) total_amount from TRANSDETAIL group by transactionType, multiplier order by transactionType
--select count(posteddate) from TRANSDETAIL

--select distinct transactionType from TRANSDETAIL
--select * from TRANSDETAIL where description = 'Zero Penalty'


select 
	fp.description, 
	v.vendorNumber, 
	td.description, 
	td.transactionType,
	ispayment = case when td.multiplier >= 0 then 1 else 0 end,
	amount = td.multiplier * td.amount,
	p.paymentMethod
from TRANSDETAIL td 
join TAXRETURNINSTANCE tri on tri.ID = td.mainSource_ID
join VENDOR v on v.ID = tri.vendor_ID -- tri.vendor_ID = v.ID tri.ID = td.mainSource_ID
join TRANSSOURCE ts on ts.ID = tri.ID
join FILINGPERIOD fp on fp.ID = tri.period_ID
left join PAYMENT_DETAIL pd on pd.DETAIL_ID = td.id
left join PAYMENT p on p.ID = pd.PAYMENT_ID
where td.captureDate >= '2017-01-01'
--where tri.vendor_ID = 78218783 -- and td.mainSource_ID = 83891345
--where transactionId = 'ST-2020-0038747'
--where dueDate = '2020-08-20 00:00:00.000'
--where tri.ID = 83344633
--where transactionId = 'ST-2019-0031643'
order by fp.year, fp.period, v.vendorNumber

--select fp.description, vendorNumber, td.description, sum(case when multiplier < 0 then amount else 0 end) debit, sum(case when multiplier > 0 then amount else 0 end) credit
--from TRANSDETAIL td 
--join TAXRETURNINSTANCE tri on tri.ID = td.mainSource_ID
--join VENDOR v on v.ID = tri.vendor_ID -- tri.vendor_ID = v.ID tri.ID = td.mainSource_ID
--join TRANSSOURCE ts on ts.ID = tri.ID
--join FILINGPERIOD fp on fp.ID = tri.period_ID
----where tri.vendor_ID = 78218783 -- and td.mainSource_ID = 83891345
----where transactionId = 'ST-2020-0038747'
----where dueDate = '2020-08-20 00:00:00.000'
----where tri.ID = 83344633
----where transactionId = 'ST-2019-0031643'
--group by fp.year, fp.period, fp.description, vendorNumber, td.description
--having abs(sum(multiplier * amount)) > 9.99
--order by fp.year, fp.period, vendorNumber

--select * 
--from TRANSDETAIL td 
--join TAXRETURNINSTANCE tri on tri.ID = td.mainSource_ID
--join VENDOR v on v.ID = tri.vendor_ID -- tri.vendor_ID = v.ID tri.ID = td.mainSource_ID
--join TRANSSOURCE ts on ts.ID = tri.ID
--where tri.vendor_ID = 78218783 and td.mainSource_ID = 83891345
----where transactionId = 'ST-2020-0038747'
----where dueDate = '2020-08-20 00:00:00.000'
----where tri.ID = 83344633
----where transactionId = 'ST-2019-0031643'
--order by td.id, dueDate, captureDate
