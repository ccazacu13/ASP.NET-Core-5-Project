using Humanizer;
using LittleFarmCakes.Data;
using LittleFarmCakes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System.Drawing;

namespace LittleFarmCakes.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        public ProductsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment env
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        public IActionResult Index2()
        {
            var products = db.Products.Include("Category").Where(p => p.Valid == true);
            return View(products.ToList());
        }

        [Authorize(Roles = "User,Contributor,Admin")]
        public IActionResult Index(string sortOrder)
        {
            ViewBag.CurrentSortOrder = sortOrder;

            //ViewBag.RaitingSortParam = String.IsNullOrEmpty(sortOrder) ? "raiting_desc" : "";
            if (String.IsNullOrEmpty(sortOrder))
            {
                ViewBag.RaitingSortParam = "raiting_desc";
                ViewBag.RaitingSortReverse = "";
            }
            else
            {
                ViewBag.RaitingSortParam = "";
                ViewBag.RaitingSortReverse = "raiting_desc";
            }



            var products = db.Products.Include("Category").Include("User").Where(p => p.Valid == true);

            switch (sortOrder)
            {
                case "raiting_desc":
                    {
                        products = products.OrderByDescending(p => p.Rating);
                        Console.WriteLine("sdg------------------------------------------------------");
                    }
                    break;
                default:
                    products = products.OrderBy(p => p.Rating);
                    break;
            }
            ViewBag.Products = products;


            var search = "";

            // MOTOR DE CAUTARE

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();


                List<int> productIds = db.Products.Where
                                        (
                                         at => at.Title.Contains(search)
                                         || at.Description.Contains(search)
                                        ).Select(a => a.Id).ToList();

                List<int> productIdsOfCommentsWithSearchString = db.Comments
                                        .Where
                                        (
                                         c => c.Content.Contains(search)
                                        ).Select(c => (int)c.ProductId).ToList();

                List<int> mergedIds = productIds.Union(productIdsOfCommentsWithSearchString).ToList();

                products = db.Products.Where(product => mergedIds.Contains(product.Id))
                     .Include("Category")
                     .Include("User")
                     .Where(p => p.Valid == true)
                     .OrderBy(p => p.Rating);

            }

            ViewBag.SearchString = search;

            // AFISARE PAGINATA

            int _perPage = 3;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }


            ViewBag.Authenticated = true;

            int totalItems = products.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

 
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage; 
            }

        
            var paginatedProducts = products.Skip(offset).Take(_perPage);



            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            ViewBag.Products = paginatedProducts;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Products/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Products/Index/?page";
            }

            Console.WriteLine(offset);
            
            return View();
        }


        [Authorize(Roles = "User,Contributor,Admin")]
        public IActionResult Show(int id)
        {
            ViewBag.Authenticated = true;

            Product product = db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(art => art.Id == id)
                                         .First();


            SetAccessRights();

            ViewBag.UserCurent = _userManager.GetUserId(User);
            

            if (TempData.ContainsKey("Message"))
                ViewBag.Message = TempData["Message"];

            product.Ratings = GetRatings();



            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "User,Contributor,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            //if (TempData.ContainsKey("Message"))
            //{
            //    TempData["Message"] = TempData["Message"];
            //}


            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);

                if (comment.Stars is not null)         // Daca comentariul adaugat are rating dam update rating-ului produsului 
                {
                    var product = db.Products.Find(comment.ProductId);
                    product.Rating = UpdateRating(comment, true);   // Functia de update

                }
                db.SaveChanges();

                TempData["Message"] = "Commentariul a fost adaugat cu succes";

                return Redirect("/Products/Show/" + comment.ProductId);
            }

            else
            {
                Product prod = db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(art => art.Id == comment.ProductId)
                                         .First();


                SetAccessRights();
                prod.Ratings = GetRatings();

                return View(prod);
            }
        }


        [Authorize(Roles = "Contributor,Admin")]
        public IActionResult New()
        {
            Product product = new Product();

            product.Categ = GetAllCategories();

            return View(product);
        }


        [Authorize(Roles = "Contributor,Admin")]
        [HttpPost]
        public IActionResult New(Product product)
        {
            product.Rating = 0;
            if (User.IsInRole("Admin"))
            {
                product.Valid = true;
            }
            else
                product.Valid = false;


            if (ModelState.IsValid)
            {
             
                db.Products.Add(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost adaugat";
                return RedirectToAction("Index");
            }
            else
            {
                product.Categ = GetAllCategories();
                return View(product);
            }
        }

        [Authorize(Roles = "Contributor,Admin")]
        public IActionResult Edit(int id)
        {

            Product product = db.Products.Include("Category")
                                         .Where(art => art.Id == id)
                                         .First();

            product.Categ = GetAllCategories();

            if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(product);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                return RedirectToAction("Index");
            }

        }

        [HttpPost]
        [Authorize(Roles = "Contributor,Admin")]
        public IActionResult Edit(int id, Product requestProduct)
        {
            Product product = db.Products.Find(id);


            if (ModelState.IsValid)

            {
                //Console.WriteLine(product.Title + " " + product.Description + " " + product.Price + " " + product.CategoryId);

                if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    product.Title = requestProduct.Title;
                    product.Description = requestProduct.Description;
                    product.CategoryId = requestProduct.CategoryId;

                    if (User.IsInRole("Admin"))
                        product.Valid = true;
                    else
                        product.Valid = false;

                    TempData["message"] = "Produsul modificat va fi reevaluat";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                    return RedirectToAction("Index");
                }
            }
            else
            {

                requestProduct.Categ = GetAllCategories();
                return View(requestProduct);
            }
        }


        // Se sterge un articol din baza de date 
        [HttpPost]
        [Authorize(Roles = "Contributor,Admin")]
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Include("Comments")
                                         .Where(art => art.Id == id)
                                         .First();

            if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost sters";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un produs care nu va apartine";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Validate()
        {
            var products = db.Products.Include("Category").Where(p => p.Valid == false);
            ViewBag.Products = products;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Validate (int id, bool validate)
        {
            var product = db.Products.Find(id);
            if (validate)
                product.Valid = true;
            else
                db.Products.Remove(product);

            db.SaveChanges();

            return RedirectToAction("Validate");
        }

        [HttpPost]
        [Authorize(Roles = "User,Contributor,Admin")]
        public IActionResult AddToCart()
        {


            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from cat in db.Categories
                             select cat;

            foreach (var category in categories)
            {

                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }

            return selectList;
        }

        public IActionResult IndexNou()
        {
            return View();
        }
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Contributor"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }

        public IActionResult UploadImage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(Product product, IFormFile productImage)
        {
            if (ModelState.IsValid)
            {

                if (productImage.Length > 0)
                {
                    var storagePath = Path.Combine(
                        _env.WebRootPath, // calea folderului wwwroot
                        "images", //calea folderului images
                        productImage.FileName
                        );


                    var databaseFileName = "/images" + productImage.FileName;
                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await productImage.CopyToAsync(fileStream);
                    }

                    product.Picture = databaseFileName;
                    db.Products.Add(product);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

            }

            return View();
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetRatings()
        {
            var selectList = new List<SelectListItem>();

            for (int i = 0; i <= 5; ++i)
            {

                selectList.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString()
                });
            }

            return selectList;

        }
        [NonAction]
        public float UpdateRating(Comment comment, bool add)
        {          
            var comments = from comm in db.Comments.Where(comm => comm.ProductId == comment.ProductId)
                           select comm;     // Cautam comentariile corespunzatore produsului la care s-a adaugat sau sters un comentariu

            int commentsSize;
            float total;

            if (add)        // cazul in care adaugam
            {
                commentsSize = 1;
                total = (int)comment.Stars;
            }
            else           // cazul in care stergem
            {
                commentsSize = 0;
                total = 0;
            }

            foreach (var comm in comments)          //calculam rating-ul total si numarul de comentarii
            {
                if (comm.Stars is not null)
                {
                    total += (int)comm.Stars;
                    commentsSize++;
                }
            }

            if (commentsSize != 0)                  //daca nu am ramas fara comentarii intoarcem raspunsul, altfel 0
                return (float)System.Math.Round(total / commentsSize, 2);
            else
                return 0;
        }

    }
};
