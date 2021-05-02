using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using final_project_route_api.Models;
using final_project_route_api.Models.HTTP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace final_project_route_api.Controllers
{
    public class RoutesController : ApiController
    {
        [HttpPost]
        [Route("api/routes/calculate")]
        public IHttpActionResult RetrievePlaceData([FromBody]ClientRouteCalculatorRequest rcr)
        {
            try
            {
                List<Route> resultList = RouteCalculator.Calculate(rcr);
                return Ok(resultList);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
                //return BadRequest(ex.Message);
            }
        }

        //public IHttpActionResult Post([FromBody] Category cat)
        //{
        //    try
        //    {
        //        Category.Insert(cat);
        //        return Ok("Added successfully");
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }
        //}
    }
}