using LittleFarmCakes.Data;
using LittleFarmCakes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace LittleFarmCakes.Controllers
{
    public class UsersController : Controller
    {
            private readonly ApplicationDbContext db;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly IWebHostEnvironment _env;
            public UsersController(
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

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var userRoles = db.UserRoles.Include("User").Include("Role");

            ViewBag.UserRoles = userRoles;
            ViewBag.Roles = GetAllRoles();

            if(TempData.ContainsKey("Message"))
            {
                ViewBag.Message = TempData["Message"];
            }

            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(string id)
        {
            try
            {
                var userRole = db.UserRoles.Include("User").Where(u => u.UserId == id).First();
                userRole.UpdateRole = GetAllRoles();

                return View(userRole);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Edit(ApplicationUserRole userRole)
        {
           
                var oldUser = db.UserRoles.Where(u => u.UserId == userRole.UserId).First();
                ApplicationUserRole update = new ApplicationUserRole();
                update.UserId = userRole.UserId;
                update.RoleId = userRole.RoleId;
                db.UserRoles.Remove(oldUser);
                db.UserRoles.Add(update);

                TempData["Message"] = "Userul a fost updatat";

                db.SaveChanges();
                return RedirectToAction("Index");
        }

    [NonAction]
    public IEnumerable<SelectListItem> GetAllRoles()
    {
        var selectList = new List<SelectListItem>();

        var roles = from role in db.Roles
                         select role;

        foreach (var role in roles)
        {

            selectList.Add(new SelectListItem
            {
                Value = role.Id.ToString(),
                Text = role.Name.ToString()
            });
        }

        return selectList;
    }

}
}
