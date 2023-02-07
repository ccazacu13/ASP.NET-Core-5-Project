using LittleFarmCakes.Data;
using LittleFarmCakes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LittleFarmCakes.Controllers
{
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        public CartsController(
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
        public IActionResult Index()
        {

            var userId = _userManager.GetUserId(User);
            var carts = db.Carts.Include("Product").Where(c => c.UserId == userId);

            ViewBag.Products = carts;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Contributor,Admin")]
        public IActionResult AddToCart(Cart cart)
        {

            string userId = _userManager.GetUserId(User);
            try
            {
                Cart oldCart = db.Carts.Where(c => c.ProductId == cart.ProductId && c.UserId == userId).First();

                oldCart.Count += cart.Count;
            }
            catch
            {
                cart.UserId = userId;
                db.Carts.Add(cart);
            }
            db.SaveChanges();

            TempData["Message"] = "Produsele au fost adaugate in cos";

            //return RedirectToAction("Show", "Products", new {id = cart.ProductId});
            return RedirectToAction("Show","Products", new {id = cart.ProductId});
        }

        [HttpPost]
        [Authorize(Roles = "User,Contributor,Admin")]

        public IActionResult Delete(Cart cart)
        {
            var userId = _userManager.GetUserId(User);
            try
            {
                var oldCart = db.Carts.Where(c => c.Product.Id == cart.ProductId && c.UserId == userId).First();
                oldCart.Count -= cart.Count;

                if (oldCart.Count == 0)
                    db.Carts.Remove(oldCart);

                db.SaveChanges();

            }
            catch (Exception)
            {

            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "User,Contributor,Admin")]
        public IActionResult UpdateCart(Cart cart)
        {

            string userId = _userManager.GetUserId(User);
            try
            {
                Cart oldCart = db.Carts.Where(c => c.ProductId == cart.ProductId && c.UserId == userId).First();

                oldCart.Count += cart.Count;
            }
            catch
            {
                cart.UserId = userId;
                db.Carts.Add(cart);
            }
            db.SaveChanges();

            //return RedirectToAction("Show", "Products", new {id = cart.ProductId});
            return RedirectToAction("Index");
        }
    }
}
