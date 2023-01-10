using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;
using TaskManagementApp.Data;
using TaskManagementApp.Models;
using Task = TaskManagementApp.Models.Task;

namespace TaskManagementApp.Controllers
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

        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                var comments = from comm in db.Comments.Include("Task").Include("Task.Project").Include("User")
                               orderby comm.CommentDate
                               select comm;

                var search = "";

                // Motor de cautare
                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
                    List<int> commIds = db.Comments.Where
                                            (
                                             at => at.CommentContent.Contains(search)
                                             || at.User.UserName.Contains(search)
                                            ).Select(a => a.CommentId).ToList();

                    comments = db.Comments.Where(comment => commIds.Contains(comment.CommentId))
                                        .Include("User")
                                        .OrderBy(a => a.CommentDate);

                }
                ViewBag.SearchString = search;

                // Paginare Comentarii
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
                ViewBag.Comments = paginatedComments;
                ViewBag.TasksList = GetAllTasks();
                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Comments/Index/?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Comments/Index/?page";
                }
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

                var comments = from comm in db.Comments.Include("Task").Include("Task.Project").Include("User")
                               join task in tasks
                                    on comm.TaskId equals task.TaskId
                               orderby task.TaskTitle, comm.CommentDate
                               select comm;

                var search = "";

                // Motor de cautare
                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
                    List<int> commIds = db.Comments.Where
                                            (
                                             at => at.CommentContent.Contains(search)
                                             || at.User.UserName.Contains(search)
                                            ).Select(a => a.CommentId).ToList();

                    comments = db.Comments.Where(comment => commIds.Contains(comment.CommentId))
                                        .Include("User")
                                        .OrderBy(a => a.CommentDate);

                }
                ViewBag.SearchString = search;

                // Paginare Comentarii
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
                ViewBag.Comments = paginatedComments;
                ViewBag.TasksList = GetAllTasks();
                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Comments/Index/?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Comments/Index/?page";
                }
                SetAccessRights();

                return View();
            }
        }

        // Adaugarea unui comentariu asociat unui task in baza de date
        /*
        [HttpPost]
        public IActionResult New(Comment comm)
        {
            comm.CommentDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Comments.Add(comm);
                db.SaveChanges();
                return Redirect("/Tasks/Show/" + comm.TaskId);
            }
            else
            {
                return Redirect("/Tasks/Show/" + comm.TaskId);
            }
        }
        */

        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Comment comm = new Comment();
            ViewBag.TasksList = GetAllTasks();
            return View(comm);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New([FromForm] Comment comment)
        {
            comment.CommentDate = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return View();
            }
            else
            {
                Comment comm = db.Comments.Include("User")
                                         .Where(c => c.CommentId == comment.CommentId)
                                         .First();

                return View(comm);
            }
        }

        // Stergerea unui comentariu asociat unui task din baza de date
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments
                .Include("User")
                .Where(c => c.CommentId == id)
                .First();

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();
                return Redirect("/Tasks/Show/" + comm.TaskId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti comentariul";
                return RedirectToAction("Index", "Tasks");
            }
        }

        // In acest moment vom implementa editarea intr-o pagina View separata
        // Se editeaza un comentariu existent
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments
                                .Include("User")
                                .Where(c => c.CommentId == id)
                                .First();
            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {

                return View(comm);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati comentariul";
                return RedirectToAction("Index", "Tasks");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments
                            .Include("User")
                            .Where(c => c.CommentId == id)
                            .First();

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {

                    comm.CommentContent = requestComment.CommentContent;
                    comm.CommentDate = DateTime.Now;

                    db.SaveChanges();

                    return Redirect("/Tasks/Show/" + comm.TaskId);
                }
                else
                {
                    return View(requestComment);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                return RedirectToAction("Index", "Tasks");
            }

        }

        [NonAction]
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;
            ViewBag.EsteOrganizator = false;
            ViewBag.EsteAdmin = false;
            ViewBag.ButonAfisareTask = true;

            if (User.IsInRole("Admin"))
            {
                ViewBag.AfisareButoane = true;
                ViewBag.EsteAdmin = true;
            }

        }

        [NonAction]
        private IEnumerable<SelectListItem> GetAllTasks()
        {
            var selectList = new List<SelectListItem>();

            // luam proiectele din care face parte userul curent
            var teamMembers = db.TeamMembers.Where(tm => tm.UserId == _userManager.GetUserId(User)).ToList();
            var projects = new List<Project>();
            foreach (TeamMember tm in teamMembers)
            {
                foreach (Team t in db.Teams)
                {
                    if (tm.TeamId == t.TeamId)
                    {
                        var project = db.Projects.Find(t.ProjectId);
                        projects.Add(project);
                    }
                }
            }

            // luam taskurile din proiectele userului curent
            foreach (Task t in db.Tasks)
            {
                foreach (var p in projects)
                {
                    if (t.ProjectId == p.ProjectId)
                    {
                        selectList.Add(new SelectListItem
                        {
                            Value = t.TaskId.ToString(),
                            Text = t.TaskTitle.ToString()
                        });
                        break;
                    }
                }
            }
            return selectList;
        }
    }
}