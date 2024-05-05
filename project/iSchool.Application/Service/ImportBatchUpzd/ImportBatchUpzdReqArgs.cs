using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.ImportBatchUpzd
{
    public class ImportBatchUpzdReqArgs : IRequest<ImportBatchUpzdResResult>
    {
        public SearchArgs Search { get; set; } = null!;
        public class SearchArgs
        {
            public int PageIndex { get; set; }
            public int PageSize { get; set; }
        }
    }

    public class ImportBatchUpzdResResult
    {
        public PagedList<SearchItem> SearchResult { get; set; } = null!;
        public class SearchItem
        {
			public string Huid { get; set; }
            public string id => Huid;

            public string Xfile { get; set; }

			public string Ver { get; set; }

			public string Stage { get; set; }

			/// <summary>
			/// 1=等待中;2=进行中;3=已成功;4=有错;5=已取消;
			/// </summary> 
			public int Status { get; set; }

			public string CreateTime { get; set; } 

			public string UpdateTime { get; set; }

			public string Errs { get; set; }

			public string User { get; set; }
		}
    }
}
