using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Domain.Enum;
using System.Text;
using iSchool.Domain.Modles;
using System.Data;
using SchoolGrade = iSchool.Domain.Enum.SchoolGrade;
using System.Text.RegularExpressions;

namespace iSchool.Application.Service.Jobs
{
    public class CheckOnlineSchoolS3to7Empty_CommandHandler : IRequestHandler<CheckOnlineSchoolS3to7Empty_Command>
    {
        public CheckOnlineSchoolS3to7Empty_CommandHandler(IServiceProvider sp, 
            ILog log, AppSettings appSettings,
            IMediator mediator, IUnitOfWork unitOfWork)
        {
            this.sp = sp;
            this.log = log;
            this.mediator = mediator;
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.appSettings = appSettings;
        }

        static readonly SemaphoreSlim lck = new SemaphoreSlim(1, 1);

        IServiceProvider sp;
        ILog log;
        IMediator mediator;
        UnitOfWork unitOfWork;
        AppSettings appSettings;

        public async Task<Unit> Handle(CheckOnlineSchoolS3to7Empty_Command cmd, CancellationToken cancellationToken)
        {
            await lck.WaitAsync().ConfigureAwait(false);
            try
            {
                await Handle_Core(cmd).ConfigureAwait(false);
            }
            finally
            {
                lck.Release();
            }
            return Unit.Value;
        }

        async Task Handle_Core(CheckOnlineSchoolS3to7Empty_Command cmd)
        {
            await Task.CompletedTask;
            var sql = $@"
select count(1) from OnlineSchool s with(nolock),OnlineSchoolExtension e with(nolock) where s.id=e.sid and s.IsValid=1 and e.IsValid=1 and e.allowedit=0 
{"and e.sid=@Sid".If(cmd.Sid != null)} {$"and s.ModifyDateTime>='{cmd.Now?.Date.ToString("yyyy-MM-dd")}'".If(cmd.Now != null)}

select e.id,e.sid into #T from OnlineSchool s with(nolock) 
from OnlineSchool s with(nolock) 
inner join OnlineSchoolExtension e with(nolock) on s.id=e.sid
where s.IsValid=1 and e.IsValid=1 and e.allowedit=0 
{"and e.sid=@Sid".If(cmd.Sid != null)} {$"and s.ModifyDateTime>='{cmd.Now?.Date.ToString("yyyy-MM-dd")}'".If(cmd.Now != null)}
order by e.id
OFFSET (@pageIndex-1)*@pageSize ROWS FETCH NEXT @pageSize ROWS ONLY

select s.name as'学校名',e.name as'学部名',e.SchFtype,e.grade as '年级',e.type as '办学类型',e.discount as '是否普惠',e.diglossia as '是否双语',e.chinese as '是否中国人学校',

e3.age as '招生年龄段1',e3.maxage as '招生年龄段2',e3.Count as '招生人数',e3.Target as '招生对象',e3.Proportion as '招录比例',e3.Point as '录取分数线',e3.Date as '招生日期',e3.data as '报名所需资料',e3.Contact as '报名方式',e3.Subjects as '考试科目',e3.Pastexam as '往期入学考试内容',e3.Scholarship as '奖学金计划',

e4.Courses as '课程设置',e4.Characteristic as '课程特色',e4.Authentication as '课程认证',

e5.Applicationfee as '申请费用',e5.Tuition as '学费',e5.Otherfee as '其它费用',

e6.Principal as '校长风采(图文)',e6.Videos as '校长风采(视频)',e6.Teacher as '教师风采',e6.Schoolhonor as '学校荣誉',e6.Studenthonor as '学生荣誉',

e7.Hardware as '硬件设施',e7.Community as '社团活动',e7.Timetables as '各个年级课程表',e7.Schedule as '作息时间表',e7.Diagram as '校车路线',

s.id as sid,e.id as eid
from OnlineSchool s with(nolock)
inner join OnlineSchoolExtension e with(nolock) on s.id=e.sid
inner join #T t on t.id=e.id
left join OnlineSchoolExtContent e2 with(nolock) on e2.eid=e.id and e2.IsValid=1
left join OnlineSchoolExtRecruit e3 with(nolock) on e3.eid=e.id and e3.IsValid=1
left join OnlineSchoolExtCourse e4 with(nolock) on e4.eid=e.id and e4.IsValid=1
left join OnlineSchoolExtCharge e5 with(nolock) on e5.eid=e.id and e5.IsValid=1
left join OnlineSchoolExtQuality e6 with(nolock) on e6.eid=e.id and e6.IsValid=1
left join OnlineSchoolExtlife e7 with(nolock) on e7.eid=e.id and e7.IsValid=1
where s.IsValid=1 and e.IsValid=1 and e.allowedit=0 
drop table #T
";
            
            int pageIndex = 1, pageSize = 500, itemCount = pageSize, row = 1;
            var ls = new List<(string sid, string eid, bool?[] bs)>();
            for (; row < itemCount + 1; pageIndex++)
            {
                var row0 = row;
                try
                {
                    var q = await unitOfWork.DbConnection.TryOpen().QueryMultipleAsync(sql, new { pageIndex, pageSize, cmd.Sid });
                    itemCount = await q.ReadFirstAsync<int>();
                    var data = await q.ReadAsync();

                    foreach (IDictionary<string, object> dy in data)
                    {
                        var ecode = SchFType0.Parse($"{dy["SchFtype"]}");
                        var t = (dy["年级"].ToString().ToEnum<SchoolGrade>(), dy["办学类型"].ToString().ToEnum<SchoolType>(), Convert.ToBoolean(dy["是否普惠"]), Convert.ToBoolean(dy["是否双语"]), Convert.ToBoolean(dy["是否中国人学校"]));
                        var sid = dy["sid"].ToString();
                        var eid = dy["eid"].ToString();
                        var bs = new bool?[10];

                        foreach (var kv in dy)
                        {
                            var x = GetValue(ecode, t, dy, kv.Key, out var step);
                            if (x.In(null, string.Empty, "无")) bs[step] = (bs[step] ?? false) || false;
                            else bs[step] = true;
                        }

                        ls.Add((sid, eid, bs));

                        row++;
                    }

                    update_s3to7(ls);
                    ls.Clear();
                }
                catch (Exception ex)
                {
                    await Task.Delay(1000);
                    row = row0;
                    pageIndex--;
                }
                finally
                {
                    unitOfWork.DbConnection.Close();
                    //unitOfWork.DbConnection.TryOpen();
                }
            }

            if (ls.Count > 0) update_s3to7(ls);
        }

        /// <summary>
        /// null  //无关
        /// ""    //未录入
        /// "xxx" //xxx
        /// </summary>
        /// <returns></returns>
        static string GetValue(SchFType0 ecode, (SchoolGrade Grade, SchoolType Type, bool Discount, bool Diglossia, bool Chinese) dataExt,
            IDictionary<string, object> dy, string k, out int step)
        {
            var val = dy[k];
            var strVal = val?.ToString();
            step = 0;
            switch (k)
            {
                case "sid":
                case "eid":
                case "学校名":
                case "学部名":
                case "录入时间":
                case "录入时间(学部)":
                case "学部标签":
                case "省":
                case "市":
                case "区":
                case "经度":
                case "纬度":
                case "高德ID":
                    return val?.ToString() ?? "";
                case "学部完成率":
                    return $"{Tryv(() => Math.Round(Convert.ToDouble(val) * 100, 2, MidpointRounding.AwayFromZero), 0)}%";
                case "年级":
                    return EnumUtil.GetDesc((SchoolGrade)Convert.ToInt32(val)) ?? "";
                case "办学类型":
                    return EnumUtil.GetDesc((SchoolType)Convert.ToInt32(val)) ?? "";
                case "是否普惠":
                case "是否双语":
                case "是否中国人学校":
                    return Tryv(() => (bool)val) == true ? "是" : "否";
                case "编辑者":
                case "审核人员":
                    return strVal;
                case "学校状态":
                    return EnumUtil.GetDesc((SchoolStatus)Convert.ToInt32(val)) ?? "";
                case "学校英文名":
                case "学校官网":
                case "学校简介":
                case "学校logo":
                case "微信公众号":
                case "地址":
                case "电话":
                    return Tryv0(() => val.ToString().Trim().Length > 1) ? "有" : "无";
                case "数据来源":
                    return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                case "走读/寄宿":
                    {
                        return val == null ? "无" : "有";
                    }
                case "学生人数":
                    return !strVal.IsNullOrEmpty() ? "有" : "无";
                case "教师人数":
                    return !strVal.IsNullOrEmpty() ? "有" : "无";
                case "学校认证":
                    {
                        if (!SchUtils.Canshow2("学校认证", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "出国方向":
                    {
                        if (!SchUtils.Canshow2("出国方向", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "有无饭堂":
                    return val != null ? "有" : "无";
                case "伙食情况":
                    return Tryv0(() => strVal.Trim() != "") ? "有" : "无";
                case "学校特色课程或项目(标签)":
                    {
                        if (!SchUtils.Canshow2("e2.characteristic", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "学校特色课程或项目(图文)":
                    {
                        if (!SchUtils.Canshow2("e2.project", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "") ? "有" : "无";
                    }
                case "高级教师数量":
                case "特级教师数量":
                    return Tryv0(() => val.ToString() != "") ? "有" : "无";
                case "外教占比":
                    {
                        if (!SchUtils.Canshow2("e2.foreigntea", ecode))
                            return null;
                        return Tryv0(() => val.ToString().Trim() != "") ? "有" : "无";
                    }
                case "建校时间":
                case "建筑面积":
                    return Tryv0(() => val.ToString().Trim() != "") ? "有" : "无";
                case "学校专访":
                    return strVal;
                case "线上体验课程":
                    {
                        if (!SchUtils.Canshow2("线上体验课程", ecode))
                            return null;
                        return strVal;
                    }
                case "开放日":
                    {
                        if (!SchUtils.Canshow2("开放日", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "学校行事历":
                    {
                        if (!SchUtils.Canshow2("学校行事历", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "划片范围":
                    {
                        if (!SchUtils.Canshow2("划片范围", ecode))
                            return null;
                        return Tryv0(() => val.ToString().Trim() != "") ? "有" : "无";
                    }
                case "对口学校":
                    {
                        if (!SchUtils.Canshow2("对口学校", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "课后管理":
                    {
                        if (!SchUtils.Canshow2("课后管理", ecode))
                            return null;
                        return Tryv0(() => val.ToString().Trim() != "") ? "有" : "无";
                    }
                case "招生年龄段1":
                case "招生年龄段2":
                    {
                        if (!SchUtils.Canshow2("招生年龄", ecode))
                            return null;
                        return Tryv0(() => val.ToString().Trim() != "") ? "有" : "无";
                    }
                case "招生人数":
                    return Tryv0(() => val.ToString().Trim() != "") ? "有" : "无";
                case "招生对象":
                    return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                case "招录比例":
                    {
                        if (!SchUtils.Canshow2("招录比例", ecode))
                            return null;
                        return Tryv0(() => val.ToString().Trim() != "") ? "有" : "无";
                    }
                case "录取分数线":
                    {
                        if (!SchUtils.Canshow2("录取分数线", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "招生日期":
                    {
                        if (!SchUtils.Canshow2("招生日期", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "报名所需资料":
                    {
                        if (!SchUtils.Canshow2("报名所需资料", ecode))
                            return null;
                        return Tryv0(() => val.ToString().Trim() != "") ? "有" : "无";
                    }
                case "报名方式":
                    {
                        if (!SchUtils.Canshow2("报名方式", ecode))
                            return null;
                        return Tryv0(() => val.ToString().Trim() != "") ? "有" : "无";
                    }
                case "考试科目":
                    {
                        if (!SchUtils.Canshow2("考试科目", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "往期入学考试内容":
                    {
                        if (!SchUtils.Canshow2("往期入学考试内容", ecode))
                            return null;
                        return strVal?.Trim().IsNullOrEmpty() == false ? "有" : "无";
                    }
                case "奖学金计划":
                    {
                        if (!SchUtils.Canshow2("奖学金计划", ecode))
                            return null;
                        return strVal?.Trim().IsNullOrEmpty() == false ? "有" : "无";
                    }
                case "课程设置":
                    {
                        if (!SchUtils.Canshow2("课程设置", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "课程认证":
                    {
                        if (!SchUtils.Canshow2("课程认证", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "课程特色":
                    {
                        if (!SchUtils.Canshow2("课程特色", ecode))
                            return null;
                        return strVal?.Trim().IsNullOrEmpty() == false ? "有" : "无";
                    }
                case "申请费用":
                    {
                        if (!SchUtils.Canshow2("申请费用", ecode))
                            return null;
                        return strVal?.Trim().IsNullOrEmpty() == false ? "有" : "无";
                    }
                case "学费":
                    {
                        if (!SchUtils.Canshow2("申请费用", ecode))
                            return null;
                        return strVal?.Trim().IsNullOrEmpty() == false ? "有" : "无";
                    }
                case "其它费用":
                    {
                        if (!SchUtils.Canshow2("其它费用", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "校长风采(图文)":
                    {
                        if (!SchUtils.Canshow2("e6.principal", ecode))
                            return null;
                        return !string.IsNullOrEmpty(strVal?.Trim()) ? "有" : "无";
                    }
                case "校长风采(视频)":
                    {
                        if (!SchUtils.Canshow2("e6.videos", ecode))
                            return null;
                        return Tryv0(() => val.ToString() != "[]") ? "有" : "无";
                    }
                case "教师风采":
                    return !string.IsNullOrEmpty(strVal?.Trim()) ? "有" : "无";
                case "学校荣誉":
                case "学生荣誉":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return !string.IsNullOrEmpty(strVal?.Trim()) ? "有" : "无";
                    }
                case "硬件设施":
                    return !string.IsNullOrEmpty(strVal?.Trim()) ? "有" : "无";
                case "社团活动":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return !string.IsNullOrEmpty(strVal?.Trim()) ? "有" : "无";
                    }
                case "各个年级课程表":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return !string.IsNullOrEmpty(strVal?.Trim()) ? "有" : "无";
                    }
                case "作息时间表":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return !string.IsNullOrEmpty(strVal?.Trim()) ? "有" : "无";
                    }
                case "校车路线":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return !string.IsNullOrEmpty(strVal?.Trim()) ? "有" : "无";
                    }

                case "升学成绩(幼儿园)-升学情况":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return Convert.ToInt32(val) > 0 ? "有" : "无";
                    }
                case "升学成绩(小学)-升学情况":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return Convert.ToInt32(val) > 0 ? "有" : "无";
                    }
                case "升学成绩(初中)-重点率":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return Convert.ToInt32(val) > 0 ? "有" : "无";
                    }
                case "升学成绩(初中)-中考平均分":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return Convert.ToInt32(val) > 0 ? "有" : "无";
                    }
                case "升学成绩(初中)-当年最高分":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return Convert.ToInt32(val) > 0 ? "有" : "无";
                    }
                case "升学成绩(初中)-高优线录取比例":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return Convert.ToInt32(val) > 0 ? "有" : "无";
                    }
                case "升学成绩(高中)-重本率":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return Convert.ToInt32(val) > 0 ? "有" : "无";
                    }
                case "升学成绩(高中)-本科率":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return Convert.ToInt32(val) > 0 ? "有" : "无";
                    }
                case "升学成绩(高中)-高优线录取人数":
                    {
                        if (!SchUtils.Canshow2(k, ecode))
                            return null;
                        return Convert.ToInt32(val) > 0 ? "有" : "无";
                    }
                case "升学成绩(初中,高中)-毕业去向":
                    {
                        if (!(dataExt.Grade == SchoolGrade.JuniorMiddleSchool || (dataExt.Grade == SchoolGrade.SeniorMiddleSchool)))
                            return null;
                        return Convert.ToInt32(val) > 0 ? "有" : "无";
                    }
            }
            return null;
        }

        void update_s3to7(List<(string sid, string eid, bool?[] bs)> d)
        {
            unitOfWork.DbConnection.TryOpen();
            unitOfWork.BeginTransaction();
            try
            {
                var sb = new StringBuilder();

                foreach (var (sid, eid, bs) in d)
                {
                    sb.Append($@"
delete from SchextStepEmpty where Sid='{sid}' and Eid='{eid}' ;
insert SchextStepEmpty(Sid,Eid,S3,S4,S5,S6,S7,Se367) select '{sid}','{eid}',{((bs[3] ?? false) ? "1" : "0")},{((bs[4] ?? false) ? "1" : "0")},{((bs[5] ?? false) ? "1" : "0")},{((bs[6] ?? false) ? "1" : "0")},{((bs[7] ?? false) ? "1" : "0")},{((bs[3] ?? false) || (bs[6] ?? false) || (bs[7] ?? false) ? 1 : 0)} ;
");
                }

                unitOfWork.DbConnection.Execute(sb.ToString(), null, unitOfWork.DbTransaction);
                unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                throw ex;
            }
        }

        static string[] GetHtmlImageUrlList(string htmlText)
        {
            if (string.IsNullOrEmpty(htmlText))
            {
                return new string[] { };
            }
            var regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
            //新建一个matches的MatchCollection对象 保存 匹配对象个数(img标签)
            var matches = regImg.Matches(htmlText);
            var i = 0;
            var sUrlList = new string[matches.Count];
            //遍历所有的img标签对象
            foreach (Match match in matches)
            {
                //获取所有Img的路径src,并保存到数组中
                sUrlList[i++] = match.Groups["imgUrl"].Value;
            }
            return sUrlList;
        }

        static T Tryv<T>(Func<T> func, T defv = default)
        {
            try { return func(); }
            catch { return defv; }
        }

        static T Tryv0<T>(Func<T> func, T defv = default)
        {
            try
            {
                var x = func();
                if (ReferenceEquals(x, null)) return defv;
                return x;
            }
            catch { return defv; }
        }
    }
}
