using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Course;
using MediatR;
using Microsoft.AspNetCore.Mvc;



namespace iSchool.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseBgApiController : ControllerBase
    {
        IMediator _mediator;
        public CourseBgApiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        


        // GET: api/<BgApiCourseController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<BgApiCourseController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<BgApiCourseController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<BgApiCourseController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<BgApiCourseController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
