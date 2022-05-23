using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using VOL.Core.Filters;

namespace VOL.WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [JWTAuthorize, ApiController]
    public class FastreportTestController : Controller
    {

        private readonly IWebHostEnvironment _hostingEnvironment;
        public FastreportTestController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        private static List<Category> businessObjects;
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {

            string webRootPath = _hostingEnvironment.WebRootPath;
            CreateBusinessObject();
            Report report = new Report();
            report.Load(webRootPath + "\\Report\\report.frx");
            report.RegisterData(businessObjects, "Categories");
            report.Prepare();
            PDFSimpleExport export = new PDFSimpleExport();
            MemoryStream stream = new MemoryStream();
            report.Export(export, stream);
            var t = stream.GetBuffer();
            stream.Close();
            report.Dispose();

            Response.Headers.Add("Cache-Control", "max-age=31536000, must-revalidate");
            Response.Headers.Add("accept-ranges", "bytes");
            return File(t, "application/pdf");


        }

        private static void CreateBusinessObject()
        {
            businessObjects = new List<Category>();

            Category category = new Category("Beverages", "Soft drinks, coffees, teas, beers");
            category.Products.Add(new Product("Chai", 18m));
            category.Products.Add(new Product("Chang", 19m));
            category.Products.Add(new Product("Ipoh coffee", 46m));
            businessObjects.Add(category);

            category = new Category("Confections", "Desserts, candies, and sweet breads");
            category.Products.Add(new Product("Chocolade", 12.75m));
            category.Products.Add(new Product("Scottish Longbreads", 12.5m));
            category.Products.Add(new Product("Tarte au sucre", 49.3m));
            businessObjects.Add(category);

            category = new Category("Seafood", "Seaweed and fish");
            category.Products.Add(new Product("Boston Crab Meat", 18.4m));
            category.Products.Add(new Product("Red caviar", 15m));
            businessObjects.Add(category);
        }

        public class Category
        {
            private string FName;
            private string FDescription;
            private List<Product> FProducts;

            public string Name
            {
                get { return FName; }
            }

            public string Description
            {
                get { return FDescription; }
            }

            public List<Product> Products
            {
                get { return FProducts; }
            }

            public Category(string name, string description)
            {
                FName = name;
                FDescription = description;
                FProducts = new List<Product>();
            }
        }

        public class Product
        {
            private string FName;
            private decimal FUnitPrice;

            public string Name
            {
                get { return FName; }
            }

            public decimal UnitPrice
            {
                get { return FUnitPrice; }
            }

            public Product(string name, decimal unitPrice)
            {
                FName = name;
                FUnitPrice = unitPrice;
            }
        }

    }
}
