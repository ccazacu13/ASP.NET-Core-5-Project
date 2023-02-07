using LittleFarmCakes.Data;
using LittleFarmCakes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LittleFarmCakes.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public CommentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {

                db.Comments.Remove(comm);
                db.SaveChanges();

                if (comm.Stars != null)         // Daca stergem un comentariu cu rating updatam in baza de date produsul
                {
                    var product = db.Products.Where(pd => pd.Id == comm.ProductId).First();     //Obtinem produsul caruia i-a fost sters comentariul
                    product.Rating = UpdateRating(comm, false);         //Updatam rating-ul
                }

                db.SaveChanges();
                TempData["Message"] = "Comentariul a fost sters";

                return Redirect("/Products/Show/" + comm.ProductId);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti comentariul";
                return RedirectToAction("Index", "Products");
            }
        }


        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);
            ViewBag.Ratings = GetRatings();

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(comm);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati comentariul";
                return RedirectToAction("Index", "Products");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    comm.Content = requestComment.Content;
                    comm.Stars = requestComment.Stars;

                    var product = db.Products.Find(comm.ProductId);
                    product.Rating = UpdateRating(comm, false);

                    db.SaveChanges();
                    TempData["Message"] = "Comentariul a fost editat";

                    return Redirect("/Products/Show/" + comm.ProductId);
                }
                else
                {
                    return View(requestComment);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                return RedirectToAction("Index", "Products");
            }
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
}
    
