using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        // Se afiseaza lista tuturor taskuri impreuna cu 
        // Pentru fiecare task se afiseaza si userul care a postat taskul respectiv
        // HttpGet implicit
        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            var tasks = from task in db.Tasks
                            .Include("Project")
                            .Include("Stat")
                            .Include("TeamMember.User")
                        orderby task.TaskTitle
                        select task;

            // MOTOR DE CAUTARE
            var search = "";
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 

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

            // Paginare Taskuri
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

        // Se afiseaza un singur task in functie de id-ul sau 
        // impreuna cu statusul sau
        // In plus sunt preluate si toate comentariile asociate unui task
        // Se afiseaza si userul care a postat taskul respectiv
        // HttpGet implicit

        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            Task task = db.Tasks
                .Include("Project")
                .Include("Stat")
                .Include("TeamMember")
                .Include("Comments")
                .Include("Comments.User")
                .Where(t => t.TaskId == id)
                .First();


            var comments = db.Comments
                                .Include("User")
                                .Where(c => c.TaskId == task.TaskId)
                                .ToList();
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

            ViewBag.TasksComments = paginatedComments;

            ViewBag.PaginationBaseUrl = "/Tasks/Show/" + id + "/?page";

            ViewBag.Users = db.Users;
            SetAccessRights(task);

            return View(task);
        }

        // Adaugarea unui comentariu asociat unui task in baza de date
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

            task.StatsList = GetAllStats();
            task.ProjectsList = GetAllProjects();
            task.TeamMembersList = GetAllTeamMembers();
            // luam pagina de unde am venit ca sa stim unde sa ne intoarcem
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
                task.ProjectsList = GetProject(nr);
                task.TeamMembersList = GetAllTeamMembers(nr);
            }

            return View(task);
        }

        // Se adauga taskul in baza de date

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult New(Task task)
        {
            var sanitizer = new HtmlSanitizer();
            if (isOrganizer(task) || User.IsInRole("Admin"))
            {
                task.StartDate = DateTime.Now;

                if (ModelState.IsValid)
                {
                    task.TaskContent = sanitizer.Sanitize(task.TaskContent);
                    db.Tasks.Add(task);
                    db.SaveChanges();
                    TempData["message"] = "Taskul a fost adaugat";
                    return Redirect(returnUrl);
                }
                else
                {
                    task.StatsList = GetAllStats();
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
                TempData["message"] = "Nu aveti drepturi";
                return Redirect(returnUrl);
            }
        }


        // Se editeaza un task existent in baza de date impreuna cu 
        // Statusul se selecteaza dintr-un dropdown
        // HttpGet implicit
        // Se afiseaza formularul impreuna cu datele aferente taskului
        // din baza de date
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {

            Task task = db.Tasks.Include("Stat")
                                        .Where(t => t.TaskId == id)
                                        .First();

            task.StatsList = GetAllStats();

            if (isOrganizer(task) || User.IsInRole("Admin"))
            {
                // luam pagina de unde am venit ca sa stim unde sa ne intoarcem
                var spl = Request.Headers["Referer"].ToString().Substring(10);
                spl = spl.Substring(spl.IndexOf("/"));
                returnUrl = spl;
                return View(task);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui task daca nu sunteti organizator pe proiect";
                return RedirectToAction("Index");

            }

        }

        // Se adauga articolul modificat in baza de date
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
                    task.StatId = requestTask.StatId;
                    TempData["message"] = "Taskul a fost modificat";
                    db.SaveChanges();
                    return Redirect(returnUrl);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui task daca nu sunteti organizator pe proiect";
                    return Redirect(returnUrl);
                }
            }
            else
            {
                requestTask.ProjectsList = GetAllProjects();
                requestTask.StatsList = GetAllStats();
                return View(requestTask);
            }
        }

        // Se sterge un task din baza de date 
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Task task = db.Tasks.Include("Comments")
                                         .Where(t => t.TaskId == id)
                                         .First();

            if (isOrganizer(task) || User.IsInRole("Admin"))
            {
                db.Tasks.Remove(task);
                db.SaveChanges();
                TempData["message"] = "Taskul a fost sters";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un task daca nu sunteti organizatorul proiectului";
                return RedirectToAction("Index");
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllStats()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate statusurile din baza de date
            var stats = from stat in db.Stats
                        select stat;

            // iteram prin statusuri
            foreach (var stat in stats)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul statusului si denumirea acestuia
                selectList.Add(new SelectListItem
                {
                    Value = stat.StatId.ToString(),
                    Text = stat.StatName.ToString()
                });
            }

            // returnam lista de statusuri
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
        public IEnumerable<SelectListItem> GetAllTeamMembers()
        {
            var selectList = new List<SelectListItem>();

            var teamMembers = from teamMember in db.TeamMembers
                                .Include("User")
                              select teamMember;

            foreach (var teamMember in teamMembers)
            {
                selectList.Add(new SelectListItem
                {
                    Value = teamMember.TeamMemberId.ToString(),
                    Text = teamMember.User.UserName.ToString()
                });
            }

            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllTeamMembers(int ProjectId)
        {

            Team team = db.Teams.Where(t => t.ProjectId == ProjectId).First();

            var selectList = new List<SelectListItem>();

            var teamMembers = from teamMember in db.TeamMembers
                                .Include("User")
                                .Where(tm => tm.TeamId == team.TeamId)
                              select teamMember;

            foreach (var teamMember in teamMembers)
            {
                selectList.Add(new SelectListItem
                {
                    Value = teamMember.TeamMemberId.ToString(),
                    Text = teamMember.User.UserName.ToString()
                });
            }

            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetProject(int id)
        {
            var selectList = new List<SelectListItem>();
            var project = db.Projects.Where(p => p.ProjectId == id).First();
            selectList.Add(new SelectListItem
            {
                Value = project.ProjectId.ToString(),
                Text = project.ProjectTitle.ToString()
            });
            return selectList;
        }

        // Conditiile de afisare a butoanelor de editare si stergere
        [NonAction]
        private void SetAccessRights(Task task)
        {
            ViewBag.AfisareButoane = false;
            ViewBag.EsteOrganizator = false;
            ViewBag.EsteAdmin = false;
            ViewBag.ButonAfisareTask = false;

            if (isOrganizer(task))
            {
                ViewBag.AfisareButoane = true;
                ViewBag.EsteOrganizator = true;
            }

            if (User.IsInRole("Admin"))
            {
                ViewBag.AfisareButoane = true;
                ViewBag.EsteAdmin = true;
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