using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace iSchool.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TestController : ControllerBase
    {
        static void IfTrue(bool _b)
        {
            if (!_b) throw new Exception();
        }

        [HttpGet]
        public IActionResult Get()
        {
            var r1 = FnResult.OK(new { s1 = "s1", s2 = new[] { 1, 2, 3 } });
            var r2 = FnResult.Fail("err2", 10000);

            var options = new JsonSerializerSettings();
            options.ContractResolver = new CamelCasePropertyNamesContractResolver();
            options.Converters.Add(new FnResultJsonConverter());

            var s1 = JsonConvert.SerializeObject(r1, options);
            var s2 = JsonConvert.SerializeObject(r2, options);
            var d1 = JsonConvert.DeserializeObject<FnResult<object>>(s1, options);
            var d2 = JsonConvert.DeserializeObject<FnResult<object>>(s2, options);
            var dnull = JsonConvert.DeserializeObject<FnResult<object>>("null", options);
            return new EmptyResult();
        }

        //[HttpDelete(nameof(Error))]
        //[AllowGoThroughMvcFilter]
        //public IActionResult Error() => throw new Exception("test throw error");

        [HttpGet(nameof(ErrorOnBg))]
        public async Task<IActionResult> ErrorOnBg()
        {
            await Task.Run(async () => 
            {
                await default(ValueTask);
                throw new Exception("this is a error on Task.Run() ...");
            });
            return Ok(1);
        }

        /// <summary>
        /// 调试时模拟后台探针接收
        /// </summary>
        [HttpPost("Bgstatistic")]
        public async Task<IFnResult> Bgstatistic(JToken body)
        {
            await default(ValueTask);
            Console.WriteLine(body.ToString());
            return FnResult.OK(body);
        }

        

    }
}
