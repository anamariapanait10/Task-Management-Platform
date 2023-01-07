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
            var teams = from team in db.Teams.Include("Project")
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
                                        || at.Project.ProjectTitle.Contains(search)
                                    ).Select(a => a.TeamId).ToList();
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
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            var assigned = db.Teams.Count(t => t.TeamId == id);
            if (assigned == 0)
            {
                TempData["message"] = "Nu exsita echipa pe care doriti sa o gasiti";
                return RedirectToAction("Index", "Teams");
            }
            else
            {
                Team team = db.Teams
                                .Include("Project")
                                .Where(t => t.TeamId == id)
                                .First();
                SetAccessRights(team);

                // team.TeamMembers = GetTeamMembers(team.TeamId);

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
            // Daca modelul este valid
            if (teamMember.UserId != "null")
            {
                if (ModelState.IsValid)
                {
                    // Verificam daca avem deja articolul in colectie
                    if (db.TeamMembers
                        .Where(tm => tm.UserId == teamMember.UserId)
                        .Where(tm => tm.TeamId == teamMember.TeamId)
                        .Count() > 0)
                    {
                        TempData["message"] = "Acest membru este deja adaugat in echipa";
                        TempData["messageType"] = "alert-danger";
                        return Redirect("/Teams/Show/" + teamMember.TeamId);
                    }
                    else
                    {
                        // Adaugam asocierea intre articol si bookmark 
                        db.TeamMembers.Add(teamMember);
                        // Salvam modificarile
                        db.SaveChanges();

                        // Adaugam un mesaj de success
                        TempData["message"] = "Membrul a fost adaugat in echipa selectata";
                        TempData["messageType"] = "alert-success";

                        // Ne intoarcem la pagina speciala a echipelor
                        return Redirect("/Teams/Show/" + teamMember.TeamId);
                    }

                }
                else
                {
                    TempData["message"] = "Nu s-a putut adauga membrul in echipa";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Teams/Show/" + teamMember.TeamId);
                }
            }
            else
            {
                TempData["message"] = "Nu ati selectat niciun membru";
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
                TempData["message"] = "Nu aveti niciun proiect creat sau proiecte disponibile";
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
                if(proj.UserId == _userManager.GetUserId(User))
                {
                    TeamMember member = new TeamMember();
                    member.UserId = _userManager.GetUserId(User);
                    member.TeamId = team.TeamId;
                    db.TeamMembers.Add(member);
                    db.SaveChanges();
                }
                TempData["message"] = "Echipa a fost adaugata";
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
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unuei echipe care nu va apartine";
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
                    TempData["message"] = "Echipa a fost modificata";
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
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei echipe care nu va apartine";
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
                db.Teams.Remove(team);
                db.SaveChanges();
                TempData["message"] = "Echipa a fost stearsa";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti o echipa care nu va apartine";
                return RedirectToAction("Index");
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllProjects()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate proiectele din baza de date
            var projects = from p in db.Projects
                           select p;

            // verific daca userul este admin, iar in acest caz acesta poate selecta orice proiect neasignat
            if (User.IsInRole("Admin"))
            {
                // iteram prin proiecte
                foreach (var proj in projects)
                {
                    // adaugam in lista elementele necesare pentru dropdown
                    // id-ul proiectului si titlul acestuia
                    // vreau ca acel proiect sa nu mai fie asignat si altei echipe
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
                // iteram prin proiecte
                foreach (var proj in projects)
                {
                    // adaugam in lista elementele necesare pentru dropdown
                    // id-ul proiectului si titlul acestuia
                    // verific daca userul care creeaza echipa are acelasi id ca si organizatorul de
                    // proiect pe care vreau sa il asignez
                    // de asemenea, vreau ca acel proiect sa nu mai fie asignat si altei echipe
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
            // returnam lista de proiecte
            return selectList;
        }


        [NonAction]
        public bool HasProjects()
        {
            // extragem toate proiectele din baza de date
            var projects = from p in db.Projects
                           select p;

            // verific daca userul este admin, iar in acest caz acesta poate vedea orice proiect neasignat
            if (User.IsInRole("Admin"))
            {
                // iteram prin proiecte
                foreach (var proj in projects)
                {
                    // vreau ca acel proiect sa nu mai fie asignat si altei echipe
                    var assigned = db.Teams.Count(t => t.ProjectId == proj.ProjectId);
                    if (assigned == 0)
                    {
                        return true;
                    }
                }
            }
            else
            {
                // iteram prin proiecte
                foreach (var proj in projects)
                {
                    // verific daca userul care creeaza echipa are acelasi id ca si organizatorul de
                    // proiect pe care vreau sa il asignez
                    // de asemenea, vreau ca acel proiect sa nu mai fie asignat si altei echipe
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
            // extragem toti membrii tuturor echipelor din baza de date
            var members  = from tm in db.TeamMembers
                           select tm;

            var tMembers = new List<TeamMember>();
            // verific daca userul este in echipa
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
            // extragem toti userii din baza de date
            var users= from u in db.Users
                       select u;

            List<ApplicationUser> available = new();
            // verific daca userul este deja in echipa
            foreach(var user in users)
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
            // extragem toti userii din baza de date
            var users = from u in db.Users
                        select u;

            List<ApplicationUser> member = new();
            // verific daca userul este deja in echipa
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

        // Conditiile de afisare a butoanelor de editare si stergere
        [NonAction]
        private void SetAccessRights(Team team)
        {
            ViewBag.AfisareButoane = false;
            ViewBag.EsteOrganizator = false;
            ViewBag.EsteAdmin = false;
            ViewBag.Admin = 0;

            if (isOrganizer(team))
            {
                ViewBag.AfisareButoane = true;
                ViewBag.EsteOrganizator = true;
            }

            if (User.IsInRole("Admin"))
            {
                ViewBag.AfisareButoane = true;
                ViewBag.EsteAdmin = true;
                ViewBag.Admin = _userManager.GetUserId(User);
            }
        }
    }
}