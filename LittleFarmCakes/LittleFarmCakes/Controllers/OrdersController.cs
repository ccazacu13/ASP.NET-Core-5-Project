using LittleFarmCakes.Data;
using LittleFarmCakes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LittleFarmCakes.Controllers
{
    public class OrdersController : Controller
    {

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        public OrdersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment env)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }
        [HttpPost]
        [Authorize(Roles = "User,Contributor,Admin")]
        public IActionResult New()
        {
            var userId = _userManager.GetUserId(User);
            var products = db.Carts.Where(c => c.UserId == userId);

            foreach (var product in products)
            {
                Order ord = new Order();
                ord.UserId = userId;
                ord.ProductId = product.ProductId;
                ord.Count = product.Count;

                db.Orders.Add(ord);
                db.Carts.Remove(product);
            }
            db.SaveChanges();
            TempData["Message"] = "Comanda a fost plasata cu succes";

            return RedirectToAction("Index", "Products");
        }
    }
}
