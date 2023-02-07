using LittleFarmCakes.Data;
using LittleFarmCakes.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System.Diagnostics;

namespace LittleFarmCakes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<HomeController> logger

            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _logger = logger;

        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Products");
            }

            var products = db.Products.Include("Category").Where(p => p.Valid == true).OrderByDescending(o => o.Rating);

            try
            {
                ViewBag.FirstProduct = products.First();
                ViewBag.Products = products.Skip(1).Take(2);
            }
            catch
            {
                ViewBag.Products = products;
            }
            return View();
        }

        public IActionResult ShowHome(int id)
        {
            var prod = db.Products.Include("Category")
                         .Include("User")
                         .Include("Comments")
                         .Include("Comments.User")
                         .Where(p => p.Id == id).First();

            return View(prod);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}