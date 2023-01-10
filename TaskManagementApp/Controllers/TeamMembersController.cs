using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System;
using System.Data;
using System.Drawing.Printing;
using System.Net.WebSockets;
using System.Security.Cryptography;
using TaskManagementApp.Data;
using TaskManagementApp.Models;
using Task = TaskManagementApp.Models.Task;

namespace TaskManagementApp.Controllers
{
    public class TeamMembersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public TeamMembersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                var teams = from team in db.Teams.Include("Project").Include("Project.User")
                            orderby team.TeamName
                            select team;

                var search = "";

                // MOTOR DE CAUTARE
                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                    List<int> teamIds = db.Teams.Where
                                        (
                                            at => at.TeamName.Contains(search)
                                        ).Select(a => a.TeamId).ToList();
                    teams = db.Teams.Where(team => teamIds.Contains(team.TeamId))
                                .Include("Project").Include("Project.User")
                                .OrderBy(a => a.TeamName);
                }
                ViewBag.SearchString = search;

                // Paginare Echipe
                int _perPage = 3;
                if (TempData.ContainsKey("message"))
                {
                    ViewBag.message = TempData["message"].ToString();
                }
                int totalItems = teams.Count();
                var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
                var offset = 0;
                if (!currentPage.Equals(0))
                {
                    offset = (currentPage - 1) * _perPage;
                }
                var paginatedTeams = teams.Skip(offset).Take(_perPage);
                ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
                ViewBag.Teams = paginatedTeams;
                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Teams/Index/?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Teams/Index/?page";
                }

                return View();
            }
            else
            {
                var user = _userManager.GetUserId(User);
                var teams = from team in db.Teams.Include("Project")
                            join member in db.TeamMembers
                                on team.TeamId equals member.TeamId 
                            where member.UserId == user
                            orderby team.TeamName
                            select team;

                var search = "";

                // MOTOR DE CAUTARE
                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                    List<int> teamIds = db.Teams.Where
                                        (
                                            at => at.TeamName.Contains(search)
                                        ).Select(a => a.TeamId).ToList();
                    teams = db.Teams.Where(team => teamIds.Contains(team.TeamId))
                                .Include("Project").Include("Project.User")
                                .OrderBy(a => a.TeamName);
                }
                ViewBag.SearchString = search;

                // Paginare Echipe
                int _perPage = 3;
                if (TempData.ContainsKey("message"))
                {
                    ViewBag.message = TempData["message"].ToString();
                }
                int totalItems = teams.Count();
                var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
                var offset = 0;
                if (!currentPage.Equals(0))
                {
                    offset = (currentPage - 1) * _perPage;
                }
                var paginatedTeams = teams.Skip(offset).Take(_perPage);
                ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
                ViewBag.Teams = paginatedTeams;
                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Teams/Index/?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Teams/Index/?page";
                }

                return View();
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            Team team = db.Teams.Find(id);
            ViewBag.Team = db.Teams.Find(id);

            Project proj = db.Projects.Find(team.ProjectId);
            ViewBag.Project = db.Projects.Find(team.ProjectId);

            ViewBag.Organiser = db.Users.Find(proj.UserId);

            ViewBag.Stats = db.Stats;


            ViewBag.Tasks = db.Tasks
                                .Include("Stat")
                                .Include("Project")
                                .Include("TeamMember")
                                .Where(t => t.ProjectId == proj.ProjectId);
            
            ViewBag.Users = db.Users;
            ViewBag.TeamMembers = db.TeamMembers.Include("User").Include("Team").Where(tm => tm.TeamId == team.TeamId);
            SetAccessRights();
            ViewBag.EsteOrganizator = false;
            ViewBag.Organizer = proj.UserId;
            if (proj.UserId == _userManager.GetUserId(User))
            {
                ViewBag.AfisareButoane = true;
                ViewBag.EsteOrganizator = true;
            }
            return View();
        }

        [HttpPost]
        [Route("TeamMembers/Delete/{UId}/{TId}")]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(string UId, int TId)
        {
            TeamMember member = db.TeamMembers
                                        .Where(tm => tm.UserId == UId && tm.TeamId == TId)
                                        .First();
            Team team = db.Teams.Find(TId);
            Project proj = db.Projects
                                 .Where(p => p.ProjectId == team.ProjectId)
                                 .First();
            if (proj.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                var tasks = db.Tasks.Include("TeamMember").Include("TeamMember.User").Where(t => t.TeamMemberId  == member.TeamMemberId);
                if (tasks.Count() > 0)
                {
                    foreach (Task t in tasks)
                    {
                        t.TeamMemberId = null;
                        Stat stat = db.Stats.Where(s => s.StatName == "Not Assigned").First();
                        if(t.Stat.StatName != "Completed")
                            t.StatId = stat.StatId;
                        db.SaveChanges();
                    }
                }
                db.TeamMembers.Remove(member);
                db.SaveChanges();
                TempData["message"] = "Membrul a fost sters";
                TempData["messageType"] = "alert-success";
                return Redirect("/Teams/Show/" + TId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti membrii din aceasta echipa";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Teams/Show/" + TId);
            }
        }
        [NonAction]
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;
            ViewBag.EsteAdmin = false;
            ViewBag.EsteOrganizator = false;
            ViewBag.ButonAfisareTask = true;
            ViewBag.Admin = 0;

            if (User.IsInRole("Admin"))
            {
                ViewBag.AfisareButoane = true;
                ViewBag.EsteAdmin = true;
                ViewBag.Admin = _userManager.GetUserId(User);
            }
        }
    }
}