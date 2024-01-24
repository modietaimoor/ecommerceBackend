using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace eCommerceApi.Helpers
{
    public class APIResponse
    {

        public static HttpResponseMessage EmptyHttpResponse
        {
            get
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.NoContent);
            }
        }


        public static HttpResponseMessage SuccessNoDataHttpResponse
        {
            get
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
        }

        public static HttpResponseMessage ExceptionHttpResponse(Exception ex)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.Content = new StringContent(ConstructJSONError(ex.Message.ToString()));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return response;
            }
            catch (Exception exx)
            {
                return response;
            }
        }


        public static HttpResponseMessage ErrorHttpResponse(string error)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {

                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.Content = new StringContent(ConstructJSONError(error));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return response;
            }
            catch (Exception ex)
            {
                return response;
            }
        }

        public static HttpResponseMessage SuccessDataHttpResponse(Object obj)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            };
            return response;
        }

        private static string ConstructJSONError(string error)
        {
            return "{\"err\" : \"" + error + "\"}";
        }
    }
}