using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{

    //添加普通高中
    [Serializable]
    public class AddHighSchoolAchCommand : IRequest<HttpResponse<string>>
    {
        public int Year { get; set; }
        public Guid ExtId { get; set; }
        public double? Keyundergraduate { get; set; }
        public double? Undergraduate { get; set; }
        public int? Count { get; set; }

        public string[] Name { get; set; }

        public string[] Point { get; set; }

        public float? Completion { get; set; }
        public Guid? UserId { get; set; }
    }

    //添加初中
    [Serializable]
    public class AddMiddleSchoolAchCommand : IRequest<HttpResponse<string>>
    {
        public AddMiddleSchoolAchCommand()
        {
        }

        public AddMiddleSchoolAchCommand(double? keyrate, double? average, double? highest, double? ratio, double? completion, int year, Guid extId)
        {
            Keyrate = keyrate;
            Average = average;
            Highest = highest;
            Ratio = ratio;
            Completion = completion;
            Year = year;
            ExtId = extId;
        }

        public double? Keyrate { get; set; }
        /// <summary>
        /// 平均分
        /// </summary>
        public double? Average { get; set; }
        /// <summary>
        /// 当年最高分
        /// </summary>
        public double? Highest { get; set; }

        /// <summary>
        /// 高优线录取比例
        /// </summary>
        public double? Ratio { get; set; }
        /// <summary>
        /// 完成率
        /// </summary>

        public double? Completion { get; set; }

        public int Year { get; set; }
        public Guid ExtId { get; set; }

        public Guid UserId { get; set; }
    }

    //添加小学
    [Serializable]
    public class AddPrimarySchoolAchCommand : IRequest<HttpResponse<string>>
    {

        public List<string> Link { get; set; }
        public Guid ExtId { get; set; }
        public int Year { get; set; }
        public Guid? UserId { get; set; }
    }

    //添加幼儿园
    [Serializable]
    public class AddKindergartenSchoolAchCommand : IRequest<HttpResponse<string>>
    {

        public List<string> Link { get; set; }
        public Guid ExtId { get; set; }
        public int Year { get; set; }
        public Guid? UserId { get; set; }
    }

}
