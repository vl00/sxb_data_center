using AutoMapper;
using Dapper;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool
{
    /// <summary>
    /// 业务帮助类
    /// </summary>
    public static class BusinessHelper
    {
        #region 走读寄宿

        /// <summary>
        /// 设置走读寄宿选中值
        /// </summary>
        /// <param name="lodging">是否寄宿</param>
        /// <param name="sdextern">是否走读</param>
        /// <returns>0:未收录; 1:走读；2：寄宿；3:可走读、寄宿</returns>
        public static int SetLodgingSdExternSelectValue(bool? lodging, bool? sdextern)
        {
            int selectValue = 0;//默认值时未收录
            //--0:未收录 => lodging = null && sdextern=null 
            //if (lodging == null && sdextern == null) { selectValue = 0; }
            //--1:走读   => lodging=False && sdextern=True
            if (lodging == false && sdextern == true) { selectValue = 1; }
            //--2:寄宿   => lodging=True && sdextern=False
            else if (lodging == true && sdextern == false) { selectValue = 2; }
            //--3:可走读、寄宿   => lodging=True && sdextern=True
            else if (lodging == true && sdextern == true) { selectValue = 3; }
            return selectValue;
        }

        /// <summary>
        /// 保存时，走读寄宿分开两个字段
        /// </summary>
        /// <param name="selectValue">走读寄宿选中的值</param>
        /// <returns></returns>
        public static Dictionary<string, bool?> SaveLodgingSdExtern(string selectValue)
        {
            Dictionary<string, bool?> dic = new Dictionary<string, bool?>();
            switch (selectValue)
            {
                //--0:未收录 => lodging = null && sdextern=null 
                case "0":
                    dic.Add("lodging", null);
                    dic.Add("sdextern", null);
                    break;

                //--1:走读   => lodging=false && sdextern=true
                case "1":
                    dic.Add("lodging", false);
                    dic.Add("sdextern", true);
                    break;

                //--2:寄宿   => lodging=true && sdextern=false
                case "2":
                    dic.Add("lodging", true);
                    dic.Add("sdextern", false);
                    break;

                //--3:可走读、寄宿   => lodging=true && sdextern=true
                case "3":
                    dic.Add("lodging", true);
                    dic.Add("sdextern", true);
                    break;

                default:
                    dic.Add("lodging", null);
                    dic.Add("sdextern", null);
                    break;
            }
            return dic;
        }
        #endregion

        public static bool? StringToBollen(string value) 
        {
            if (value == "false") { return false; }
            else return true;
        }


        #region 师生比例，外教占比
        #region 旧算法
        /// <summary>
        /// 1:N模式，求N
        /// 1、分母/分子=result
        /// 2、result四舍五入，返回整数部分
        /// </summary>
        /// <param name="molecule">分子</param>
        /// <param name="denominator">分母</param>
        /// <returns></returns>
        public static int? GetIntByRounding(float? molecule, float? denominator)
        {
            int? result = null;
            if (molecule != null && molecule > 0 && denominator != null)
            {
                var num = denominator / (double)molecule;
                var intNum = Math.Truncate((double)num);
                var xsNum = num % 1;
                if (xsNum >= 0.5)
                {
                    ++intNum;
                }
                result = (int?)intNum;
            }
            return result;
        }
        #endregion


        /// <summary>
        /// 1:N模式，求N
        /// 1、分母/分子=result
        /// 2、result四舍五入，返回整数部分
        /// </summary>
        /// <param name="molecule">分子</param>
        /// <param name="denominator">分母</param>
        /// <returns></returns>
        public static float? GetFloatByRounding(float? molecule, float? denominator)
        {
            float? result = null;
            if (molecule != null && molecule > 0 && denominator != null)
            {
                var num = (decimal)denominator / (decimal)molecule;
                var xsNum = num % 1;
                if (num <= (decimal)0.04)
                {
                    return 0;
                }
                else if (xsNum == 0)//整数
                {
                    return (float)Math.Truncate(num);
                }
                else if (xsNum >= (decimal)0.95)//整数
                {
                    return (float)Math.Truncate(++num);
                }
                else
                {
                    if (xsNum * 10 % 1 >= (decimal)0.5)
                    {
                        var n = num + (decimal)0.01;
                        return (float)Math.Round(n, 1);
                    }
                    else
                    {
                        return (float)Math.Round(num, 1);
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// N%模式，求N
        /// 1、外教人数 除于 教师人数 =result
        /// 2、result四舍五入，返回整数部分
        /// </summary>
        /// <param name="foreignTeaCount">除数</param>
        /// <param name="teachercount">被除数</param>
        /// <returns></returns>
        public static int? GetPercentage(float? foreignTeaCount, float? teachercount)
        {
            int? result = null;
            //外教人数、教师人数非空
            if (foreignTeaCount != null && teachercount != null && teachercount > 0)
            {
                var num = foreignTeaCount / (double)teachercount * 100;
                var intNum = Math.Truncate((double)num);
                var xsNum = num % 1;
                if (xsNum >= 0.5)
                {
                    ++intNum;
                }
                result = (int?)intNum;
            }
            return result;
        }
        #endregion
    }
}
