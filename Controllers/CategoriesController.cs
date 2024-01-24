using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using eCommerceApi.DAL;
using eCommerceApi.Helpers;
using eCommerceApi.Models;
using Newtonsoft.Json;

namespace eCommerceApi.Controllers
{
    [RoutePrefix("api/Categories")]
    public class CategoriesController : ApiController
    {
        [HttpGet]
        [Route("GetAllCategories")]
        public HttpResponseMessage GetAllCategories()
        {
            try
            {
                eCommerceDataContext dbContext = new eCommerceDataContext();
                var categories = dbContext.Categories.Select(x => new {
                    x.CategoryID,
                    x.CategoryName,
                    CategoryImage = x.CategoryImage.ToArray()
                }).ToList();
                return APIResponse.SuccessDataHttpResponse(categories);
            }
            catch (Exception ex)
            {
                return APIResponse.ExceptionHttpResponse(ex);
            }
        }

        [HttpGet]
        [Route("DeleteCategory")]
        public HttpResponseMessage DeleteCategory(int id)
        {
            try
            {
                eCommerceDataContext dbContext = new eCommerceDataContext();
                Category cat = dbContext.Categories.SingleOrDefault(x => x.CategoryID == id);
                if (cat != null)
                {
                    dbContext.Categories.DeleteOnSubmit(cat);
                    dbContext.SubmitChanges();
                }
                return APIResponse.SuccessNoDataHttpResponse;
            }
            catch (Exception ex)
            {
                return APIResponse.ExceptionHttpResponse(ex);
            }
        }

        [HttpPost]
        [Route("AddUpdateCategory")]
        public HttpResponseMessage AddUpdateCategory()
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(HttpContext.Current.Request.Form["category"]);
                int categoryID = json.CategoryID;
                string categoryName = json.CategoryName;
                byte[] imgBytes = null;
                if (HttpContext.Current.Request.Files != null && HttpContext.Current.Request.Files.Count > 0)
                {
                    HttpPostedFile imgFile = HttpContext.Current.Request.Files[0];
                    string fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Areas/" + categoryName + ".jpg"));
                    imgFile.SaveAs(fileSavePath);
                    imgBytes = File.ReadAllBytes(fileSavePath);
                    if (File.Exists(fileSavePath))
                    {
                        File.Delete(fileSavePath);
                    }
                }
                eCommerceDataContext dbContext = new eCommerceDataContext();
                if (categoryID == 0)
                {
                    dbContext.Categories.InsertOnSubmit(new Category
                    {
                        CategoryName = categoryName,
                        CategoryImage = imgBytes
                    });
                }
                else
                {
                    Category oldCat = dbContext.Categories.SingleOrDefault(x => x.CategoryID == categoryID);
                    if (oldCat != null)
                    {
                        oldCat.CategoryName = categoryName;
                    }
                }
                dbContext.SubmitChanges();
                return APIResponse.SuccessNoDataHttpResponse;
            }
            catch (Exception ex)
            {
                return APIResponse.ExceptionHttpResponse(ex);
            }
        }
    }
}