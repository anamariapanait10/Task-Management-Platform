using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TaskManagementApp.Data;
using TaskManagementApp.Models;
using static Humanizer.On;
using Project = TaskManagementApp.Models.Project;
using Task = TaskManagementApp.Models.Task;

namespace TaskManagementApp.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProjectsController(
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
                var projs = from proj in db.Projects.Include("User")
                            orderby proj.ProjectTitle
                            select proj;
                var search = "";
                // Search engine
                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); 
                    
                    List<int> projectIds = db.Projects.Where
                                            (
                                             at => at.ProjectTitle.Contains(search)
                                             || at.ProjectContent.Contains(search)
                                            ).Select(a => a.ProjectId).ToList();
                    
                    projs = db.Projects.Where(project => projectIds.Contains(project.ProjectId))
                                        .Include("User")
                                        .OrderBy(a => a.ProjectDate);
                }
                ViewBag.SearchString = search;

                // Paging the projects (3 projects on a page)
                int _perPage = 3;

                if (TempData.ContainsKey("message"))
                {
                    ViewBag.message = TempData["message"].ToString();
                }

                int totalItems = projs.Count();

                var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

                var offset = 0;

                if (!currentPage.Equals(0))
                {
                    offset = (currentPage - 1) * _perPage;
                }

                var paginatedProjects = projs.Skip(offset).Take(_perPage);

                ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

                ViewBag.Projects = paginatedProjects;

                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Projects/Index/?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Projects/Index/?page";
                }

                SetAccessRights();
                return View();
            }
            else
            {
                var user = _userManager.GetUserId(User);
                var projs = from proj in db.Projects.Include("User")
                            where proj.UserId == user
                            orderby proj.ProjectTitle
                            select proj;
                var search = "";

                // Search engine
                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
                    List<int> projectIds = db.Projects.Where
                                            (
                                             at => at.ProjectTitle.Contains(search)
                                             || at.ProjectContent.Contains(search)
                                            ).Select(a => a.ProjectId).ToList();
                    projs = db.Projects.Where(project => projectIds.Contains(project.ProjectId))
                                        .Include("User")
                                        .OrderBy(a => a.ProjectDate);
                }
                ViewBag.SearchString = search;

                // Paging the projects (3 projects on a page)
                int _perPage = 3;

                if (TempData.ContainsKey("message"))
                {
                    ViewBag.message = TempData["message"].ToString();
                }

                int totalItems = projs.Count();

                var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

                var offset = 0;

                if (!currentPage.Equals(0))
                {
                    offset = (currentPage - 1) * _perPage;
                }

                var paginatedProjects = projs.Skip(offset).Take(_perPage);

                ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

                ViewBag.Projects = paginatedProjects;

                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Projects/Index/?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Projects/Index/?page";
                }

                SetAccessRights();
                return View();
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            Project project = db.Projects.Include("User")
                                         .Where(p => p.ProjectId == id)
                                         .First();
            var tasks = db.Tasks.Include("Project").Include("Stat").Include("TeamMember").Include("TeamMember.User")
                                                    .Where(t => t.ProjectId == id)
                                                    .ToList();

            // Paging the tasks
            int _perPage = 3;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            int totalItems = tasks.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }
            var paginatedTasks = tasks.Skip(offset).Take(_perPage);
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            ViewBag.ProjectsTasks = paginatedTasks;

            ViewBag.PaginationBaseUrl = "/Projects/Show/" + id + "/?page";

            SetAccessRights(project);

            ViewBag.Users = db.Users;
            ViewBag.TeamMembers = db.TeamMembers.Include("Team").Include("User");
            ViewBag.Teams = db.Teams.Include("Project");

            return View(project);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show([FromForm] Task task)
        {
            task.StatId = 1;

            if (ModelState.IsValid)
            {
                db.Tasks.Add(task);
                db.SaveChanges();
                return Redirect("/Projects/Show/" + task.ProjectId);
            }
            else
            {
                Project project = db.Projects.Include("User")
                                        .Where(p => p.ProjectId == task.ProjectId)
                                        .First();

                SetAccessRights(project);
                ViewBag.Users = db.Users;
                ViewBag.TeamMembers = db.TeamMembers.Include("Team").Include("User");
                ViewBag.Teams = db.Teams.Include("Project");
                ViewBag.Tasks = db.Tasks.Include("Stat").Include("Project").Include("TeamMember");

                return View(project);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Project project = new Project();

            return View(project);
        }
        
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New(Project project)
        {
            project.ProjectDate = DateTime.Now;

            project.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Projects.Add(project);
                db.SaveChanges();
                TempData["message"] = "The project has been added!";
                TempData["messageType"] = "alert-succes";
                return RedirectToAction("Index");
            }
            else
            {
                return View(project);
            }
        }
        
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Project project = db.Projects.Include("User")
                                        .Where(p => p.ProjectId == id)
                                        .First();
            if (isOrganizer(project) || User.IsInRole("Admin"))
            {
                return View(project);
            }
            else
            {
                TempData["message"] = "You do not have the right to edit a project that is not yours!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Project requestProject)
        {
            Project project = db.Projects.Include("User")
                                         .Where(p => p.ProjectId == id)
                                         .First();
            if (ModelState.IsValid)
            {
                if (isOrganizer(project) || User.IsInRole("Admin"))
                {
                    project.ProjectTitle = requestProject.ProjectTitle;
                    project.ProjectContent = requestProject.ProjectContent;
                    TempData["message"] = "The project has been modified!";
                    TempData["messageType"] = "alert-succes";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "You do not have the right to edit a project that is not yours!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View(requestProject);
            }
        }
  
        [Route("Projects/EditOrg/{UId}/{TId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult EditOrg(string UId, int TId)
        {
            Team team = db.Teams.Find(TId);
            Project project = db.Projects.Where(p => p.ProjectId == team.ProjectId).First();

            project.UsersList = GetMembers(UId, TId);
            ViewBag.ProjEditOrgOrg = UId;
            ViewBag.ProjEditOrgTeam = TId;

            if (User.IsInRole("Admin"))
            {
                return View(project);
            }
            else
            {
                TempData["message"] = "You do not have the right to change the project organizer!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Teams/Show/" + TId);
            }
        }
        
        [HttpPost]
        [Route("Projects/EditOrg/{UId}/{TId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult EditOrg(string UId, int TId, Project requestProject)
        {
            Team team = db.Teams.Find(TId);
            Project project = db.Projects.Where(p => p.ProjectId == team.ProjectId).First();
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    project.UserId = requestProject.UserId;
                    TempData["message"] = "Project organizer has been changed!";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return Redirect("/Teams/Show/" + TId);
                }
                else
                {
                    TempData["message"] = "You do not have the right to change the project organizer!";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Teams/Show/" + TId);
                }
            }
            else
            {
                TempData["message"] = "You did not choose another organizer!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Projects/EditOrg/" + UId + "/" + TId);
            }
        }
        
        [Route("Projects/EditOrgDel/{UId}/{TId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult EditOrgDel(string UId, int TId)
        {
            Team team = db.Teams.Find(TId);
            Project project = db.Projects.Where(p => p.ProjectId == team.ProjectId).First();

            project.UsersList = GetMembers(UId, TId);
            ViewBag.ProjEditOrgOrg = UId;
            ViewBag.ProjEditOrgTeam = TId;

            if (User.IsInRole("Admin"))
            {
                if (project.UsersList.Count() == 0)
                {
                    if(UId == _userManager.GetUserId(User))
                    {
                        TempData["message"] = "You are the last member of this team so you can not leave the team!" + "\n" + "Do you want to delete the team?";
                        TempData["messageType"] = "alert-danger";
                        return Redirect("/Teams/Show/" + TId);
                    }
                    TempData["message"] = "This is the last member of this team, so you can not delete this person!" + "\n" + "Do you want to delete the team?";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Teams/Show/" + TId);
                }
                TempData["message"] = "Before deleting the organizer you have to choose another organizer!";
                TempData["messageType"] = "alert-danger";
                return View(project);
            }
            else
            {
                TempData["message"] = "You do not have the right to change the project organizer!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Teams/Show/" + TId);
            }
        }
       
        [HttpPost]
        [Route("/Projects/EditOrgDel/{UId}/{TId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult EditOrgDel(string UId, int TId, Project requestProject)
        {
            TeamMember member = db.TeamMembers
                                        .Where(tm => tm.UserId == UId && tm.TeamId == TId)
                                        .First();
            Team team = db.Teams.Find(TId);
            Project proj = db.Projects
                                 .Where(p => p.ProjectId == team.ProjectId)
                                 .First();
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    var tasks = db.Tasks.Include("TeamMember").Include("TeamMember.User").Where(t => t.TeamMemberId == member.TeamMemberId);
                    if (tasks.Count() > 0)
                    {
                        foreach (Task t in tasks)
                        {
                            t.TeamMemberId = null;
                            Stat stat = db.Stats.Where(s => s.StatName == "Not Assigned").First();
                            if (t.Stat.StatName != "Completed")
                                t.StatId = stat.StatId;
                            db.SaveChanges();
                        }
                    }
                    proj.UserId = requestProject.UserId;
                    db.TeamMembers.Remove(member);
                    TempData["message"] = "The project organizer has been changed and the old one has been deeted!";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return Redirect("/Teams/Show/" + TId);
                }
                else
                {
                    TempData["message"] = "You do not have the right to change the project organizer!";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Teams/Show/" + TId);
                }
            }
            else
            {
                TempData["message"] = "You have not selected another organizer!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Projects/EditOrgDel/" + UId + "/" + TId);
            }
        }
        [NonAction]
        public List<SelectListItem> GetMembers(string UId, int TId)
        {
            var tMembers = db.TeamMembers.Where(tm => tm.TeamId == TId);
            var users = db.Users;

            List<SelectListItem> available = new();
            foreach (var tMember in tMembers)
            {
                if (tMember.UserId != UId)
                {
                    foreach (var user in users)
                    {
                        if(tMember.UserId == user.Id)
                        {
                            available.Add(new SelectListItem
                            {
                                Value = user.Id.ToString(),
                                Text = user.UserName.ToString()
                            });
                            break;
                        }
                    }
                }
            }
            return available;
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Project project = db.Projects.Include("User")
                                        .Where(p => p.ProjectId == id)
                                        .First();
            if (isOrganizer(project) || User.IsInRole("Admin"))
            {

                var tasks = db.Tasks.Where(t => t.ProjectId == project.ProjectId);
                if(tasks.Count() > 0)
                {
                    foreach(Task t in tasks)
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
                var team = db.Teams.Where(t => t.ProjectId == project.ProjectId);

                if (team.Count() == 1)
                {
                    foreach (Team t in team)
                    {
                        var members = db.TeamMembers.Where(tm => tm.TeamId == t.TeamId);

                        if (members.Count() > 0)
                        {
                            foreach (TeamMember m in members)
                            {
                                db.TeamMembers.Remove(m);
                                db.SaveChanges();
                            }

                        }
                        db.Teams.Remove(t);
                        db.SaveChanges();
                    }                    
                }
                db.Projects.Remove(project);
                db.SaveChanges();
                TempData["message"] = "The project has been deleted!";
                TempData["messageType"] = "alert-succes";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You do not have the right to delete this project!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [NonAction]
        public bool isOrganizer(Project project)
        {

            string organizerId = project.UserId;

            return organizerId == _userManager.GetUserId(User);
        }

        private void SetAccessRights(Project project)
        {
            ViewBag.ShowButtons = false;
            ViewBag.IsOrganizer = false;
            ViewBag.IsAdmin = false;
            ViewBag.ShowButtonTask = true;

            if (isOrganizer(project))
            {
                ViewBag.ShowButtons = true;
                ViewBag.IsOrganizer = true;
            }
            if (User.IsInRole("Admin"))
            {
                ViewBag.ShowButtons = true;
                ViewBag.IsAdmin = true;
            }
        }
        private void SetAccessRights()
        {
            ViewBag.ShowButtons = false;
            ViewBag.IsOrganizer = false;
            ViewBag.IsAdmin = false;
            ViewBag.ShowButtonTask = true;
        }
    }
}