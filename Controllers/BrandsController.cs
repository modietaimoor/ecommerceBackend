using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using eCommerceApi.DAL;
using eCommerceApi.Helpers;
using Newtonsoft.Json;

namespace eCommerceApi.Controllers
{
    [RoutePrefix("api/Brands")]
    public class BrandsController : ApiController
    {
        [HttpGet]
        [Route("GetAllBrands")]
        public HttpResponseMessage GetAllBrands()
        {
            try
            {
                eCommerceDataContext dbContext = new eCommerceDataContext();
                var brands = dbContext.Brands.Select(x => new {
                    x.BrandID,
                    x.BrandName,
                    BrandImage = x.BrandImage.ToArray()
                }).ToList();
                return APIResponse.SuccessDataHttpResponse(brands);
            }
            catch (Exception ex)
            {
                return APIResponse.ExceptionHttpResponse(ex);
            }
        }

        [HttpGet]
        [Route("DeleteBrand")]
        public HttpResponseMessage DeleteBrand(int id)
        {
            try
            {
                eCommerceDataContext dbContext = new eCommerceDataContext();
                Brand brd = dbContext.Brands.SingleOrDefault(x => x.BrandID == id);
                if (brd != null)
                {
                    dbContext.Brands.DeleteOnSubmit(brd);
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
        [Route("AddUpdateBrand")]
        public HttpResponseMessage AddUpdateBrand()
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(HttpContext.Current.Request.Form["brand"]);
                int brandID = json.BrandID;
                string brandName = json.BrandName;
                byte[] imgBytes = null;
                if (HttpContext.Current.Request.Files != null && HttpContext.Current.Request.Files.Count > 0)
                {
                    HttpPostedFile imgFile = HttpContext.Current.Request.Files[0];
                    string fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Areas/" + brandName + ".jpg"));
                    imgFile.SaveAs(fileSavePath);
                    imgBytes = File.ReadAllBytes(fileSavePath);
                    if (File.Exists(fileSavePath))
                    {
                        File.Delete(fileSavePath);
                    }
                }
                eCommerceDataContext dbContext = new eCommerceDataContext();
                if (brandID == 0)
                {
                    dbContext.Brands.InsertOnSubmit(new Brand
                    {
                        BrandName = brandName,
                        BrandImage = imgBytes
                    });
                }
                else
                {
                    Brand oldBr = dbContext.Brands.SingleOrDefault(x => x.BrandID == brandID);
                    if (oldBr != null)
                    {
                        oldBr.BrandName = brandName;
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