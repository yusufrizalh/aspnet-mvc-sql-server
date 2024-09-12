using Microsoft.AspNetCore.Mvc;
using MyWebMvc.Models;
using MyWebMvc.Services;

namespace MyWebMvc.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public ProductsController(ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        public IActionResult Index()
        {
            var products = context.Products.OrderByDescending(
                p => p.Id).ToList();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Image cannot be null!");
            }

            if (!ModelState.IsValid)
            {
                return View(productDto);
            }

            // mandling image upload
            string newFileName = DateTime.Now.ToString("ddddMMyyyyHHmmssfff");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);
            string imageFullPath = environment.WebRootPath
                + "/products/" + newFileName;

            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            // save new product to database
            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now,
            };
            context.Products.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }
    }
}
