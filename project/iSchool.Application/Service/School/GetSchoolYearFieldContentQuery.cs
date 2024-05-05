using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    /// <summary>
    /// 年份字段内容查询
    /// </summary>
    public class GetSchoolYearFieldContentQuery:IRequest<Dictionary<string, string>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="eid">学部Id</param>
        /// <param name="year">年份</param>
        public GetSchoolYearFieldContentQuery(string fieldName, Guid eid, int year) 
        {
            this.FieldName = fieldName;
            this.Eid = eid;
            this.Year = year;
        }

        /// <summary>
        /// 字段名字(如果有多个字段，则用英文竖线|隔开,查数据时要先判断是否多字段)
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 学部Id
        /// </summary>
        public Guid  Eid { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

    }
}
