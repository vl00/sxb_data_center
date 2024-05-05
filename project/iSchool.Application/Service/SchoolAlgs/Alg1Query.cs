using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service
{
    public class Alg1Query : IRequest<Alg1QyRstDto>
    {
        public Guid Sid { get; set; }
        public Guid Eid { get; set; }
    }

    public class ExtInfoEntity
    {
        public Guid Sid { get; set; }
        public Guid Eid { get; set; }
        public byte Grade { get; set; }
        public byte Type { get; set; }
        //是否普惠学校
        public bool? Discount { get; set; }
        //是否中国国籍学校
        public bool? Chinese { get; set; }
        //是否双语
        public bool? Diglossia { get; set; }
    }

    public class Alg1QyRstDto
    {
        public Guid Eid { get; set; }

        public ExtInfoEntity Ext { get; set; } = new ExtInfoEntity();

        public int? TeacherCount { get; set; }
        public int? FgnTeacherCount { get; set; }
        public int? UndergduateOverCount { get; set; }
        public int? GduateOverCount { get; set; }
        public KeyValueDto<int>[] TeacherHonor { get; set; } = new KeyValueDto<int>[0];

        public KeyValueDto<string>[] SubjsKvs { get; set; } = new KeyValueDto<string>[0];
        public KeyValueDto<string>[] ArtsKvs { get; set; } = new KeyValueDto<string>[0];
        public KeyValueDto<string>[] SportsKvs { get; set; } = new KeyValueDto<string>[0];
        public KeyValueDto<string>[] ScienceKvs { get; set; } = new KeyValueDto<string>[0];
    }
}
