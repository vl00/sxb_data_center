using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Application.Service.OnlineSchools;
using System.Buffers;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using iSchool.Infrastructure.Dapper;
using System.Web;

namespace iSchool.Application.Service.Searchs
{
    public class ESearchLikeQueryHandler : IRequestHandler<ESearchLikeQuery, FnResult<PagedList<Guid>>>
    {
        ILog log;
        HttpClient http;
        AppSettings appSettings;

        public ESearchLikeQueryHandler(ILog log, IHttpClientFactory httpClientFactory, AppSettings appSettings)
        {
            this.log = log;
            this.appSettings = appSettings;
            http = httpClientFactory.CreateClient(string.Empty);
        }

        public async Task<FnResult<PagedList<Guid>>> Handle(ESearchLikeQuery req, CancellationToken cancellation)
        {            
            var gid = req.Sid ?? (Guid.TryParse(req.Name, out var _gid) ? _gid : Guid.Empty);
            if (gid != Guid.Empty) req.Sid = null;
            if (!(req.Grade > 0)) req.Grade = null;
            if (!(req.Type > 0)) req.Type = null;
            if (!(req.Province > 0)) req.Province = null;
            if (!(req.City > 0)) req.City = null;
            if (!(req.Area > 0)) req.Area = null;            

            using (http)
            {
                var res = await http.SendAsync(
                    new HttpRequestMessage(HttpMethod.Post, $"{appSettings.ESearchUrl}/BGSchoolSearch/BGSearchSchool")                 
                    .SetContent(new StringContent(req.ToJsonString(ignoreNull: true), Encoding.UTF8, "application/json"))
                );
                res.EnsureSuccessStatusCode();

                var fr = (await res.Content.ReadAsStringAsync()).ToObject<FnResult<ESearchLikeApiResult>>();
                if (!fr.IsOk) return FnResult.Fail<PagedList<Guid>>("调用es出错", 400);

                return FnResult.OK(new PagedList<Guid>
                {
                    CurrentPageIndex = req.PageIndex,
                    PageSize = req.PageSize,
                    TotalItemCount = (int)fr.Data.Total,
                    CurrentPageItems = fr.Data.Sid,
                });
            }
        }
        //public async Task<FnResult<PagedList<Guid>>> Handle(ESearchLikeQuery req, CancellationToken cancellation)
        //{
        //    await default(ValueTask);
        //    return FnResult.OK(new PagedList<Guid>
        //    {
        //        CurrentPageIndex = req.PageIndex,
        //        PageSize = req.PageSize,
        //        TotalItemCount = 100,
        //        CurrentPageItems = new[]
        //        {
        //            Guid.Parse("6ba5b4e8-6187-42e2-9be8-4addcbb1bb3d"),
        //            Guid.Parse("43eaf35a-b081-4505-ada1-c04013f2d096"),
        //            Guid.Parse("d5b7fa24-04f1-49a9-9fa0-a8c299c99d7d"),
        //            Guid.Parse("be1f4679-690c-41a5-bf90-0fb8832f61ad"),
        //            Guid.Parse("00ff6e79-2ce7-4da3-87cb-f418da4ea2a0"),
        //            Guid.Parse("2df9992d-b5dc-4a55-9045-3fe68bc036d1"),
        //            Guid.Parse("9b05ffb5-0bd0-49ff-a05f-9f9fbefe50d4"),
        //            Guid.Parse("054f8fcc-5a87-4f68-9133-0d16f7633185"),
        //            Guid.Parse("7984285a-8dd7-4bf1-9174-080bec3469ad"),
        //            Guid.Parse("06b4e29f-5903-4e90-ac10-95fcfe796167"),
        //        },
        //    });
        //}
    }
}
