using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using iSchool.Infrastructure.Cache;
using iSchool.Application.ViewModels;

namespace iSchool.Application.Service
{
    public class KvsQueryHandler : IRequestHandler<KvsQuery, KeyValueDto<string>[]>
    {
        const string prev_key = @"kvsquery";
        const double _mlen = 0.5 * 1024 * 1024;
        static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        UnitOfWork unitOfWork;
        CacheManager cacheManager;
        ILog log;
        IMemoryCache memoryCache;

        public KvsQueryHandler(IUnitOfWork unitOfWork, CacheManager cacheManager, ILog log, IMemoryCache memoryCache)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.cacheManager = cacheManager;
            this.log = log;
            this.memoryCache = memoryCache;
        }

        public async Task<KeyValueDto<string>[]> Handle(KvsQuery req, CancellationToken cancellationToken)
        {
            KeyValueDto<string>[] ress = null;
            string json = null;
            string[] ss = null;
            int c = 0;

            var _key = $"{prev_key}/{req.Type}/p{req.ParentId}";

            ress = memoryCache.Get<KeyValueDto<string>[]>(CacheManager.StrToMD5(_key));
            if (ress != null) return ress;

            try
            {
                //c = await retry_to_do(() => Convert.ToInt32(cacheManager.GetStr($"{_key}_count") ?? "0"), 2, 500);

                //IEnumerable<Task<string>> do_read_cache()
                //{
                //    for (var i = 0; i < c; i++)
                //    {
                //        var _i = i;
                //        yield return retry_to_do(() => cacheManager.GetStr($"{_key}_{_i}"), 2, 500);
                //    }
                //}
                //ss = await Task.WhenAll(do_read_cache()).ConfigureAwait(false);
                //json = string.Join("", ss);

                //if (!string.IsNullOrEmpty(json))
                //{
                //    return ress = json.ToObject<KeyValueDto<string>[]>(true);
                //}
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);

                ress = memoryCache.Get<KeyValueDto<string>[]>(CacheManager.StrToMD5(_key));
                if (ress != null) return ress;

                var sql = @"select * from [dbo].[KeyVals] where [type]=@Type and parentid=@ParentId and IsValid=1";

                ress = (await unitOfWork.DbConnection.QueryAsync<KeyVals>(sql, req).ConfigureAwait(false))
                    .Select(_ => new KeyValueDto<string> { Key = _.Name, Value = _.Id, Message = _.Parentid })
                    .ToArray();

                //memoryCache.Set(CacheManager.StrToMD5(_key), ress, TimeSpan.FromMinutes(15));
            }
            finally
            {
                _semaphore.Release();
            }

            try
            {
                json = ress.ToJsonString();
                ss = split_str(json, out c);

                //IEnumerable<Task> do_write_cache()
                //{
                //    for (var i = 0; i < c; i++)
                //    {
                //        var _i = i;
                //        yield return retry_to_do(() => cacheManager.Add($"{_key}_{_i}", ss[_i]), 2, 500);
                //    }
                //    yield return retry_to_do(() => cacheManager.Add($"{_key}_count", c.ToString()), 2, 500);
                //}
                //await Task.WhenAll(do_write_cache()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return ress;
        }

        async Task<T> retry_to_do<T>(Func<T> func, int c, int ms)
        {
            T res = default;

            async Task _f()
            {
                c = c < 1 ? 1 : c;
                for (var i = 1; i <= c; i++)
                {
                    try
                    {
                        res = func();
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (i == c) throw ex;
                        else await Task.Delay(ms).ConfigureAwait(false);
                    }
                }
            }

            await _f().ConfigureAwait(false);
            return res;
        }

        static string[] split_str(string str, out int c)
        {
            c = (int)Math.Ceiling(str.Length / _mlen);
            var n = (int)_mlen;

            var x = 0;
            var ss = new string[c];
            for (var i = 0; i < c; i++)
            {
                if (i == c - 1) ss[i] = str.Substring(x);
                else ss[i] = str.Substring(x, n);
                x += n;
            }

            return ss;
        }
    }
}
