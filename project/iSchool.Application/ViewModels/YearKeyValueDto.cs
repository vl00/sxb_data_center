using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    public class YearKeyValueDto<T>
    {
        public int Year { get; set; }
        public string Key { get; set; }
        public T Value { get; set; }        
    }
}
