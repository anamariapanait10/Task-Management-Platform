using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGeneration.Design;
using System.Data;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipelines;
using TaskManagementApp.Data;
using TaskManagementApp.Models;
using Microsoft.AspNetCore.SignalR;

namespace TaskManagementApp.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService
            <DbContextOptions<ApplicationDbContext>>()))
            {

                if (!context.Stats.Any())
                {
                    context.Stats.AddRange(
                    new Stat { StatName = "Not Assigned" },
                    new Stat { StatName = "Not Started" },
                    new Stat { StatName = "In Progress" },
                    new Stat { StatName = "Completed" }
                    );
                    context.SaveChanges();
                }

                // Verificam daca in baza de date exista cel putin un rol
                // insemnand ca a fost rulat codul
                // De aceea facem return pentru a nu insera rolurile inca o data
                // Acesta metoda trebuie sa se execute o singura data
                if (context.Roles.Any())
                {
                    return; // baza de date contine deja roluri
                }
                // CREAREA ROLURILOR IN BD
                // daca nu contine roluri, acestea se vor crea
                context.Roles.AddRange(
                new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7211", Name = "User", NormalizedName = "User".ToUpper() }
                );
                // o noua instanta pe care o vom utiliza pentru crearea parolelor utilizatorilor
                // parolele sunt de tip hash
                var hasher = new PasswordHasher<ApplicationUser>();
                // CREAREA USERILOR IN BD
                // Se creeaza cate un user pentru fiecare rol
                context.Users.AddRange(
                new ApplicationUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb0", // primary key
                    UserName = "Admin",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Admin1!")
                },
                new ApplicationUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb1", // primary key
                    UserName = "Jane",
                    EmailConfirmed = true,
                    NormalizedEmail = "JANE@TEST.COM",
                    Email = "jane@test.com",
                    NormalizedUserName = "JANE@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "User1!")
                },
                 new ApplicationUser
                 {
                     Id = "8e445865-a24d-4543-a6c6-9443d048cdb2", // primary key
                     UserName = "Mark",
                     EmailConfirmed = true,
                     NormalizedEmail = "MARK@TEST.COM",
                     Email = "mark@test.com",
                     NormalizedUserName = "MARK@TEST.COM",
                     PasswordHash = hasher.HashPassword(null, "User1!")
                 },
                 new ApplicationUser
                 {
                     Id = "8e445865-a24d-4543-a6c6-9443d048cdb3", // primary key
                     UserName = "Erica",
                     EmailConfirmed = true,
                     NormalizedEmail = "ERICA@TEST.COM",
                     Email = "erica@test.com",
                     NormalizedUserName = "ERICA@TEST.COM",
                     PasswordHash = hasher.HashPassword(null, "User1!")
                 },
                 new ApplicationUser
                 {
                     Id = "8e445865-a24d-4543-a6c6-9443d048cdb4", // primary key
                     UserName = "John",
                     EmailConfirmed = true,
                     NormalizedEmail = "JOHN@TEST.COM",
                     Email = "john@test.com",
                     NormalizedUserName = "JOHN@TEST.COM",
                     PasswordHash = hasher.HashPassword(null, "User1!")
                 },
                 new ApplicationUser
                 {
                     Id = "8e445865-a24d-4543-a6c6-9443d048cdb5", // primary key
                     UserName = "Tom",
                     EmailConfirmed = true,
                     NormalizedEmail = "TOM@TEST.COM",
                     Email = "tom@test.com",
                     NormalizedUserName = "TOM@TEST.COM",
                     PasswordHash = hasher.HashPassword(null, "User1!")
                 },
                 new ApplicationUser
                 {
                     Id = "8e445865-a24d-4543-a6c6-9443d048cdb6", // primary key
                     UserName = "Jerry",
                     EmailConfirmed = true,
                     NormalizedEmail = "JERRY@TEST.COM",
                     Email = "jerry@test.com",
                     NormalizedUserName = "JERRY@TEST.COM",
                     PasswordHash = hasher.HashPassword(null, "User1!")
                 },
                 new ApplicationUser
                 {
                     Id = "8e445865-a24d-4543-a6c6-9443d048cdb7", // primary key
                     UserName = "Sofia",
                     EmailConfirmed = true,
                     NormalizedEmail = "SOFIA@TEST.COM",
                     Email = "sofia@test.com",
                     NormalizedUserName = "SOFIA@TEST.COM",
                     PasswordHash = hasher.HashPassword(null, "User1!")
                 },
                 new ApplicationUser
                 {
                     Id = "8e445865-a24d-4543-a6c6-9443d048cdb8", // primary key
                     UserName = "Kate",
                     EmailConfirmed = true,
                     NormalizedEmail = "KATE@TEST.COM",
                     Email = "kate@test.com",
                     NormalizedUserName = "KATE@TEST.COM",
                     PasswordHash = hasher.HashPassword(null, "User1!")
                 }
                );
                // ASOCIEREA USER-ROLE
                context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb6"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb7"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb8"
                }
                );
                context.SaveChanges();

                if (!context.Projects.Any())
                {
                    context.Projects.AddRange(
                    new Project
                    {
                        ProjectTitle = "Task Management App",
                        ProjectContent = "Acest proiect are ca scop modelarea unei platforme de gestionare a taskurilor, care sa fie de folos companiilor care lucreaza la diverse proiecte, asigurand buna functionare a intregii echipe." +
                                    " Vrem ca aplicatia sa permita inregistrarea utilizatorilor, iar ulterior, acestia sa creeze proiecte si echipe, sa adauge taskuri si comentarii, care sa permita actualizarea in timp real a informatiilor.",
                        ProjectDate = new DateTime(2022, 12, 12, 10, 4, 15),
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    },
                    new Project
                    {
                        ProjectTitle = "My Shell",
                        ProjectContent = "Acest proiect simuleaza un mini shell." + " Vrem ca noul shell sa permita functionalitatile elementele de baza gasite in orice linie de comanda (ex.istoric comenzi, pipe, expresii logice, suspendarea unui program).",
                        ProjectDate = new DateTime(2022, 10, 22, 22, 30, 15),
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb7"
                    },
                    new Project
                    {
                        ProjectTitle = "DiskAnalizer",
                        ProjectContent = "Vrem sa implementam un daemon care analizeaza spatiul utilizat pe un dispozitiv de stocare incepand de la o cale data, si sa construim un program utilitar care permite folosirea acestei functionalitati " +
                                        "din linia de comanda. Daemonul trebuie sa analizeze spatiul ocupat recursiv, pentru fiecare director continut, indiferent de adancime.",
                        ProjectDate = new DateTime(2022, 10, 22, 17, 30, 15),
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4"
                    },
                    new Project
                    {
                        ProjectTitle = "Gestiune Baza de Date",
                        ProjectContent = "Vrem sa creem o baza de date (adaugare tabele, constraneri) si sa adaugam proceduri, functii, triggeri si pachete care sa ajute la gestionare acesteia.",
                        ProjectDate = new DateTime(2022, 12, 24, 8, 00, 00),
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb6"
                    },
                    new Project
                    {
                        ProjectTitle = "ArticlesApp",
                        ProjectContent = "Vrem sa creem o aplicatie sa permita utilizatorilor sa urmareasca stiri, sa citeasca articole si documentare, din diverse domenii care ii atrag.",
                        ProjectDate = new DateTime(2022, 10, 4, 16, 05, 30),
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                    }
                    );
                    context.SaveChanges();
                }

                if (!context.Teams.Any())
                {
                    context.Teams.AddRange(
                    new Team
                    {
                        TeamName = "ASP Task Management",
                        TeamDate = new DateTime(2022, 12, 12, 13, 13, 13),
                        ProjectId = 1
                    },
                    new Team
                    {
                        TeamName = "SO #Nr.1",
                        TeamDate = new DateTime(2022, 10, 23, 22, 30, 15),
                        ProjectId = 2
                    },
                    new Team
                    {
                        TeamName = "TopTeam SO",
                        TeamDate = new DateTime(2022, 10, 22, 17, 32, 45),
                        ProjectId = 3
                    },
                    new Team
                    {
                        TeamName = "Laborator ASP",
                        TeamDate = new DateTime(2022, 12, 24, 8, 00, 00),
                        ProjectId = 5
                    }
                    );
                    context.SaveChanges();
                }

                if (!context.TeamMembers.Any())
                {
                    context.TeamMembers.AddRange(
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                        TeamId = 1
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                        TeamId = 1
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb7",
                        TeamId = 1
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb7",
                        TeamId = 2
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3",
                        TeamId = 2
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                        TeamId = 2
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                        TeamId = 3
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                        TeamId = 4
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                        TeamId = 4
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                        TeamId = 4
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3",
                        TeamId = 4
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                        TeamId = 4
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5",
                        TeamId = 4
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb6",
                        TeamId = 4
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb7",
                        TeamId = 4
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb8",
                        TeamId = 4
                    },
                    new TeamMember
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5",
                        TeamId = 3
                    }
                    );
                    context.SaveChanges();
                }
                if (!context.Tasks.Any())
                {
                    context.Tasks.AddRange(
                    new Task
                    {
                        TaskTitle = "Implementat functii utile",
                        TaskContent = "Vrem sa implementam functii pentru citre si scriere din fisier. De asemenea trebuie functii pentru convesia din string in int si invers.",
                        StatId = 2, // ar trebui sa fie Not Started
                        StartDate = new DateTime(2022, 12, 24, 8, 00, 00),
                        DueDate = new DateTime(2023, 3, 1, 12, 00, 00),
                        ProjectId = 3,
                        TeamMemberId = 17
                    },
                    new Task
                    {
                        TaskTitle = "Adaugat daemon-ul",
                        TaskContent = "Vrem sa adaugam fisierul cu codul pe care il va executa daemon-ul. Trebuie avut in vedere sablonul pentru un program daemon cat si cerintele date.",
                        StatId = 1, // ar trebui sa fie Not Assigned
                        StartDate = new DateTime(2022, 11, 08, 6, 00, 00),
                        DueDate = new DateTime(2023, 6, 1, 7, 00, 00),
                        ProjectId = 3
                    },
                    new Task
                    {
                        TaskTitle = "Adaugat script PATH",
                        TaskContent = "De scris un script care adauga programul da.c la calea de fisiere PATH. Astfel vom avea un proiect care poate fi rulat din orice director si nu neaparat din cel in care se afla programul.",
                        StatId = 3, // ar trebui sa fie In Progress
                        StartDate = new DateTime(2022, 11, 08, 6, 00, 00),
                        DueDate = new DateTime(2023, 6, 1, 7, 00, 00),
                        ProjectId = 3,
                        TeamMemberId = 17
                    },
                    new Task
                    {
                        TaskTitle = "Adaugat functia analiza",
                        TaskContent = "De scris functia care calculeaza dimensiunile de fisiere si foldere. Aceasta functie va scrie rezultatele intr-un fisiersi sub forma de procente.",
                        StatId = 4, // ar trebui sa fie Completed
                        StartDate = new DateTime(2022, 10, 07, 6, 00, 00),
                        DueDate = new DateTime(2022, 12, 14, 5, 00, 00),
                        ProjectId = 3,
                        TeamMemberId = 3
                    },
                    new Task
                    {
                        TaskTitle = "De adaugat documentatie",
                        TaskContent = "De scris pe scurt o descriere a proiectului impreuna cu etapele parcurse. La fiecare etapa se va scrie si persoana care s-a ocupat cu bucata aceea de cod. De preferat documentatia sa fie adaugata in README.md.",
                        StatId = 1, // ar trebui sa fie Not Assigned
                        StartDate = new DateTime(2022, 11, 08, 6, 00, 00),
                        DueDate = new DateTime(2023, 6, 1, 7, 00, 00),
                        ProjectId = 3,
                    },
                    new Task
                    {
                        TaskTitle = "Diagrama ER",
                        TaskContent = "De adaugat diagrama entitate relatie pentru realizarea proiectului",
                        StatId = 4, //completed
                        StartDate = new DateTime(2022, 12, 12, 20, 2, 33),
                        DueDate = new DateTime(2023, 1, 1, 1, 1, 1),
                        ProjectId = 1,
                        TeamMemberId = 1
                    },
                    new Task
                    {
                        TaskTitle = "Team MVC",
                        TaskContent = "De adaugat Model, View si Controller pentru Team, care replica functionalitatile unei echipe (adaugare membru, selectare proiect etc)",
                        StatId = 3, //progress
                        StartDate = new DateTime(2022, 12, 13, 13, 13, 13),
                        DueDate = new DateTime(2023, 10, 1, 1, 1, 1),
                        ProjectId = 1,
                        TeamMemberId = 2
                    },
                    new Task
                    {
                        TaskTitle = "Task MVC",
                        TaskContent = "De adaugat Model, View si Controller pentru Task, care replica functionalitatile unei task (asignare, reasignare, modificare data start/data final, modificare status etc)",
                        StatId = 3, //progress
                        StartDate = new DateTime(2022, 12, 13, 13, 13, 13),
                        DueDate = new DateTime(2023, 10, 1, 1, 1, 1),
                        ProjectId = 1,
                        TeamMemberId = 3
                    },
                    new Task
                    {
                        TaskTitle = "Design",
                        TaskContent = "De adaugat design in aplicatie",
                        StatId = 2, //not started
                        DueDate = new DateTime(2023, 10, 1, 1, 1, 1),
                        ProjectId = 1,
                        TeamMemberId = 3
                    },
                    new Task
                    {
                        TaskTitle = "Imagini User",
                        TaskContent = "Modificat proiectul astfel incat userii sa aiba poze de profil",
                        StatId = 1, //not assigned
                        ProjectId = 1
                    },
                    new Task
                    {
                        TaskTitle = "Adaugat triggere",
                        TaskContent = "De adaugat triggere la inserarea pe toate tabelele.",
                        StatId = 1, // ar trebui sa fie Not Assigned
                        StartDate = new DateTime(2022, 08, 13, 9, 00, 00),
                        DueDate = new DateTime(2023, 06, 12, 7, 30, 00),
                        ProjectId = 4
                    },
                    new Task
                    {
                        TaskTitle = "Adaugat un pachet",
                        TaskContent = "Vrem sa adaugam un pachet care inglobeaza toate functiile si subprogramele din aplicatie.",
                        StatId = 1, // ar trebui sa fie Not Assigned
                        StartDate = new DateTime(2022, 9, 08, 6, 00, 00),
                        DueDate = new DateTime(2023, 7, 1, 7, 00, 00),
                        ProjectId = 4
                    }
                    );
                    context.SaveChanges();
                }

                if (!context.Comments.Any())
                {
                    context.Comments.AddRange(
                        new Comment
                        {
                            CommentContent = "Cam cand estimezi ca te vei apuca de acest task pentru ca as avea si eu nevoie de aceste functii.",
                            CommentDate = new DateTime(2023, 1, 9, 8, 00, 00),
                            TaskId = 1, // ar trebui sa fie Not Started
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5",
                        },
                    new Comment
                    {
                        CommentContent = "As putea sa iau eu taskul acesta?",
                        CommentDate = new DateTime(2023, 1, 9, 8, 00, 00),
                        TaskId = 2, // ar trebui sa fie Not Assigned
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5",
                    },
                    new Comment
                    {
                        CommentContent = "Arata bine pana acum",
                        CommentDate = new DateTime(2023, 1, 9, 8, 00, 00),
                        TaskId = 3, // ar trebui sa fie Not Assigned
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                    },
                    new Comment
                    {
                        CommentContent = "Ai putea sa faci te rog fisier separat pentru constante?",
                        CommentDate = new DateTime(2023, 1, 9, 8, 00, 00),
                        TaskId = 4, // ar trebui sa fie Not Assigned
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                    },
                    new Comment
                    {
                        CommentContent = "Sigur, am adaugat acum",
                        CommentDate = new DateTime(2023, 1, 9, 8, 00, 00),
                        TaskId = 4, // ar trebui sa fie Not Assigned
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5",
                    },
                    new Comment
                    {
                        CommentContent = "Am adaugat model si controller pentru Team, dar view-urile nu sunt inca facute",
                        CommentDate = new DateTime(2022, 12, 19, 14, 12, 11),
                        TaskId = 7,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                    },
                    new Comment
                    {
                        CommentContent = "Cand poti pune si View-urile ca sa pot rula aplicatia",
                        CommentDate = new DateTime(2022, 12, 21, 14, 12, 11),
                        TaskId = 7,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    },
                    new Comment
                    {
                        CommentContent = "Nu cred ca mai am timp sa ma ocup si de design",
                        CommentDate = new DateTime(2023, 1, 9, 23, 12, 11),
                        TaskId = 9,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb7"
                    },
                    new Comment
                    {
                        CommentContent = "Too Late - predam proiectul",
                        CommentDate = new DateTime(2023, 12, 10, 16, 00, 00),
                        TaskId = 10,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    }
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}

