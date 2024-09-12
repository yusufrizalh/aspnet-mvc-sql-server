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

        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            // show product from product dto
            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("dd/MMMM/yyyy");

            return View(productDto);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("dd/MMMM/yyyy");

                return View(productDto);
            }

            // handling new image upload
            string newFileName = product.ImageFileName;
            if (productDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("ddddMMyyyyHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFile!.FileName);
                string imageFullPath = environment.WebRootPath
                    + "/products/" + newFileName;

                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                // delete old image
                string oldImageFullPath = environment.WebRootPath +
                        "/products/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }

            // update product to database
            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }
    }
}
