using iSchool;
using iSchool.Domain.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool
{
    /// <summary>
    /// 字段显示控制类
    /// </summary>
    public static class SchUtils
    {
        /// <summary>
        /// 1后端，2前端
        /// </summary>
		const int plat = 1; 

        /// <summary>
        /// filed 参考 schext_show.xlsx | 学校详情字段表.{version}.xlsx
        /// </summary>
        public static bool Canshow2(string field, in SchFType0 sch) => Can_Show2(sch.ToString(), field?.ToLower(), plat);

        static bool can_show_with_plat(bool defVal, int plat, params bool[] platVals)
        {
            if (plat < 1 || platVals?.Length < 1 || plat > platVals.Length) return defVal;
            return platVals[plat - 1];
        }

        #region Can_Show2

        // from '学校详情字段表-开发版v3 2020.11.25.xlsx'
        static bool Can_Show2(string code, string field, int plat)
        {
            switch (field)
            {
                case "学校英文名":
                case "s.name_e":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "学校官网":
                case "s.website":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "学校简介":
                case "s.intro":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "学校logo":
                case "s.logo":
                    {
                        return code == "lx110" ? can_show_with_plat(true, plat, true, false)    //公办幼儿园
                            : code == "lx120" ? can_show_with_plat(true, plat, true, false)    //普通民办幼儿园
                            : code == "lx121" ? can_show_with_plat(true, plat, true, false)    //民办普惠幼儿园
                            : code == "lx130" ? can_show_with_plat(true, plat, true, false)    //国际幼儿园
                            : code == "lx199" ? can_show_with_plat(true, plat, true, false)    //幼托机构
                            : code == "lx210" ? can_show_with_plat(true, plat, true, false)    //公办小学
                            : code == "lx220" ? can_show_with_plat(true, plat, true, false)    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? can_show_with_plat(true, plat, true, false)    //外国人小学
                            : code == "lx310" ? can_show_with_plat(true, plat, true, false)    //公办初中
                            : code == "lx320" ? can_show_with_plat(true, plat, true, false)    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? can_show_with_plat(true, plat, true, false)    //外国人初中
                            : code == "lx410" ? can_show_with_plat(true, plat, true, false)    //公办高中
                            : code == "lx420" ? can_show_with_plat(true, plat, true, false)    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? can_show_with_plat(true, plat, true, false)    //国际高中
                            : code == "lx430" ? can_show_with_plat(true, plat, true, false)    //外国人高中
                            : code == "lx180" ? can_show_with_plat(true, plat, true, false)    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "学部标签":
                case "generaltag":
                    {
                        return code == "lx110" ? can_show_with_plat(true, plat, true, false)    //公办幼儿园
                            : code == "lx120" ? can_show_with_plat(true, plat, true, false)    //普通民办幼儿园
                            : code == "lx121" ? can_show_with_plat(true, plat, true, false)    //民办普惠幼儿园
                            : code == "lx130" ? can_show_with_plat(true, plat, true, false)    //国际幼儿园
                            : code == "lx199" ? can_show_with_plat(true, plat, true, false)    //幼托机构
                            : code == "lx210" ? can_show_with_plat(true, plat, true, false)    //公办小学
                            : code == "lx220" ? can_show_with_plat(true, plat, true, false)    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? can_show_with_plat(true, plat, true, false)    //外国人小学
                            : code == "lx310" ? can_show_with_plat(true, plat, true, false)    //公办初中
                            : code == "lx320" ? can_show_with_plat(true, plat, true, false)    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? can_show_with_plat(true, plat, true, false)    //外国人初中
                            : code == "lx410" ? can_show_with_plat(true, plat, true, false)    //公办高中
                            : code == "lx420" ? can_show_with_plat(true, plat, true, false)    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? can_show_with_plat(true, plat, true, false)    //国际高中
                            : code == "lx430" ? can_show_with_plat(true, plat, true, false)    //外国人高中
                            : code == "lx180" ? can_show_with_plat(true, plat, true, false)    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "数据来源":
                case "e.source":
                    {
                        return code == "lx110" ? can_show_with_plat(true, plat, true, false)    //公办幼儿园
                            : code == "lx120" ? can_show_with_plat(true, plat, true, false)    //普通民办幼儿园
                            : code == "lx121" ? can_show_with_plat(true, plat, true, false)    //民办普惠幼儿园
                            : code == "lx130" ? can_show_with_plat(true, plat, true, false)    //国际幼儿园
                            : code == "lx199" ? can_show_with_plat(true, plat, true, false)    //幼托机构
                            : code == "lx210" ? can_show_with_plat(true, plat, true, false)    //公办小学
                            : code == "lx220" ? can_show_with_plat(true, plat, true, false)    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? can_show_with_plat(true, plat, true, false)    //外国人小学
                            : code == "lx310" ? can_show_with_plat(true, plat, true, false)    //公办初中
                            : code == "lx320" ? can_show_with_plat(true, plat, true, false)    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? can_show_with_plat(true, plat, true, false)    //外国人初中
                            : code == "lx410" ? can_show_with_plat(true, plat, true, false)    //公办高中
                            : code == "lx420" ? can_show_with_plat(true, plat, true, false)    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? can_show_with_plat(true, plat, true, false)    //国际高中
                            : code == "lx430" ? can_show_with_plat(true, plat, true, false)    //外国人高中
                            : code == "lx180" ? can_show_with_plat(true, plat, true, false)    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "微信公众号":
                case "e.weixin":
                    {
                        return code == "lx110" ? can_show_with_plat(true, plat, true, false)    //公办幼儿园
                            : code == "lx120" ? can_show_with_plat(true, plat, true, false)    //普通民办幼儿园
                            : code == "lx121" ? can_show_with_plat(true, plat, true, false)    //民办普惠幼儿园
                            : code == "lx130" ? can_show_with_plat(true, plat, true, false)    //国际幼儿园
                            : code == "lx199" ? can_show_with_plat(true, plat, true, false)    //幼托机构
                            : code == "lx210" ? can_show_with_plat(true, plat, true, false)    //公办小学
                            : code == "lx220" ? can_show_with_plat(true, plat, true, false)    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? can_show_with_plat(true, plat, true, false)    //外国人小学
                            : code == "lx310" ? can_show_with_plat(true, plat, true, false)    //公办初中
                            : code == "lx320" ? can_show_with_plat(true, plat, true, false)    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? can_show_with_plat(true, plat, true, false)    //外国人初中
                            : code == "lx410" ? can_show_with_plat(true, plat, true, false)    //公办高中
                            : code == "lx420" ? can_show_with_plat(true, plat, true, false)    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? can_show_with_plat(true, plat, true, false)    //国际高中
                            : code == "lx430" ? can_show_with_plat(true, plat, true, false)    //外国人高中
                            : code == "lx180" ? can_show_with_plat(true, plat, true, false)    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "高德id":
                case "e1.claimedamapeid":
                    {
                        return code == "lx110" ? can_show_with_plat(true, plat, true, false)    //公办幼儿园
                            : code == "lx120" ? can_show_with_plat(true, plat, true, false)    //普通民办幼儿园
                            : code == "lx121" ? can_show_with_plat(true, plat, true, false)    //民办普惠幼儿园
                            : code == "lx130" ? can_show_with_plat(true, plat, true, false)    //国际幼儿园
                            : code == "lx199" ? can_show_with_plat(true, plat, true, false)    //幼托机构
                            : code == "lx210" ? can_show_with_plat(true, plat, true, false)    //公办小学
                            : code == "lx220" ? can_show_with_plat(true, plat, true, false)    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? can_show_with_plat(true, plat, true, false)    //外国人小学
                            : code == "lx310" ? can_show_with_plat(true, plat, true, false)    //公办初中
                            : code == "lx320" ? can_show_with_plat(true, plat, true, false)    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? can_show_with_plat(true, plat, true, false)    //外国人初中
                            : code == "lx410" ? can_show_with_plat(true, plat, true, false)    //公办高中
                            : code == "lx420" ? can_show_with_plat(true, plat, true, false)    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? can_show_with_plat(true, plat, true, false)    //国际高中
                            : code == "lx430" ? can_show_with_plat(true, plat, true, false)    //外国人高中
                            : code == "lx180" ? can_show_with_plat(true, plat, true, false)    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "省":
                case "e2.province":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "市":
                case "e2.city":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "区":
                case "e2.area":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "经度":
                case "e2.longitude":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "纬度":
                case "e2.latitude":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "地址":
                case "e2.address":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "电话":
                case "e2.tel":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "走读/寄宿":
                case "e2.lodging":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "学生人数":
                case "e2.studentcount":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "教师人数":
                case "e2.teachercount":
                    {
                        return code == "lx110" ? can_show_with_plat(true, plat, true, false)    //公办幼儿园
                            : code == "lx120" ? can_show_with_plat(true, plat, true, false)    //普通民办幼儿园
                            : code == "lx121" ? can_show_with_plat(true, plat, true, false)    //民办普惠幼儿园
                            : code == "lx130" ? can_show_with_plat(true, plat, true, false)    //国际幼儿园
                            : code == "lx199" ? can_show_with_plat(true, plat, true, false)    //幼托机构
                            : code == "lx210" ? can_show_with_plat(true, plat, true, false)    //公办小学
                            : code == "lx220" ? can_show_with_plat(true, plat, true, false)    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? can_show_with_plat(true, plat, true, false)    //外国人小学
                            : code == "lx310" ? can_show_with_plat(true, plat, true, false)    //公办初中
                            : code == "lx320" ? can_show_with_plat(true, plat, true, false)    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? can_show_with_plat(true, plat, true, false)    //外国人初中
                            : code == "lx410" ? can_show_with_plat(true, plat, true, false)    //公办高中
                            : code == "lx420" ? can_show_with_plat(true, plat, true, false)    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? can_show_with_plat(true, plat, true, false)    //国际高中
                            : code == "lx430" ? can_show_with_plat(true, plat, true, false)    //外国人高中
                            : code == "lx180" ? can_show_with_plat(true, plat, true, false)    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "师生比":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "学校认证":
                case "e2.authentication":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? false    //港澳台高中
                            : true;
                    }
                case "出国方向":
                case "e2.abroad":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "有无饭堂":
                case "e2.canteen":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "伙食情况":
                case "e2.meal":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "学校特色课程或项目":
                case "e2.characteristic+e2.project":
                case "e2.characteristic,e2.project":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "学校特色课程或项目(标签)":
                case "e2.characteristic":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "学校特色课程或项目(图文)":
                case "e2.project":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "高级教师数量":
                case "e2.seniortea":
                    {
                        return code == "lx110" ? can_show_with_plat(true, plat, true, false)    //公办幼儿园
                            : code == "lx120" ? can_show_with_plat(true, plat, true, false)    //普通民办幼儿园
                            : code == "lx121" ? can_show_with_plat(true, plat, true, false)    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? can_show_with_plat(true, plat, true, false)    //幼托机构
                            : code == "lx210" ? can_show_with_plat(true, plat, true, false)    //公办小学
                            : code == "lx220" ? can_show_with_plat(true, plat, true, false)    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? can_show_with_plat(true, plat, true, false)    //公办初中
                            : code == "lx320" ? can_show_with_plat(true, plat, true, false)    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? can_show_with_plat(true, plat, true, false)    //公办高中
                            : code == "lx420" ? can_show_with_plat(true, plat, true, false)    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? can_show_with_plat(true, plat, true, false)    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "特级教师数量":
                case "e2.characteristictea":
                    {
                        return code == "lx110" ? can_show_with_plat(true, plat, true, false)    //公办幼儿园
                            : code == "lx120" ? can_show_with_plat(true, plat, true, false)    //普通民办幼儿园
                            : code == "lx121" ? can_show_with_plat(true, plat, true, false)    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? can_show_with_plat(true, plat, true, false)    //幼托机构
                            : code == "lx210" ? can_show_with_plat(true, plat, true, false)    //公办小学
                            : code == "lx220" ? can_show_with_plat(true, plat, true, false)    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? can_show_with_plat(true, plat, true, false)    //公办初中
                            : code == "lx320" ? can_show_with_plat(true, plat, true, false)    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? can_show_with_plat(true, plat, true, false)    //公办高中
                            : code == "lx420" ? can_show_with_plat(true, plat, true, false)    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? can_show_with_plat(true, plat, true, false)    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "外教人数":
                case "e2.foreignteacount":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? can_show_with_plat(true, plat, true, false)    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? can_show_with_plat(true, plat, true, false)    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? can_show_with_plat(true, plat, true, false)    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? can_show_with_plat(true, plat, true, false)    //国际高中
                            : code == "lx430" ? can_show_with_plat(true, plat, true, false)    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "外教占比":
                case "e2.foreigntea":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "建校时间":
                case "e2.creationdate":
                    {
                        return code == "lx110" ? can_show_with_plat(true, plat, true, false)    //公办幼儿园
                            : code == "lx120" ? can_show_with_plat(true, plat, true, false)    //普通民办幼儿园
                            : code == "lx121" ? can_show_with_plat(true, plat, true, false)    //民办普惠幼儿园
                            : code == "lx130" ? can_show_with_plat(true, plat, true, false)    //国际幼儿园
                            : code == "lx199" ? can_show_with_plat(true, plat, true, false)    //幼托机构
                            : code == "lx210" ? can_show_with_plat(true, plat, true, false)    //公办小学
                            : code == "lx220" ? can_show_with_plat(true, plat, true, false)    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? can_show_with_plat(true, plat, true, false)    //外国人小学
                            : code == "lx310" ? can_show_with_plat(true, plat, true, false)    //公办初中
                            : code == "lx320" ? can_show_with_plat(true, plat, true, false)    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? can_show_with_plat(true, plat, true, false)    //外国人初中
                            : code == "lx410" ? can_show_with_plat(true, plat, true, false)    //公办高中
                            : code == "lx420" ? can_show_with_plat(true, plat, true, false)    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? can_show_with_plat(true, plat, true, false)    //国际高中
                            : code == "lx430" ? can_show_with_plat(true, plat, true, false)    //外国人高中
                            : code == "lx180" ? can_show_with_plat(true, plat, true, false)    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "建筑面积":
                case "e2.acreage":
                    {
                        return code == "lx110" ? can_show_with_plat(true, plat, true, false)    //公办幼儿园
                            : code == "lx120" ? can_show_with_plat(true, plat, true, false)    //普通民办幼儿园
                            : code == "lx121" ? can_show_with_plat(true, plat, true, false)    //民办普惠幼儿园
                            : code == "lx130" ? can_show_with_plat(true, plat, true, false)    //国际幼儿园
                            : code == "lx199" ? can_show_with_plat(true, plat, true, false)    //幼托机构
                            : code == "lx210" ? can_show_with_plat(true, plat, true, false)    //公办小学
                            : code == "lx220" ? can_show_with_plat(true, plat, true, false)    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? can_show_with_plat(true, plat, true, false)    //外国人小学
                            : code == "lx310" ? can_show_with_plat(true, plat, true, false)    //公办初中
                            : code == "lx320" ? can_show_with_plat(true, plat, true, false)    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? can_show_with_plat(true, plat, true, false)    //外国人初中
                            : code == "lx410" ? can_show_with_plat(true, plat, true, false)    //公办高中
                            : code == "lx420" ? can_show_with_plat(true, plat, true, false)    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? can_show_with_plat(true, plat, true, false)    //国际高中
                            : code == "lx430" ? can_show_with_plat(true, plat, true, false)    //外国人高中
                            : code == "lx180" ? can_show_with_plat(true, plat, true, false)    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "学校专访":
                case "vdo.type=2":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "线上体验课程":
                case "vdo.type=3":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "开放日":
                case "e2.openhours":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "学校行事历":
                case "e2.calendar":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "划片范围":
                case "e2.range":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? false    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? false    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? false    //港澳台初中
                            : code == "lx480" ? false    //港澳台高中
                            : true;
                    }
                case "对口学校":
                case "e2.counterpart":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? false    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? false    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? false    //港澳台初中
                            : code == "lx480" ? false    //港澳台高中
                            : true;
                    }
                case "课后管理":
                case "e2.afterclass":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? false    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? false    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? false    //港澳台初中
                            : code == "lx480" ? false    //港澳台高中
                            : true;
                    }
                case "招生年龄":
                case "e3.age+e3.maxage":
                case "e3.age,e3.maxage":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "招生年龄段1":
                case "e3.age":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "招生年龄段2":
                case "e3.maxage":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "招生人数":
                case "e3.count":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "招生对象":
                case "e3.target":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "招录比例":
                case "e3.proportion":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? can_show_with_plat(true, plat, true, false)    //普通民办幼儿园
                            : code == "lx121" ? can_show_with_plat(true, plat, true, false)    //民办普惠幼儿园
                            : code == "lx130" ? can_show_with_plat(true, plat, true, false)    //国际幼儿园
                            : code == "lx199" ? can_show_with_plat(true, plat, true, false)    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? can_show_with_plat(true, plat, true, false)    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? can_show_with_plat(true, plat, true, false)    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? can_show_with_plat(true, plat, true, false)    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? can_show_with_plat(true, plat, true, false)    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? can_show_with_plat(true, plat, true, false)    //国际高中
                            : code == "lx430" ? can_show_with_plat(true, plat, true, false)    //外国人高中
                            : code == "lx180" ? can_show_with_plat(true, plat, true, false)    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "录取分数线":
                case "e3.point":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? can_show_with_plat(true, plat, true, false)    //普通民办小学
                            : code == "lx231" ? can_show_with_plat(true, plat, true, false)    //双语小学
                            : code == "lx230" ? can_show_with_plat(true, plat, true, false)    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? can_show_with_plat(true, plat, true, false)    //普通民办初中
                            : code == "lx331" ? can_show_with_plat(true, plat, true, false)    //双语初中
                            : code == "lx330" ? can_show_with_plat(true, plat, true, false)    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? can_show_with_plat(true, plat, true, false)    //双语高中
                            : code == "lx432" ? can_show_with_plat(true, plat, true, false)    //国际高中
                            : code == "lx430" ? can_show_with_plat(true, plat, true, false)    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? can_show_with_plat(true, plat, true, false)    //港澳台小学
                            : code == "lx380" ? can_show_with_plat(true, plat, true, false)    //港澳台初中
                            : code == "lx480" ? can_show_with_plat(true, plat, true, false)    //港澳台高中
                            : true;
                    }
                case "招生日期":
                case "e3.date":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "报名所需资料":
                case "e3.data":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "报名方式":
                case "e3.contact":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "考试科目":
                case "e3.subjects":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "往期入学考试内容":
                case "e3.pastexam":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "奖学金计划":
                case "e3.scholarship":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "课程设置":
                case "e4.courses":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "课程特色":
                case "e4.characteristic":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "课程认证":
                case "e4.authentication":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "申请费用":
                case "e5.applicationfee":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "学费":
                case "e5.tuition":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "其它费用":
                case "e5.otherfee":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "校长风采":
                case "e6.principal+e6.videos":
                case "e6.principal,e6.videos":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "校长风采(图文)":
                case "e6.principal":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "校长风采(视频)":
                case "e6.videos":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "教师风采":
                case "e6.teacher":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "学校荣誉":
                case "e6.schoolhonor":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "学生荣誉":
                case "e6.studenthonor":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "硬件设施":
                case "e7.hardware":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "社团活动":
                case "e7.community":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "各个年级课程表":
                case "e7.timetables":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? true    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "作息时间表":
                case "e7.schedule":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "校车路线":
                case "e7.diagram":
                    {
                        return code == "lx110" ? true    //公办幼儿园
                            : code == "lx120" ? true    //普通民办幼儿园
                            : code == "lx121" ? true    //民办普惠幼儿园
                            : code == "lx130" ? true    //国际幼儿园
                            : code == "lx199" ? true    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? true    //普通民办小学
                            : code == "lx231" ? true    //双语小学
                            : code == "lx230" ? true    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "升学成绩(幼儿园)-升学情况":
                case "ka.link":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? false    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? false    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? false    //港澳台初中
                            : code == "lx480" ? false    //港澳台高中
                            : true;
                    }
                case "升学成绩(小学)-升学情况":
                case "psa.link":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? false    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? false    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? false    //港澳台初中
                            : code == "lx480" ? false    //港澳台高中
                            : true;
                    }
                case "升学成绩-升学情况":
                case "sa.link":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? false    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? false    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? true    //港澳台幼儿园
                            : code == "lx280" ? true    //港澳台小学
                            : code == "lx380" ? false    //港澳台初中
                            : code == "lx480" ? false    //港澳台高中
                            : true;
                    }
                case "升学成绩(初中)-重点率":
                case "msa.keyrate":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? false    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? false    //港澳台高中
                            : true;
                    }
                case "升学成绩(初中)-中考平均分":
                case "msa.average":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? false    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? false    //港澳台高中
                            : true;
                    }
                case "升学成绩(初中)-当年最高分":
                case "msa.highest":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? false    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? false    //港澳台高中
                            : true;
                    }
                case "升学成绩(初中)-高优线录取比例":
                case "msa.ratio":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? false    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? false    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? false    //港澳台高中
                            : true;
                    }
                case "升学成绩(初中)-毕业去向":
                case "m.sa.count":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? false    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? false    //港澳台高中
                            : true;
                    }
                case "升学成绩-毕业去向":
                case "sa.count":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? true    //公办初中
                            : code == "lx320" ? true    //普通民办初中
                            : code == "lx331" ? true    //双语初中
                            : code == "lx330" ? true    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? true    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "升学成绩(高中)-毕业去向":
                case "h.sa.count":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? false    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? true    //国际高中
                            : code == "lx430" ? true    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? false    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "升学成绩(高中)-重本率":
                case "hsa.keyundergraduate":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? false    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? false    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "升学成绩(高中)-本科率":
                case "hsa.undergraduate":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? false    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? true    //公办高中
                            : code == "lx420" ? true    //普通民办高中
                            : code == "lx431" ? true    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? false    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
                case "升学成绩(高中)-高优线录取人数":
                case "hsa.count":
                    {
                        return code == "lx110" ? false    //公办幼儿园
                            : code == "lx120" ? false    //普通民办幼儿园
                            : code == "lx121" ? false    //民办普惠幼儿园
                            : code == "lx130" ? false    //国际幼儿园
                            : code == "lx199" ? false    //幼托机构
                            : code == "lx210" ? false    //公办小学
                            : code == "lx220" ? false    //普通民办小学
                            : code == "lx231" ? false    //双语小学
                            : code == "lx230" ? false    //外国人小学
                            : code == "lx310" ? false    //公办初中
                            : code == "lx320" ? false    //普通民办初中
                            : code == "lx331" ? false    //双语初中
                            : code == "lx330" ? false    //外国人初中
                            : code == "lx410" ? false    //公办高中
                            : code == "lx420" ? false    //普通民办高中
                            : code == "lx431" ? false    //双语高中
                            : code == "lx432" ? false    //国际高中
                            : code == "lx430" ? false    //外国人高中
                            : code == "lx180" ? false    //港澳台幼儿园
                            : code == "lx280" ? false    //港澳台小学
                            : code == "lx380" ? false    //港澳台初中
                            : code == "lx480" ? true    //港澳台高中
                            : true;
                    }
            }
            return true;
        }

        #endregion Can_Show2
    }
}
