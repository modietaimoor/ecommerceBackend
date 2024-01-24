using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ClosedXML.Excel;
using eCommerceApi.DAL;
using eCommerceApi.Helpers;
using Newtonsoft.Json;

namespace eCommerceApi.Controllers
{
    [RoutePrefix("api/Products")]
    public class ProductsController : ApiController
    {
        [HttpGet]
        [Route("GetAllProducts")]
        public HttpResponseMessage GetAllProducts()
        {
            try
            {
                eCommerceDataContext dbContext = new eCommerceDataContext();
                var products = (from prod in dbContext.Products
                                join brand in dbContext.Brands on prod.BrandID equals brand.BrandID
                                join cat in dbContext.Categories on prod.CategoryID equals cat.CategoryID
                                select new
                                {
                                    prod.ProductID,
                                    prod.ProductName,
                                    prod.ProductDescription,
                                    prod.QuantityInStore,
                                    prod.ProductPrice,
                                    prod.BrandID,
                                    brand.BrandName,
                                    prod.CategoryID,
                                    cat.CategoryName,
                                    ProductImage = prod.ProductImage.ToArray()
                                }).ToList();
                return APIResponse.SuccessDataHttpResponse(products);
            }
            catch (Exception ex)
            {
                return APIResponse.ExceptionHttpResponse(ex);
            }
        }

        [HttpGet]
        [Route("DeleteProduct")]
        public HttpResponseMessage DeleteProduct(int id)
        {
            try
            {
                eCommerceDataContext dbContext = new eCommerceDataContext();
                Product prod = dbContext.Products.SingleOrDefault(x => x.ProductID == id);
                if (prod != null)
                {
                    dbContext.Products.DeleteOnSubmit(prod);
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
        [Route("AddUpdateProduct")]
        public HttpResponseMessage AddUpdateProduct()
        {
            try
            {
                eCommerceDataContext dbContext = new eCommerceDataContext();
                dynamic json = JsonConvert.DeserializeObject(HttpContext.Current.Request.Form["product"]);
                int productID = json.ProductID;
                string productName = json.ProductName;
                int brandID = json.BrandID;
                string productDescription = json.ProductDescription;
                int categoryID = json.CategoryID;
                decimal productPrice = json.ProductPrice;
                int quantityInStore = json.QuantityInStore;
                byte[] imgBytes = null;
                if (HttpContext.Current.Request.Files != null && HttpContext.Current.Request.Files.Count > 0)
                {
                    HttpPostedFile imgFile = HttpContext.Current.Request.Files[0];
                    string fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Areas/" + productName + ".jpg"));
                    imgFile.SaveAs(fileSavePath);
                    imgBytes = File.ReadAllBytes(fileSavePath);
                    if (File.Exists(fileSavePath))
                    {
                        File.Delete(fileSavePath);
                    }
                }
                if (productID == 0)
                {
                    dbContext.Products.InsertOnSubmit(new Product
                    {
                        ProductName = productName,
                        ProductDescription = productDescription,
                        ProductPrice = productPrice,
                        QuantityInStore = quantityInStore,
                        CategoryID = categoryID,
                        BrandID = brandID,
                        CreationDate = DateTime.Now,
                        ProductImage = imgBytes
                    });
                }
                else
                {
                    Product oldProd = dbContext.Products.SingleOrDefault(x => x.ProductID == productID);
                    if (oldProd != null)
                    {
                        oldProd.ProductName = productName;
                        oldProd.ProductDescription = productDescription;
                        oldProd.ProductPrice = productPrice;
                        oldProd.QuantityInStore = quantityInStore;
                        oldProd.CategoryID = categoryID;
                        oldProd.BrandID = brandID;
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

        [HttpPost]
        [Route("ProductsFromExcel")]
        public HttpResponseMessage ProductsFromExcel()
        {
            try
            {
                if (HttpContext.Current.Request.Files != null && HttpContext.Current.Request.Files.Count > 0)
                {
                    eCommerceDataContext dbContext = new eCommerceDataContext();
                    HttpPostedFile imgFile = HttpContext.Current.Request.Files[0];
                    string fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Areas/test.xlsx"));
                    imgFile.SaveAs(fileSavePath);
                    DataTable dt = ImportExceltoDatatable(fileSavePath);
                    if (File.Exists(fileSavePath))
                    {
                        File.Delete(fileSavePath);
                    }
                    List<Product> products = new List<Product>();
                    Product prod;
                    foreach (DataRow dr in dt.Rows)
                    {
                        prod = new Product();
                        prod.ProductName = dr["ProductName"].ToString();
                        prod.CategoryID = int.Parse(dr["CategoryID"].ToString());
                        prod.BrandID = int.Parse(dr["BrandID"].ToString());
                        prod.QuantityInStore = int.Parse(dr["QuantityInStore"].ToString());
                        prod.ProductPrice = decimal.Parse(dr["ProductPrice"].ToString());
                        prod.ProductDescription = dr["ProductDescription"].ToString();
                        prod.CreationDate = DateTime.Now;
                        products.Add(prod);
                    }

                    var oldProds = dbContext.Products.ToList();
                    dbContext.Products.DeleteAllOnSubmit(oldProds);
                    dbContext.SubmitChanges();

                    dbContext.Products.InsertAllOnSubmit(products);
                    dbContext.SubmitChanges();
                }
                return APIResponse.SuccessNoDataHttpResponse;
            }
            catch (Exception ex)
            {
                return APIResponse.ExceptionHttpResponse(ex);
            }
        }

        public DataTable ImportExceltoDatatable(string filePath)
        {
            // Open the Excel file using ClosedXML.
            // Keep in mind the Excel file cannot be open when trying to read it
            using (XLWorkbook workBook = new XLWorkbook(filePath))
            {
                //Read the first Sheet from Excel file.
                IXLWorksheet workSheet = workBook.Worksheet(1);

                //Create a new DataTable.
                DataTable dt = new DataTable();

                //Loop through the Worksheet rows.
                bool firstRow = true;
                foreach (IXLRow row in workSheet.Rows())
                {
                    //Use the first row to add columns to DataTable.
                    if (firstRow)
                    {
                        foreach (IXLCell cell in row.Cells())
                        {
                            dt.Columns.Add(cell.Value.ToString());
                        }
                        firstRow = false;
                    }
                    else
                    {
                        //Add rows to DataTable.
                        dt.Rows.Add();
                        int i = 0;

                        foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))
                        {
                            dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                            i++;
                        }
                    }
                }

                return dt;
            }
        }
    }
}