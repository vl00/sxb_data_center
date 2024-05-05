using iSchool.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool
{
    /// <summary>
    /// 跳过mvc filters
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AllowGoThroughMvcFilterAttribute : Attribute, IGoThroughMvcFilter { }
}
