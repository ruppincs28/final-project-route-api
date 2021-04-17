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
    public class TestsController : ApiController
    {
        [HttpPost]
        [Route("api/tests/place_data")]
        public IHttpActionResult RetrievePlaceData([FromBody]ClientCompaniesWithAddresses ccwaRes)
        {
            try
            {
                string API_KEY = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["GOOGLE_API_KEY"]);
                List<ReturnModel> resultList = new List<ReturnModel>();

                foreach (string ccwa in ccwaRes.CompaniesWithAddresses)
                {
                    string companyNameWithStateEscaped = Uri.EscapeUriString(ccwa);

                    // Extract exact address from company + state combination
                    string json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/place/findplacefromtext/json?" +
                        $"input={companyNameWithStateEscaped}&inputtype=textquery&language=en-US&fields=all&key={API_KEY}");
                    string exactAddress = ReturnModel.ExtractFormattedAddress(json);
                    string exactAddressEscaped = Uri.EscapeUriString(exactAddress);

                    // REMOVE THIS
                    // Extract latitude, longitude from an address - redundant, we have these fields in the response above
                    json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/geocode/json?" +
                        $"address={exactAddressEscaped}&key={API_KEY}");
                    Coordinate coordinate = ReturnModel.ExtractLatLng(json);

                    // Get Route
                    json = HTTPHelpers.SynchronizedRequest("GET", $"https://maps.googleapis.com/maps/api/directions/json" +
                        $"?origin=32.1774678,34.8554012&destination={coordinate.Lat},{coordinate.Lng}&mode=transit&key={API_KEY}&language=en-US");
                    string steps = ReturnModel.ExtractSteps(json);

                    ReturnModel route = new ReturnModel(exactAddress, coordinate, steps);
                    resultList.Add(route);
                }

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