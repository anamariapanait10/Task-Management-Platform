using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;
using TaskManagementApp.Data;
using TaskManagementApp.Models;
using Task = TaskManagementApp.Models.Task;

namespace TaskManagementApp.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public TeamsController(
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

                // Search engine
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

                // Paging the teams
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
                var teams = from team in db.Teams.Include("Project").Include("Project.User")
                            where team.Project.UserId == user
                            orderby team.TeamName
                            select team;

                var search = "";

                // Search engine
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

                // Paging the teams
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
            var assigned = db.Teams.Count(t => t.TeamId == id);
            if (assigned == 0)
            {
                TempData["message"] = "The team you want to find does not exist!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Teams");
            }
            else
            {
                Team team = db.Teams
                                .Include("Project")
                                .Where(t => t.TeamId == id)
                                .First();
                SetAccessRights(team);

                ViewBag.UsersList = GetUsers(team.TeamId);
                ViewBag.UserMembers = GetMembers(team.TeamId);

                ViewBag.NrAvailableUsers = GetUsers(team.TeamId).Count;

                ViewBag.Organizer = team.Project.UserId;

                return View(team);
            }
        }

        [HttpPost]
        public IActionResult AddMember([FromForm] TeamMember teamMember)
        { 
            if (teamMember.UserId != "null")
            {
                if (ModelState.IsValid)
                {
                    if (db.TeamMembers
                        .Where(tm => tm.UserId == teamMember.UserId)
                        .Where(tm => tm.TeamId == teamMember.TeamId)
                        .Count() > 0)
                    {
                        TempData["message"] = "This member is already added to the team!";
                        TempData["messageType"] = "alert-danger";
                        return Redirect("/Teams/Show/" + teamMember.TeamId);
                    }
                    else
                    {
                        db.TeamMembers.Add(teamMember);
                        db.SaveChanges();

                        TempData["message"] = "The member has been added to the selected team!";
                        TempData["messageType"] = "alert-success";
                        return Redirect("/Teams/Show/" + teamMember.TeamId);
                    }
                }
                else
                {
                    TempData["message"] = "The member could not be added to the team!";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Teams/Show/" + teamMember.TeamId);
                }
            }
            else
            {
                TempData["message"] = "You have not selected any member!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Teams/Show/" + teamMember.TeamId);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public ActionResult New()
        {
            Team team = new Team();

            if (HasProjects() == true)
                team.ProjectsList = GetAllProjects();
            else
            {
                TempData["message"] = "You have no projects created or available!";
                return RedirectToAction("Index", "Teams");
            }
            return View(team);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult New(Team team)
        {
            team.TeamDate = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.Teams.Add(team);
                db.SaveChanges();
                Project proj = db.Projects.Find(team.ProjectId);
                if (proj.UserId == _userManager.GetUserId(User))
                {
                    TeamMember member = new TeamMember();
                    member.UserId = _userManager.GetUserId(User);
                    member.TeamId = team.TeamId;
                    db.TeamMembers.Add(member);
                    db.SaveChanges();
                }
                TempData["message"] = "The team has been added!";
                return RedirectToAction("Index", "Teams");
            }
            else
            {
                team.ProjectsList = GetAllProjects();
                return View(team);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public ActionResult Edit(int id)
        {
            Team team = db.Teams.Find(id);
            Project proj = db.Projects.Find(team.ProjectId);

            if (proj.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
            {
                TempData["message"] = "You do not have the right to make changes to a team that does not belong to you!";
                return RedirectToAction("Index");
            }
            return View(team);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Edit(int id, Team requestedTeam)
        {
            Team team = db.Teams.Find(id);
            Project proj = db.Projects.Find(team.ProjectId);

            if (proj.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    team.TeamName = requestedTeam.TeamName;
                    TempData["message"] = "The team has been modified!";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(requestedTeam);
                }
            }
            else
            {
                TempData["message"] = "You do not have the right to make changes to a team that does not belong to you!";
                return RedirectToAction("Index");
            }

        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Team team = db.Teams
                            .Include("Project")
                            .Where(t => t.TeamId == id)
                            .First();
            Project proj = db.Projects
                             .Where(p => p.ProjectId == team.ProjectId)
                             .First();
            if (proj.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                var tasks = db.Tasks.Where(t => t.ProjectId == proj.ProjectId);
                if (tasks.Count() > 0)
                {
                    foreach (Task t in tasks)
                    {
                        var comments = db.Comments.Where(c => c.TaskId == t.TaskId);
                        if (comments.Count() > 0)
                        {
                            foreach (Comment c in comments)
                            {
                                db.Comments.Remove(c);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                var members = db.TeamMembers.Where(tm => tm.TeamId == team.TeamId);

                if (members.Count() > 0)
                {
                    foreach (TeamMember m in members)
                    {
                        db.TeamMembers.Remove(m);
                    }

                }
                db.Teams.Remove(team);
                db.SaveChanges();
                TempData["message"] = "The team has been deleted!";
                TempData["messageType"] = "alert-succes";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You do not have the right to delete this team!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllProjects()
        {
            var selectList = new List<SelectListItem>();

            var projects = from p in db.Projects
                           select p;

            if (User.IsInRole("Admin"))
            {
                foreach (var proj in projects)
                {
                    // we want the project to not be assigned to another team
                    var assigned = db.Teams.Count(t => t.ProjectId == proj.ProjectId);
                    if (assigned == 0)
                    {
                        selectList.Add(new SelectListItem
                        {
                            Value = proj.ProjectId.ToString(),
                            Text = proj.ProjectTitle.ToString()
                        });
                    }
                }
            }
            else
            {
                foreach (var proj in projects)
                {
                    // we want the user to be the organizer of the projects that are included in the dropdown
                    // we also want the project to not be assigned to another team
                    if (proj.UserId == _userManager.GetUserId(User))
                    {
                        var assigned = db.Teams.Count(t => t.ProjectId == proj.ProjectId);
                        if (assigned == 0)
                        {
                            selectList.Add(new SelectListItem
                            {
                                Value = proj.ProjectId.ToString(),
                                Text = proj.ProjectTitle.ToString()
                            });
                        }
                    }
                }
            }
            return selectList;
        }

        [NonAction]
        public bool HasProjects()
        { 
            var projects = from p in db.Projects
                           select p;

            if (User.IsInRole("Admin"))
            {
                foreach (var proj in projects)
                {
                    // we want the project to not be assigned to another team
                    var assigned = db.Teams.Count(t => t.ProjectId == proj.ProjectId);
                    if (assigned == 0)
                    {
                        return true;
                    }
                }
            }
            else
            { 
                foreach (var proj in projects)
                {
                    // we want the user to be the organizer of the project
                    // we also want the project to not be assigned to another team
                    if (proj.UserId == _userManager.GetUserId(User))
                    {
                        var assigned = db.Teams.Count(t => t.ProjectId == proj.ProjectId);
                        if (assigned == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /*
        [NonAction]
        public ICollection<TeamMember> GetTeamMembers(int id)
        {
            var members  = from tm in db.TeamMembers
                           select tm;

            var tMembers = new List<TeamMember>();
            foreach (var tm in members)
            {
                if (tm.TeamId == id)
                {
                    tMembers.Add(tm);
                }
            }
            return tMembers;
        }*/

        [NonAction]
        public List<ApplicationUser> GetUsers(int id)
        {
            var users = from u in db.Users
                        select u;

            List<ApplicationUser> available = new();
            foreach (var user in users)
            {
                var contor = db.TeamMembers.Count(tm => tm.UserId == user.Id && tm.TeamId == id);
                if (contor == 0)
                {
                    available.Add(user);
                }
            }
            return available;
        }

        [NonAction]
        public List<ApplicationUser> GetMembers(int id)
        {
            var users = from u in db.Users
                        select u;

            List<ApplicationUser> member = new();
            foreach (var user in users)
            {
                var contor = db.TeamMembers.Count(tm => tm.UserId == user.Id && tm.TeamId == id);
                if (contor != 0)
                {
                    member.Add(user);
                }
            }
            return member;
        }

        [NonAction]
        public bool isOrganizer(Team team)
        {

            Project project = db.Projects
                            .Where(p => p.ProjectId == team.ProjectId)
                            .First();

            string organizerId = project.UserId;

            return organizerId == _userManager.GetUserId(User);
        }

        [NonAction]
        private void SetAccessRights(Team team)
        {
            ViewBag.ShowButtons = false;
            ViewBag.IsOrganizer = false;
            ViewBag.IsAdmin = false;
            ViewBag.Admin = 0;

            if (isOrganizer(team))
            {
                ViewBag.ShowButtons = true;
                ViewBag.IsOrganizer = true;
            }

            if (User.IsInRole("Admin"))
            {
                ViewBag.ShowButtons = true;
                ViewBag.IsAdmin = true;
                ViewBag.Admin = _userManager.GetUserId(User);
            }
        }
    }
}