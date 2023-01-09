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
                        ProjectTitle = "ArticolesApp",
                        ProjectContent = "Vrem sa creem o aplicatie sa permita utilizatorilor sa urmareasca stiri, sa citeasca articole si documentare, din diverse domenii care ii atrag.",
                        ProjectDate = new DateTime(2022, 10, 4, 16, 05, 30),
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                    }
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}