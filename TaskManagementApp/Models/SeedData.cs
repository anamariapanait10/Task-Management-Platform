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

                if (context.Roles.Any())
                {
                    return; 
                }

                // Adding Roles into the Data Base
                context.Roles.AddRange(
                new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7211", Name = "User", NormalizedName = "User".ToUpper() }
                );

                // this instance will be used to create user's passwords
                // passwords are a hash
                var hasher = new PasswordHasher<ApplicationUser>();

                // Adding Users into the Data Base
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
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",  // primary key
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

                // Association of Users with Roles (USER-ROLE)
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
                        ProjectContent = "This project aims to design a task management platform that will be useful to companies working on various projects, ensuring the smooth functioning of the entire team." +
                                    "We want the application to allow users to register, and later, for them to create projects and teams, to add tasks and comments, which will allow the information to be updated in real time.",
                        ProjectDate = new DateTime(2022, 12, 12, 10, 4, 15),
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    },
                    new Project
                    {
                        ProjectTitle = "My Shell",
                        ProjectContent = "This project simulates a mini shell (like a bash shell)." + " We want the new shell to allow the functionalities of the basic elements found in any command line (e.g. command history, pipes, logical expressions, suspending a program).",
                        ProjectDate = new DateTime(2022, 10, 22, 22, 30, 15),
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb7"
                    },
                    new Project
                    {
                        ProjectTitle = "DiskAnalizer",
                        ProjectContent = "We want to implement a daemon that analyzes the space used on a storage device starting from a given path, and build a utility program that allows the use of this functionality" +
                                        "from the command line. The daemon must analyze the occupied space recursively for each directory it contains, regardless of depth.",
                        ProjectDate = new DateTime(2022, 10, 22, 17, 30, 15),
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4"
                    },
                    new Project
                    {
                        ProjectTitle = "Database Management",
                        ProjectContent = "We want to create a database (add tables, constraints) and add procedures, functions, triggers and packages to help manage it.",
                        ProjectDate = new DateTime(2022, 12, 24, 8, 00, 00),
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb6"
                    },
                    new Project
                    {
                        ProjectTitle = "ArticlesApp",
                        ProjectContent = "We want to create an application to allow users to follow news, read articles and documentaries, from various fields that attract them.",
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
                        TeamName = "Lab ASP",
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
                        TaskTitle = "Implement useful functions",
                        TaskContent = "We want to implement functions to read and write from the file. This should also work for converting a string to an int and vice versa.",
                        StatId = 2, 
                        StartDate = new DateTime(2022, 12, 24, 8, 00, 00),
                        DueDate = new DateTime(2023, 3, 1, 12, 00, 00),
                        ProjectId = 3,
                        TeamMemberId = 17
                    },
                    new Task
                    {
                        TaskTitle = "Add daemon",
                        TaskContent = "We want to add the file with the code that the daemon will execute. You must consider the template for a daemon program as well as the given requirements.",
                        StatId = 1,
                        StartDate = new DateTime(2022, 11, 08, 6, 00, 00),
                        DueDate = new DateTime(2023, 6, 1, 7, 00, 00),
                        ProjectId = 3
                    },
                    new Task
                    {
                        TaskTitle = "Add script PATH",
                        TaskContent = "Write a script that adds the 'da.c' program to the PATH file path. Thus we will have a project that can be run from any directory and not necessarily from the one where the program is located.\r\n",
                        StatId = 3,
                        StartDate = new DateTime(2022, 11, 08, 6, 00, 00),
                        DueDate = new DateTime(2023, 6, 1, 7, 00, 00),
                        ProjectId = 3,
                        TeamMemberId = 17
                    },
                    new Task
                    {
                        TaskTitle = "Add analytics features",
                        TaskContent = "Write the function that calculates the sizes of files and folders. This function will write the results in a file in the form of percentages.\r\n",
                        StatId = 4, 
                        StartDate = new DateTime(2022, 10, 07, 6, 00, 00),
                        DueDate = new DateTime(2022, 12, 14, 5, 00, 00),
                        ProjectId = 3,
                        TeamMemberId = 3
                    },
                    new Task
                    {
                        TaskTitle = "Add documentation",
                        TaskContent = "Write a brief description of the project together with the completed stages. At each stage, the person who dealt with that piece of code will also be written. Preferably, the documentation should be added to 'README.md'.",
                        StatId = 1, 
                        StartDate = new DateTime(2022, 11, 08, 6, 00, 00),
                        DueDate = new DateTime(2023, 6, 1, 7, 00, 00),
                        ProjectId = 3,
                    },
                    new Task
                    {
                        TaskTitle = "ERD",
                        TaskContent = "Add the Entity Relationship Diagram (ERD) for the project.",
                        StatId = 4,
                        StartDate = new DateTime(2022, 12, 12, 20, 2, 33),
                        DueDate = new DateTime(2023, 1, 1, 1, 1, 1),
                        ProjectId = 1,
                        TeamMemberId = 1
                    },
                    new Task
                    {
                        TaskTitle = "Team MVC",
                        TaskContent = "Add Model, View and Controller for TEAM, which replicates the functionalities of a team (adding a member, selecting a project etc.)",
                        StatId = 3,
                        StartDate = new DateTime(2022, 12, 13, 13, 13, 13),
                        DueDate = new DateTime(2023, 10, 1, 1, 1, 1),
                        ProjectId = 1,
                        TeamMemberId = 2
                    },
                    new Task
                    {
                        TaskTitle = "Task MVC",
                        TaskContent = "Add Model, View and Controller for TASK, which replicates the functionalities of a task (assigning, reassigning, changing start/end date, changing status etc.)",
                        StatId = 3,
                        StartDate = new DateTime(2022, 12, 13, 13, 13, 13),
                        DueDate = new DateTime(2023, 10, 1, 1, 1, 1),
                        ProjectId = 1,
                        TeamMemberId = 3
                    },
                    new Task
                    {
                        TaskTitle = "Design",
                        TaskContent = "Add design to the app.",
                        StatId = 2, 
                        DueDate = new DateTime(2023, 10, 1, 1, 1, 1),
                        ProjectId = 1,
                        TeamMemberId = 3
                    },
                    new Task
                    {
                        TaskTitle = "User's profile pictures",
                        TaskContent = "The project needs to be modified so that users have profile pictures.",
                        StatId = 1,
                        ProjectId = 1
                    },
                    new Task
                    {
                        TaskTitle = "Add triggers",
                        TaskContent = "Add triggers for INSERT for all tables.",
                        StatId = 1, 
                        StartDate = new DateTime(2022, 08, 13, 9, 00, 00),
                        DueDate = new DateTime(2023, 06, 12, 7, 30, 00),
                        ProjectId = 4
                    },
                    new Task
                    {
                        TaskTitle = "Add a package",
                        TaskContent = "We want to add a package that includes all the functions and subprograms in the application.",
                        StatId = 1, 
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
                            CommentContent = "When do you think you will start this task because I would also need these functions.",
                            CommentDate = new DateTime(2023, 1, 9, 8, 00, 00),
                            TaskId = 1, 
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5",
                        },
                    new Comment
                    {
                        CommentContent = "Can I take this task?",
                        CommentDate = new DateTime(2023, 1, 9, 8, 00, 00),
                        TaskId = 2, 
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5",
                    },
                    new Comment
                    {
                        CommentContent = "It looks good at this point!",
                        CommentDate = new DateTime(2023, 1, 9, 8, 00, 00),
                        TaskId = 3, 
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                    },
                    new Comment
                    {
                        CommentContent = "Can you make a separate file for the constants?",
                        CommentDate = new DateTime(2023, 1, 9, 8, 00, 00),
                        TaskId = 4,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                    },
                    new Comment
                    {
                        CommentContent = "Sure, I will add it now!",
                        CommentDate = new DateTime(2023, 1, 9, 8, 00, 00),
                        TaskId = 4, 
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5",
                    },
                    new Comment
                    {
                        CommentContent = "I have added the Model and the Controller for Team, but the the Views is not done yet",
                        CommentDate = new DateTime(2022, 12, 19, 14, 12, 11),
                        TaskId = 7,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                    },
                    new Comment
                    {
                        CommentContent = "When do you think you will add the View so I can run the app?",
                        CommentDate = new DateTime(2022, 12, 21, 14, 12, 11),
                        TaskId = 7,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    },
                    new Comment
                    {
                        CommentContent = "I don't think I will have time to make the design =(",
                        CommentDate = new DateTime(2023, 1, 9, 23, 12, 11),
                        TaskId = 9,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb7"
                    },
                    new Comment
                    {
                        CommentContent = "Too Late - we have to turn in the project",
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

