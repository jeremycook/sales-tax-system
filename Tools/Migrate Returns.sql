select
	taxId
	,triId
	,vendorNumber
	,vendorActive
	,taxTypeName
	,taxEffectiveDate
	,dueDate
	,startDate
	,endDate
	,zeroTax
	,netTaxable
	,excessTax
	,netTax
	,vendorFee
	,naicsCode
	,geocode
	--,sum(case when td.transactionType in ('PAYMENT','CREDIT_APPLY','NSF_PAYMENT') then td.amount else null end) amountPaid
	--,sum(case when td.transactionType in ('CHARGE','CREDIT_USED','NSF') then td.amount else null end) amountCharged
	,sum(case when td.multiplier >= 0 then td.amount else null end) amountPaid
	,sum(case when td.multiplier < 0 then td.amount else null end) amountCharged
	,sum(td.multiplier * td.amount) balance
from (

select 
	t.ID as taxId
	,tri.ID as triId
	,v.vendorNumber
	,cast(isnull((select top 1 1 from ACCOUNT a where a.vendor_ID = v.ID and a.status = 'A'), 0) as bit) vendorActive
	,tt.name as taxTypeName
	,t.effectiveDate as taxEffectiveDate
	,p.dueDate
	,p.startDate
	,p.endDate
	,tri.zeroTax
	,t.netTaxable
	,t.excessTax
	,t.netTax
	,t.vendorFee
	,n.naicsCode
	,geo.code geocode
from (

select
	row_number() over (partition by tri.vendor_ID, tri.period_ID, t.taxtype_ID
		--, case td.description
		--	when 'Audit: Sales Tax (Interest)' then 'Sales Tax'
		--	when 'Audit: Sales Tax (Late Filing Penalty)' then 'Sales Tax'
		--	when 'Audit: Sales Tax (Tax)' then 'Sales Tax'
		--	when 'Fee paid by CCR' then 'Sales Tax'
		--	when 'Interest' then 'Sales Tax'
		--	when 'Late Filing Penalty' then 'Sales Tax'
		--	when 'License Application Fee' then 'License Fee'
		--	when 'License Application Fee - PAID IN ONESTOP' then 'License Fee'
		--	when 'License Fee' then 'License Fee'
		--	when 'License Fee - PAID BY CCR' then 'License Fee'
		--	when 'License Fee - PAID IN ONE STOP' then 'License Fee'
		--	when 'License Fee - PAID IN ONESTOP' then 'License Fee'
		--	when 'License Renewal Fee' then 'License Fee'
		--	when 'Lodging' then 'Lodging Tax'
		--	when 'NSF Fee' then 'Sales Tax'
		--	when 'Anywhere Public Investment Fund Sales' then 'Anywhere Public Investment Fund Sales Tax'
		--	when 'Sales Tax' then 'Sales Tax'
		--	when 'Zero Interest' then 'Sales Tax'
		--	when 'Zero Penalty' then 'Sales Tax'
		--	else 'Uncategorized'
		--end 
		order by t.ID desc) as rowNum,
	t.ID as taxId
from taxreturninstance tri
join vendor v on v.ID = tri.vendor_ID
join transdetail td on (tri.id = td.MAINSOURCE_ID)
join TAX t on t.taxReturnInstance_ID = tri.ID
join TAXTYPE tt on tt.ID = t.taxType_ID
where not (tt.name = 'Lodging' and v.vendorNumber not like '%L')
and not (tt.name <> 'Lodging' and v.vendorNumber like '%L')

) qry1
join TAX t on t.ID = qry1.taxId
join TAXTYPE tt on tt.ID = t.taxType_ID
join TAXRETURNINSTANCE tri on tri.ID = t.taxReturnInstance_ID
join VENDOR v on v.ID = tri.vendor_ID
join FILINGPERIOD p on p.ID = tri.period_ID
left join NAICS n on n.ID = v.naics_ID
left join GEOCODE geo on geo.ID = v.geoCode_ID
where qry1.rowNum = 1

) qry2
left join TRANSDETAIL td on td.mainSource_ID = triId
where startDate >= '2017-01-01'
--and vendorNumber like '003127%' 
--and taxTypeName = 'Lodging'
group by
	taxId
	,triId
	,vendorNumber
	,vendorActive
	,taxTypeName
	,taxEffectiveDate
	,dueDate
	,startDate
	,endDate
	,netTaxable
	,excessTax
	,netTax
	,vendorFee
	,zeroTax
	,naicsCode
	,geocode
order by triId -- dueDate, startDate, vendorNumber
