using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using eCommerceApi.DAL;
using eCommerceApi.Helpers;
using eCommerceApi.Models;

namespace eCommerceApi.Controllers
{
    [RoutePrefix("api/Login")]
    public class LoginController : ApiController
    {
        [HttpPost]
        [Route("LoginUser")]
        public HttpResponseMessage LoginUser(LoginModel lm)
        {
            try
            {
                eCommerceDataContext dbContext = new eCommerceDataContext();
                User user = dbContext.Users.SingleOrDefault(x=> (x.Email == lm.name || x.PhoneNumber == lm.name) && x.Password == lm.password);
                return APIResponse.SuccessDataHttpResponse(user);
            }
            catch (Exception ex)
            {
                return APIResponse.ExceptionHttpResponse(ex);
            }
        }

        [HttpGet]
        [Route("GetLogin")]
        public HttpResponseMessage GetLogin(string name, string password)
        {
            try
            {
                eCommerceDataContext dbContext = new eCommerceDataContext();
                User user = dbContext.Users.SingleOrDefault(x => (x.Email == name || x.PhoneNumber == name) && x.Password == password);
                return APIResponse.SuccessDataHttpResponse(user);
            }
            catch (Exception ex)
            {
                return APIResponse.ExceptionHttpResponse(ex);
            }
        }
    }
}