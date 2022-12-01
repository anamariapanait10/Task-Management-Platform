using FinalProjectApp.Data;
using FinalProjectApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectApp.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext db;
        public TeamsController(ApplicationDbContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            var team = db.Teams;
            ViewBag.Teams = team;
            return View();
        }
        public IActionResult Show(int id)
        {
            Team team = db.Teams.Find(id);
            return View(team);
        }
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Team team)
        {
            if (ModelState.IsValid)
            {
                db.Teams.Add(team);
                db.SaveChanges();
                TempData["message"] = "Echipa a fost adaugata";
                return RedirectToAction("Index");
            }

            else
            {
                return View(team);
            }
        }
        public ActionResult Edit(int id)
        {
            Team team = db.Teams.Find(id);
            return View(team);
        }

        [HttpPost]
        public ActionResult Edit(int id, Team requestedTeam)
        {
            Team team = db.Teams.Find(id);

            if (ModelState.IsValid)
            {

                team.Name = requestedTeam.Name;
                db.SaveChanges();
                TempData["message"] = "Echipa a fost modificata!";
                return RedirectToAction("Index");
            }
            else
            {
                return View(requestedTeam);
            }
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Team team = db.Teams.Find(id);
            db.Teams.Remove(team);
            TempData["message"] = "Echipa a fost stearsa";
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
