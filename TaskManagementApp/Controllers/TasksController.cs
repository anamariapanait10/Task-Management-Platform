using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TaskManagementApp.Data;
using TaskManagementApp.Models;
using Project = TaskManagementApp.Models.Project;
using Task = TaskManagementApp.Models.Task;

namespace TaskManagementApp.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        private static string returnUrl = "";

        public TasksController(
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
                var tasks = from task in db.Tasks
                            .Include("Project")
                            .Include("Stat")
                            .Include("TeamMember.User")
                            orderby task.TaskTitle
                            select task;

                // Search engine
                var search = "";
                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                    List<int> taskIds = db.Tasks.Where(
                                             at => at.TaskTitle.Contains(search)
                                             || at.TaskContent.Contains(search)
                                            ).Select(a => a.TaskId).ToList();

                    tasks = db.Tasks.Where(task => taskIds.Contains(task.TaskId))
                                        .Include("Project")
                                        .Include("Stat")
                                        .Include("TeamMember.User")
                                        .OrderBy(a => a.StartDate);
                }
                ViewBag.SearchString = search;

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
                ViewBag.Tasks = paginatedTasks;
                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Tasks/Index/?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Tasks/Index/?page";
                }

                ViewBag.Users = db.Users;

                SetAccessRights();
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
                var tasks = from task in db.Tasks.Include("Project").Include("Stat").Include("TeamMember.User")
                            join team in teams
                                    on task.ProjectId equals team.ProjectId
                            orderby team.TeamName, task.TaskTitle
                            select task;

                // Search engine
                var search = "";
                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                    List<int> taskIds = db.Tasks.Where(
                                             at => at.TaskTitle.Contains(search)
                                             || at.TaskContent.Contains(search)
                                            ).Select(a => a.TaskId).ToList();

                    tasks = db.Tasks.Where(task => taskIds.Contains(task.TaskId))
                                        .Include("Project")
                                        .Include("Stat")
                                        .Include("TeamMember.User")
                                        .OrderBy(a => a.StartDate);
                }
                ViewBag.SearchString = search;

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
                ViewBag.Tasks = paginatedTasks;
                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Tasks/Index/?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Tasks/Index/?page";
                }

                ViewBag.Users = db.Users;

                SetAccessRights();
                return View();

            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        { 
            var spl = Request.Headers["Referer"].ToString().Substring(10);
            spl = spl.Substring(spl.IndexOf("/"));
            ViewBag.returnUrl = spl;

            Task task = db.Tasks
                .Include("Project")
                .Include("Stat")
                .Include("TeamMember")
                .Include("TeamMember.User")
                .Include("Comments")
                .Include("Comments.User")
                .Where(t => t.TaskId == id)
                .First();

            var comments = db.Comments
                                .Include("User")
                                .Where(c => c.TaskId == task.TaskId)
                                .ToList();
            // Paging the comments
            int _perPage = 3;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            int totalItems = comments.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }
            var paginatedComments = comments.Skip(offset).Take(_perPage);
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            ViewBag.TasksComments = paginatedComments;

            ViewBag.PaginationBaseUrl = "/Tasks/Show/" + id + "/?page";

            ViewBag.Users = db.Users;
            
            SetAccessRights(task);

            return View(task);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.CommentDate = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Tasks/Show/" + comment.TaskId);
            }
            else
            {
                Task task = db.Tasks
                   .Include("Project")
                   .Include("Stat")
                   .Include("TeamMember")
                   .Include("Comments")
                   .Include("Comments.User")
                   .Where(t => t.TaskId == comment.TaskId)
                   .First();

                SetAccessRights(task);

                return View(task);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Task task = new Task();
            
            var spl = Request.Headers["Referer"].ToString().Substring(10);
            spl = spl.Substring(spl.IndexOf("/"));
            returnUrl = spl;

            if (Regex.Match(returnUrl, @"/Projects/Show/*").Success)
            {
                string routeEnd = returnUrl.Substring(15);
                string number = "";
                if (routeEnd.Contains('?'))
                {
                    number = routeEnd.Substring(0, routeEnd.IndexOf('/'));
                }
                else
                {
                    number = routeEnd;
                }

                var nr = Int32.Parse(number);
                ViewBag.Project = db.Projects.Where(p => p.ProjectId == nr).First();
                if(User.IsInRole("Admin") || _userManager.GetUserId(User) == ViewBag.Project.UserId)
                {
                    task.ProjectId = ViewBag.Project.ProjectId;
                    return View(task);
                }
                else
                {
                    TempData["message"] = "You have no right to add tasks to a project that does not belong to you!";
                    TempData["messageType"] = "alert-danger";
                    return Redirect(returnUrl);
                }
            }
            if (Regex.Match(returnUrl, @"/TeamMembers/Show/*").Success)
            {
                string routeEnd = returnUrl.Substring(18);
                string number = "";
                if (routeEnd.Contains('?'))
                {
                    number = routeEnd.Substring(0, routeEnd.IndexOf('/'));
                }
                else
                {
                    number = routeEnd;
                }

                var nr = Int32.Parse(number);
                ViewBag.Project = db.Projects.Where(p => p.ProjectId == nr).First();
                if (User.IsInRole("Admin") || _userManager.GetUserId(User) == ViewBag.Project.UserId)
                {
                    task.ProjectId = ViewBag.Project.ProjectId;

                    return View(task);
                }
                else
                {
                    TempData["message"] = "You do not have the right to add tasks to a team that does not belong to you!";
                    TempData["messageType"] = "alert-danger";
                    return Redirect(returnUrl);
                }
            }
            TempData["message"] = "You cannot add tasks if you are not on the page of a project or a team!";
            TempData["messageType"] = "alert-danger";
            return Redirect(returnUrl);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult New(Task task)
        {
            var sanitizer = new HtmlSanitizer();
            var proj = db.Projects.Where(p => p.ProjectId == task.ProjectId).First();
            if (_userManager.GetUserId(User) == proj.UserId || User.IsInRole("Admin"))
            {
                Stat stat = db.Stats.Where(s => s.StatName == "Not Assigned").First();
                task.StatId = stat.StatId;

                if (ModelState.IsValid)
                {
                    task.TaskContent = sanitizer.Sanitize(task.TaskContent);
                    db.Tasks.Add(task);
                    db.SaveChanges();
                    TempData["message"] = "The task has been added!";
                    TempData["messageType"] = "alert-succes";
                    return Redirect(returnUrl);
                }
                else
                {
                    return View(task);
                }
            }
            else
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                          .Where(y => y.Count > 0)
                          .ToList();
                var jsonString = JsonConvert.SerializeObject(
                   errors, Formatting.Indented,
                   new JsonConverter[] { new StringEnumConverter() });
                Console.WriteLine(jsonString);
                TempData["message"] = "You have no rights!";
                TempData["messageType"] = "alert-danger";
                return Redirect(returnUrl);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            var spl = Request.Headers["Referer"].ToString().Substring(10);
            spl = spl.Substring(spl.IndexOf("/"));
            returnUrl = spl;

            Task task = db.Tasks.Include("Stat").Where(t => t.TaskId == id).First();
            if(task != null)
            {
                if (isOrganizer(task) || User.IsInRole("Admin"))
                {
                    return View(task);
                }
                else
                {
                    TempData["message"] = "You do not have the right to make changes to a task if you are not the organizer of the project to which it belongs!";
                    TempData["messageType"] = "alert-danger";
                    return Redirect(returnUrl);
                }
            }
            else
            {
                TempData["message"] = "The task you want to edit does not exist!";
                TempData["messageType"] = "alert-danger";
                return Redirect(returnUrl);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Task requestTask)
        {
            Task task = db.Tasks.Find(id);
            var sanitizer = new HtmlSanitizer();

            if (ModelState.IsValid)
            {
                if (isOrganizer(task) || User.IsInRole("Admin"))
                {

                    task.TaskTitle = requestTask.TaskTitle;
                    task.TaskContent = sanitizer.Sanitize(requestTask.TaskContent);
                    TempData["message"] = "The task has been edited!";
                    TempData["messageType"] = "alert-succes";
                    db.SaveChanges();
                    return Redirect(returnUrl);
                }
                else
                {
                    TempData["message"] = "You do not have the right to make changes to a task if you are not the project organizer!";
                    TempData["messageType"] = "alert-danger";
                    return Redirect(returnUrl);
                }
            }
            else
            {
                return View(requestTask);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult AssignTask(int id)
        {
            Task task = db.Tasks.Include("Stat")
                                        .Where(t => t.TaskId == id)
                                        .First();
            if (task == null)
            {
                TempData["message"] = "The wanted task does not exist!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Tasks/Index/" + id);
            }
            if (isOrganizer(task) || User.IsInRole("Admin"))
            {
                task.TeamMembersList = GetAvailableTeamMembers(task);
                return View(task);
            }
            else
            {
                TempData["message"] = "You do not have the right to assign tasks from a team or project that does not belong to you!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Tasks/Show/" + id);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult AssignTask(int id, Task requestTask)
        {
            Task task = db.Tasks.Include("Stat").Include("TeamMember").Include("TeamMember.User").Where(t => t.TaskId == id).First();

            if (isOrganizer(task) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    if (requestTask.TeamMemberId != null && task.TeamMemberId == null)
                    {
                        if (task.Stat.StatName == "Not Assigned")
                        {
                            Stat stat = db.Stats.Where(s => s.StatName == "Not Started").First();
                            task.StatId = stat.StatId;
                        }
                        task.TeamMemberId = requestTask.TeamMemberId;
                        if (requestTask.StartDate != null)
                            task.StartDate = requestTask.StartDate;
                        if (requestTask.DueDate != null)
                            task.DueDate = requestTask.DueDate;
                        TempData["message"] = "The task has been assigned!";
                        TempData["messageType"] = "alert-succes";
                        db.SaveChanges();
                        return Redirect("/Tasks/Show/" + id);
                    }
                    else
                    {
                        if (requestTask.TeamMemberId == null && task.TeamMemberId != null)
                        {
                            Stat stat = db.Stats.Where(s => s.StatName == "Not Assigned").First();
                            task.StatId = stat.StatId;
                            task.TeamMemberId = null;
                            if (requestTask.StartDate != null)
                                task.StartDate = requestTask.StartDate;
                            if (requestTask.DueDate != null)
                                task.DueDate = requestTask.DueDate;
                            db.SaveChanges();
                            return Redirect("/Tasks/Show/" + id);
                        }
                        else if (requestTask.TeamMemberId != null && task.TeamMemberId != null)
                        {
                            task.TeamMemberId = requestTask.TeamMemberId;
                            if (requestTask.StartDate != null)
                                task.StartDate = requestTask.StartDate;
                            if (requestTask.DueDate != null)
                                task.DueDate = requestTask.DueDate;
                            TempData["message"] = "The task has been assigned!";
                            TempData["messageType"] = "alert-succes";
                            db.SaveChanges();
                            return Redirect("/Tasks/Show/" + id);
                        }

                        TempData["message"] = "Select a member!";
                        TempData["messageType"] = "alert-danger";
                        requestTask.TeamMembersList = GetAvailableTeamMembers(requestTask);
                        return View(requestTask);
                    }
                }
                else 
                {
                    TempData["message"] = "Model State not Valid!";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Tasks/Show/" + id);
                }
            }
            else
            {
                TempData["message"] = "You do not have the right to assign a task if you are not the organizer of the project to which it belongs!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Tasks/Show/" + id);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult ChangeDate(int id)
        {
            Task task = db.Tasks.Include("Stat")
                                        .Where(t => t.TaskId == id)
                                        .First();
            if (task == null)
            {
                TempData["message"] = "The wanted task does not exist!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Tasks/Index/" + id);
            }
            if (isOrganizer(task) || User.IsInRole("Admin"))
            {
                return View(task);
            }
            else
            {
                TempData["message"] = "You do not have the right to change the start or end date for the tasks in a team or project that does not belong to you!\r\n";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Tasks/Show/" + id);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult ChangeDate(int id, Task requestTask)
        {
            Task task = db.Tasks.Include("Stat").Include("TeamMember").Include("TeamMember.User").Where(t => t.TaskId == id).First();

            if (isOrganizer(task) || User.IsInRole("Admin"))
            {
                if (requestTask.StartDate != null)
                    task.StartDate = requestTask.StartDate;
                if (requestTask.DueDate != null)
                    task.DueDate = requestTask.DueDate;
                TempData["message"] = "The task has been assigned!";
                TempData["messageType"] = "alert-succes";
                db.SaveChanges();
                return Redirect("/Tasks/Show/" + id);
            }
            else
            {
                TempData["message"] = "You do not have the right to assign a task if you are not the organizer of the project to which it belongs!\r\n";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Tasks/Show/" + id);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult ChangeState(int id)
        {
            Task task = db.Tasks.Include("Stat")
                                        .Where(t => t.TaskId == id)
                                        .First();
            if (task == null)
            {
                TempData["message"] = "The wanted task does not exist!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Tasks/Index/" + id);
            }
            SetAccessRights(task);
            if (isOrganizer(task) || User.IsInRole("Admin") || task.TeamMember.UserId == _userManager.GetUserId(User))
            {
                task.StatsList = GetAllStats(task);
                return View(task);
            }
            else
            {
                TempData["message"] = "You do not have the right to change the status of this task!\r\n";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Tasks/Show/" + id);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult ChangeState(int id, Task requestTask)
        {
            Task task = db.Tasks.Include("Stat").Include("TeamMember").Include("TeamMember.User").Where(t => t.TaskId == id).First();

            if (isOrganizer(task) || User.IsInRole("Admin") || task.TeamMember.UserId == _userManager.GetUserId(User))
            {
                if (ModelState.IsValid && requestTask.StatId != null)
                {
                    task.StatId = requestTask.StatId;
                    db.SaveChanges();
                    TempData["message"] = "The status of the task has been changed!";
                    TempData["messageType"] = "alert-succes";
                    return Redirect("/Tasks/Show/" + id);
                }
                else
                {
                    requestTask.StatsList = GetAllStats(task);
                    SetAccessRights(task);
                    TempData["message"] = "Select a status!";
                    TempData["messageType"] = "alert-danger";
                    return View(requestTask);
                }
            }
            else
            {
                TempData["message"] = "You do not have the right to change the status of this task!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Tasks/Show/" + id);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Task task = db.Tasks.Include("Comments")
                                         .Where(t => t.TaskId == id)
                                         .First();
            var comm = db.Comments.Where(c => c.TaskId == task.TaskId);

            if (isOrganizer(task) || User.IsInRole("Admin"))
            {
                foreach(Comment c in comm)
                {
                    db.Comments.Remove(c);
                    db.SaveChanges();
                }
                db.Tasks.Remove(task);
                db.SaveChanges();
                TempData["message"] = "The task has been deleted!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "You do not have the right to delete a task if you are not the organizer of the project!\r\n";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllStats(Task task)
        {
            var selectList = new List<SelectListItem>();

            var stats = from stat in db.Stats
                        where stat.StatName != "Not Assigned" && stat.StatId != task.StatId
                        select stat;

            foreach (var stat in stats)
            {
                selectList.Add(new SelectListItem
                {
                    Value = stat.StatId.ToString(),
                    Text = stat.StatName.ToString()
                });
            }

            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAvailableTeamMembers(Task task)
        {
            var selectList = new List<SelectListItem>();

            var project = db.Projects.Find(task.ProjectId);

            var team = db.Teams.Where(t => t.ProjectId == project.ProjectId).FirstOrDefault();
            if (team == null)
            {
                return selectList;
            }

            if (task.TeamMemberId != null)
            {
                var teamMembers = from teamMember in db.TeamMembers
                                     .Include("User")
                                  where teamMember.TeamMemberId != task.TeamMemberId
                                        && teamMember.TeamId == team.TeamId
                                  select teamMember;
                foreach (var member in teamMembers)
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = member.TeamMemberId.ToString(),
                        Text = member.User.UserName.ToString()
                    });
                }
            }
            else
            {
                var teamMembers = from teamMember in db.TeamMembers
                                     .Include("User")
                                  where teamMember.TeamMemberId != task.TeamMemberId
                                        && teamMember.TeamId == team.TeamId
                                  select teamMember;
                foreach (var member in teamMembers)
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = member.TeamMemberId.ToString(),
                        Text = member.User.UserName.ToString()
                    });
                }
            }
            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllProjects()
        {
            var selectList = new List<SelectListItem>();

            var projects = from proj in db.Projects
                           select proj;

            foreach (var project in projects)
            {
                selectList.Add(new SelectListItem
                {
                    Value = project.ProjectId.ToString(),
                    Text = project.ProjectTitle.ToString()
                });
            }

            return selectList;
        }

        [NonAction]
        private void SetAccessRights(Task task)
        {
            ViewBag.ShowButtons = false;
            ViewBag.IsOrganizer = false;
            ViewBag.IsAdmin = false;
            ViewBag.ShowButtonTask = false;
            ViewBag.AssignedUser = false;
            ViewBag.CurrentUser = _userManager.GetUserId(User);

            if (isOrganizer(task))
            {
                ViewBag.ShowButtons = true;
                ViewBag.IsOrganizer = true;
                return;
            }

            if (User.IsInRole("Admin"))
            {
                ViewBag.ShowButtons = true;
                ViewBag.IsAdmin = true;
                return;
            }

            if (task.TeamMemberId != null)
            {
                if (task.TeamMember.UserId == _userManager.GetUserId(User))
                {
                    ViewBag.ShowButtons = true;
                    ViewBag.AssignedUser = true;
                    return;
                }
            }
        }

        [NonAction]
        private void SetAccessRights()
        {
            ViewBag.ShowButtons = false;
            ViewBag.IsOrganizer = false;
            ViewBag.IsAdmin = false;
            ViewBag.ShowButtonTask = true;
            ViewBag.CurrentUser = _userManager.GetUserId(User);

            if (User.IsInRole("Admin"))
            {
                ViewBag.ShowButtons = true;
                ViewBag.IsAdmin = true;
            }
        }


        [NonAction]
        public bool isOrganizer(Task task)
        {

            Project project = db.Projects
                            .Where(p => p.ProjectId == task.ProjectId)
                            .First();

            string organizerId = project.UserId;

            return organizerId == _userManager.GetUserId(User);
        }
    }
}