--select count(1) from Lyega_OLschextSimpleInfo    
--select count(1) from OnlineSchool s join OnlineSchoolExtension e on s.id=e.sid where s.IsValid=1 and e.IsValid=1 and s.status=3
--select count(*) from OnlineSchool o where not exists(select 1 from School s where o.id=s.id) and o.IsValid=1

delete from Lyega_OLschextSimpleInfo where eid not in(select e.id from OnlineSchool s join OnlineSchoolExtension e on s.id=e.sid where s.IsValid=1 and e.IsValid=1 and s.status=3)

insert Lyega_OLschextSimpleInfo (eid,sid,Schname,Extname,grade,type,discount,diglossia,chinese,SchFType,
province,city,area,address,latitude,longitude,LatLong,lodging,sdextern,
TotalScore,IsAuthedByOpen,AuditId,ModifyTime) ---------------------------------------------------------------------------------
select e.id as eid,e.sid,s.name as Schname,e.name as Extname,
e.grade,e.type,e.discount,e.diglossia,e.chinese,e.SchFtype,
c.province,c.city,c.area,c.address,c.latitude,c.longitude,c.LatLong,c.lodging,c.sdextern,
CONVERT(float,null) as TotalScore,CONVERT(bit,null) as IsAuthedByOpen,a.Id as AuditId,
(case when a.ModifyDateTime is not null then a.ModifyDateTime else s.ModifyDateTime end) as ModifyTime
--into Lyega_OLschextSimpleInfo 
from dbo.OnlineSchool s with(nolock)
inner join dbo.OnlineSchoolExtension e with(nolock) on e.sid=s.id
left join dbo.OnlineSchoolExtContent c with(nolock) on c.eid=e.id and c.IsValid=1
left join (select a.* from dbo.SchoolAudit a with(nolock)
inner join (select sid,MAX(CreateTime)mt,(case when Min(CreateTime)=Max(CreateTime) then 1 else 0 end)_isnew from [dbo].SchoolAudit with(nolock) group by sid) a0 on a0.sid=a.sid and a.CreateTime=a0.mt)a on s.id=a.sid
where s.status=3 and s.IsValid=1 and e.IsValid=1 ---------------------------
and not exists(select 1 from Lyega_OLschextSimpleInfo where eid=e.id)
