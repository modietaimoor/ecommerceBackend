using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eCommerceApi.Models
{
    public class CategoryModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public byte[] CategoryImage { get; set; }
    }
}