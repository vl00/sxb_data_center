using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using MediatR;
using System.Linq;

namespace iSchool.Application.Service
{
    public class GetSchoolImageQueryHandler : IRequestHandler<GetSchoolImageQuery, List<VueUploadImgArry>>
    {
        private readonly IRepository<SchoolImage> _schoolImageRepository;
        private readonly IMapper _mapper;

        public GetSchoolImageQueryHandler(IRepository<SchoolImage> schoolImageRepository,
            IMapper mapper)
        {
            _schoolImageRepository = schoolImageRepository;
            _mapper = mapper;
        }

        public async Task<List<VueUploadImgArry>> Handle(GetSchoolImageQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var schoolImages = _schoolImageRepository.GetAll(x => request.Types.Contains(x.Type) && x.IsValid == true && x.Eid == request.Extid).OrderByDescending(x => x.Sort);
            var dto = _mapper.Map<IEnumerable<SchoolImageDto>>(schoolImages);
            var data = dto.GroupBy(x => x.Type);
            List<VueUploadImgArry> vueUploads = new List<VueUploadImgArry>();
            foreach (var item in data)
            {
                vueUploads.Add(new VueUploadImgArry()
                {
                    Name = item.Key.ToString(),
                    Data = _mapper.Map<List<VueUploadImgArryItem>>(item)
                });
            }
            return vueUploads;
        }
    }
}
