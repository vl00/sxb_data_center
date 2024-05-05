using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using MediatR;


namespace iSchool.Application.Service
{
    public class GetSchoolImageQuery : IRequest<List<VueUploadImgArry>>
    {
        public GetSchoolImageQuery(Guid sid, Guid extid, List<int> types)
        {
            Sid = sid;
            Types = types;
            Extid = extid;
        }

        public Guid Sid { get; set; }

        public Guid Extid { get; set; }

        public List<int> Types { get; set; }
    }
}
