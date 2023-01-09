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

        // Se afiseaza lista tuturor proiectelor impreuna cu echipa
        // din care fac parte
        // HttpGet implicit
        // Pentru fiecare proiect se afiseaza si utilizatorul care a postat proiectul
        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            var projs = from proj in db.Projects.Include("User")
                        orderby proj.ProjectTitle
                        select proj;
            var search = "";
            // MOTOR DE CAUTARE
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere
                // Cautare in proiect (Title si Content)
                List<int> projectIds = db.Projects.Where
                                        (
                                         at => at.ProjectTitle.Contains(search)
                                         || at.ProjectContent.Contains(search)
                                        ).Select(a => a.ProjectId).ToList();
                // Lista proiectelor care contin cuvantul cautat
                projs = db.Projects.Where(project => projectIds.Contains(project.ProjectId))
                                    .Include("User")
                                    .OrderBy(a => a.ProjectDate);
            }
            ViewBag.SearchString = search;
            // AFISARE PAGINATA
            // Alegem sa afisam 3 proiecte pe pagina
            int _perPage = 3;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            // Fiind un numar variabil de proiecte, verificam de fiecare data utilizand 
            // metoda Count()
            int totalItems = projs.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            // /Projects/Index?page=valoare
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3 
            // Asadar offsetul este egal cu numarul de proiecte care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau proiectele corespunzatoare pentru fiecare pagina la care ne aflam 
            // in functie de offset
            var paginatedProjects = projs.Skip(offset).Take(_perPage);

            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem proiectele cu ajutorul unui ViewBag catre View-ul corespunzator
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


        // Se afiseaza un singur proiect in functie de id-ul sau 
        // impreuna cu echipa din care face parte.
        // In plus sunt preluate si toate taskurile asociate unui proiect
        // HttpGet implicit
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            Project project = db.Projects.Include("User")
                                         .Where(p => p.ProjectId == id)
                                         .First();
            var tasks = db.Tasks.Include("Project").Include("Stat").Include("TeamMember").Include("TeamMember.User")
                                                    .Where(t => t.ProjectId == id)
                                                    .ToList();

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

            ViewBag.ProjectsTasks = paginatedTasks;

            ViewBag.PaginationBaseUrl = "/Projects/Show/" + id + "/?page";

            SetAccessRights(project);

            ViewBag.Users = db.Users;
            ViewBag.TeamMembers = db.TeamMembers.Include("Team").Include("User");
            ViewBag.Teams = db.Teams.Include("Project");

            return View(project);
        }

        // Adaugarea unui task asociat unui proiect in baza de date
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
        // Se afiseaza formularul in care se vor completa datele unui proiect
        // impreuna cu selectarea echipei din care face parte
        // HttpGet implicit
        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Project project = new Project();

            return View(project);
        }
        // Se adauga proiectul in baza de date
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
                TempData["message"] = "Proiectul a fost adaugat";
                TempData["messageType"] = "alert-succes";
                return RedirectToAction("Index");
            }
            else
            {
                return View(project);
            }
        }
        // Se editeaza un proiect existent in baza de date impreuna cu echipa din care face parte
        // Echipa se selecteaza dintr-un dropdown
        // HttpGet implicit
        // Se afiseaza formularul impreuna cu datele aferente proiectului din baza de date
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
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui proiect care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
        // Se adauga proiectul modificat in baza de date
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
                    TempData["message"] = "Proiectul a fost modificat";
                    TempData["messageType"] = "alert-succes";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui proiect care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View(requestProject);
            }
        }
        // Se atribuie proiectul altui membru al echipei
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
                TempData["message"] = "Nu aveti dreptul sa modificati organizatorul proiectului";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Teams/Show/" + TId);
            }
        }
        // Se adauga proiectul modificat in baza de date
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
                    TempData["message"] = "Organizatorul de proiect a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return Redirect("/Teams/Show/" + TId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa modificati organizatorul proiectului";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Teams/Show/" + TId);
                }
            }
            else
            {
                TempData["message"] = "Nu ati selectat niciun alt organizator!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Projects/EditOrg/" + UId + "/" + TId);
            }
        }
        // Se atribuie proiectul altui membru al echipei
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
                        TempData["message"] = "Sunteti ultimul membru din echipa si nu puteti parasi echipa!" + "\n" + "Stergeti echipa?";
                        TempData["messageType"] = "alert-danger";
                        return Redirect("/Teams/Show/" + TId);
                    }
                    TempData["message"] = "Aceste este ultimul membru din echipa si nu il puteti elimina!" + "\n" + "Stergeti echipa?";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Teams/Show/" + TId);
                }
                TempData["message"] = "Inainte sa stergeti organizatorul trebuie sa selectati alt organizator!";
                TempData["messageType"] = "alert-danger";
                return View(project);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa modificati organizatorul proiectului";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Teams/Show/" + TId);
            }
        }
        // Se adauga proiectul modificat in baza de date
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
                    TempData["message"] = "Organizatorul de proiect a fost modificat, iar cel vechi sters";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return Redirect("/Teams/Show/" + TId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa modificati organizatorul proiectului";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Teams/Show/" + TId);
                }
            }
            else
            {
                TempData["message"] = "Nu ati selectat niciun alt organizator!";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Projects/EditOrgDel/" + UId + "/" + TId);
            }
        }
        [NonAction]
        public List<SelectListItem> GetMembers(string UId, int TId)
        {
            // extragem componenta echipei date
            var tMembers = db.TeamMembers.Where(tm => tm.TeamId == TId);
            var users = db.Users;

            List<SelectListItem> available = new();
            // verific daca userul este deja in echipa
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
        // Se sterge un proiect din baza de date 
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
                TempData["message"] = "Proiectul a fost sters!";
                TempData["messageType"] = "alert-succes";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sastergeti acest proiect!";
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

        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights(Project project)
        {
            ViewBag.AfisareButoane = false;
            ViewBag.EsteOrganizator = false;
            ViewBag.EsteAdmin = false;
            ViewBag.ButonAfisareTask = true;

            if (isOrganizer(project))
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
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;
            ViewBag.EsteOrganizator = false;
            ViewBag.EsteAdmin = false;
            ViewBag.ButonAfisareTask = true;
        }
    }
}