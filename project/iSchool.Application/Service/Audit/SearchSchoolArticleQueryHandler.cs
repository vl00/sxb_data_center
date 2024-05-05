using Dapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Infrastructure.Dapper;
using Newtonsoft.Json.Linq;
using iSchool.Domain.Modles;
using Microsoft.Extensions.Options;
using static iSchool.Infrastructure.ObjectHelper;

namespace iSchool.Application.Service.Audit
{
    public class SearchSchoolArticleQueryHandler : IRequestHandler<SearchSchoolArticleQuery, PagedList<SearchSchoolArticleResult>>
    {
        UnitOfWork unitOfWork;
        IHttpClientFactory httpClientFactory;
        AppSettings appSettings;

        public SearchSchoolArticleQueryHandler(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory, IOptions<AppSettings> options)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.httpClientFactory = httpClientFactory;
            this.appSettings = options.Value;
        }

        public async Task<PagedList<SearchSchoolArticleResult>> Handle(SearchSchoolArticleQuery request, CancellationToken cancellationToken)
        {
            var sql = @"
select t.id,t.name from GeneralTagBind b
inner join GeneralTag t on t.id=b.tagID
where b.dataID=@Eid --and b.dataType=2
";
            var tags = await unitOfWork.DbConnection.QueryAsync<Tag>(sql, request);
            var gids = tags.Select(_ => _.Id).ToArray();

            /// 调用运营平台查询学部所关联tag的文章
            using var httpClient = httpClientFactory.CreateClient(string.Empty);
            var res = await httpClient.PostAsync($"{appSettings.OperationPlatformUrl}/api/ArticleApi?pageIndex={request.PageIndex}&pageSize={request.PageSize}",
                new StringContent(gids.ToJsonString(), Encoding.UTF8, "application/json"));
            res.EnsureSuccessStatusCode();
            var r = (await res.Content.ReadAsStringAsync()).ToObject<FnResult<JObject>>(true);
            if (!r.IsOk)
            {
                throw new FnResultException(r.Code, r.Msg);
            }

            var pg = new PagedList<SearchSchoolArticleResult>();
            pg.CurrentPageIndex = request.PageIndex;
            pg.PageSize = request.PageSize;
            pg.TotalItemCount = (int)r.Data["total"];
            pg.CurrentPageItems = Tryv(() => r.Data["items"].ToString().ToObject<SearchSchoolArticleResult[]>(true), Enumerable.Empty<SearchSchoolArticleResult>());

            return pg;
        }
    }
}
