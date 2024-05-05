using CSRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Appliaction.CommonHelper
{
    /// <summary>
    /// 
    /// </summary>
    internal static class CSRedisClientHelper
    {
        #region batch del normal key or key with pattern
        /**
         *   await redis.BatchDelAsync(new[] { "a:b:*", "c:*:d" }, 10);
         */

        public static Task BatchDelAsync(this CSRedisClient redis, string key, int timeoutSeconds = -1)
            => BatchDelAsync(redis, Enumerable.Repeat(key, 1), timeoutSeconds);

        public static Task BatchDelAsync(this CSRedisClient redis, IEnumerable<string> keys, int timeoutSeconds = -1)
            => BatchDelAsync(redis, keys, timeoutSeconds == -1 ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(timeoutSeconds));

        public static async Task BatchDelAsync(this CSRedisClient redis, IEnumerable<string> keys, TimeSpan timeout)
        {
            var cts = timeout == Timeout.InfiniteTimeSpan ? default : new CancellationTokenSource(timeout).Token;
            await foreach (var ks in Split(ScanRdlKeys(redis, keys, -1, cts), 500))
            {
                try
                {
                    using var pipe = redis.StartPipe();
                    foreach (var k in ks)
                        pipe.Del(k);
                    await pipe.EndPipeAsync();
                }
                catch
                {
                    // ignore
                }
            }
        }

        static async IAsyncEnumerable<IEnumerable<T>> Split<T>(IAsyncEnumerable<T> arr, int len)
        {
            List<T> ls = null;
            await foreach (var k in arr)
            {
                ls ??= new List<T>(len);
                ls.Add(k);
                if (ls.Count < len) continue;
                yield return ls;
                ls.Clear();
            }
            if (ls?.Count > 0)
            {
                yield return ls;
            }
        }

        public static IAsyncEnumerable<string> ScanRdlKeys(this CSRedisClient redis, IEnumerable<string> keysOrPatterns, CancellationToken cancellation = default)
            => ScanRdlKeys(redis, keysOrPatterns, null, cancellation);

        static async IAsyncEnumerable<string> ScanRdlKeys(this CSRedisClient redis, IEnumerable<string> keysOrPatterns, int? max4ScanEmpty,
            [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            foreach (var keyP in keysOrPatterns)
            {
                if (string.IsNullOrEmpty(keyP) || string.IsNullOrWhiteSpace(keyP))
                {
                    continue;
                }
                if (!keyP.Contains('*'))
                {
                    yield return keyP;
                    continue;
                }
                var cursor = 0L;
                int i_err = 0, i_scan = 0, i_max4ScanEmpty = (max4ScanEmpty ?? -1);
                while (!cancellation.IsCancellationRequested)
                {
                    string[] ks;
                    try
                    {
                        var scan = await redis.ScanAsync(cursor, keyP, 1000);
                        cursor = scan.Cursor;
                        ks = scan.Items;
                        i_err = 0;
                    }
                    catch  // ignore error
                    {
                        if ((i_err++) >= 2) break;
                        else continue;
                    }
                    if (ks?.Length > 0)
                    {
                        i_scan = 0;
                        foreach (var k in ks)
                            yield return k;
                    }
                    if (cursor <= 0 || (i_max4ScanEmpty != -1 && (i_scan++) >= i_max4ScanEmpty)) // else if
                    {
                        break;
                    }
                }
            }
        }

        #endregion

        #region batch del hash-key field or with field pattern        
        /**
         *   await redis.BatchDelAsync(new[] { 
         *      ("a:b:*", null), 
         *      ("c:*:d", "huhu|*"),
         *   }, 10);
         */

        public static Task BatchDelAsync(this CSRedisClient redis, (string Key, string Field) k, int timeoutSeconds = -1)
            => BatchDelAsync(redis, Enumerable.Repeat(k, 1), timeoutSeconds);

        public static Task BatchDelAsync(this CSRedisClient redis, IEnumerable<(string Key, string Field)> ks, int timeoutSeconds = -1)
            => BatchDelAsync(redis, ks, timeoutSeconds == -1 ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(timeoutSeconds));

        public static async Task BatchDelAsync(this CSRedisClient redis, IEnumerable<(string Key, string Field)> ks, TimeSpan timeout)
        {
            var cts = timeout == Timeout.InfiniteTimeSpan ? default : new CancellationTokenSource(timeout).Token;
            await foreach (var kfs in Split(ScanRdlHKeys(redis, ks, (-1, -1), cts), 500))
            {
                try
                {
                    using var pipe = redis.StartPipe();
                    foreach (var x in kfs)
                    {
                        if (x.Item2 == null) pipe.Del(x.Item1);
                        else pipe.HDel(x.Item1, x.Item2);
                    }
                    await pipe.EndPipeAsync();
                }
                catch
                {
                    // ignore
                }
            }
        }

        public static IAsyncEnumerable<(string Key, string Field)> ScanRdlHKeys(this CSRedisClient redis, IEnumerable<(string, string)> hkeyfieldsOrPatterns, CancellationToken cancellation = default)
            => ScanRdlHKeys(redis, hkeyfieldsOrPatterns, null, cancellation);

        static async IAsyncEnumerable<(string Key, string Field)> ScanRdlHKeys(this CSRedisClient redis, IEnumerable<(string, string)> hkeyfieldsOrPatterns, (int, int)? max4ScanEmpty = null,
            [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            string[] k1s = null;
            foreach (var (keyP, fieldP) in hkeyfieldsOrPatterns)
            {
                if (string.IsNullOrEmpty(keyP) || string.IsNullOrWhiteSpace(keyP))
                {
                    continue;
                }
                var is_nf = string.IsNullOrEmpty(fieldP) || fieldP == "*" ? (bool?)null : !fieldP.Contains('*');
                var is_nk = !keyP.Contains('*');
                if (is_nk && is_nf != false)
                {
                    yield return (keyP, is_nf == null ? null : fieldP);
                    continue;
                }
                k1s ??= new string[1];
                k1s[0] = keyP;
                await foreach (var k0 in ScanRdlKeys(redis, k1s, (max4ScanEmpty?.Item1 ?? -1), cancellation))
                {
                    if (is_nf != false)
                    {
                        yield return (k0, is_nf == null ? null : fieldP);
                    }
                    else
                    {
                        var cursor = 0L;
                        int i_err = 0, i_scan = 0, i_max4ScanEmpty = (max4ScanEmpty?.Item2 ?? -1);
                        while (!cancellation.IsCancellationRequested)
                        {
                            (string, string)[] fs;
                            try
                            {
                                var scan = await redis.HScanAsync(k0, cursor, fieldP, 100);
                                cursor = scan.Cursor;
                                fs = scan.Items;
                                i_err = 0;
                            }
                            catch  // ignore error
                            {
                                if ((i_err++) >= 2) break;
                                else continue;
                            }
                            if (fs?.Length > 0)
                            {
                                i_scan = 0;
                                foreach (var x in fs)
                                    yield return (k0, x.Item1);
                            }
                            if (cursor <= 0 || (i_max4ScanEmpty != -1 && (i_scan++) >= i_max4ScanEmpty)) // else if 
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
