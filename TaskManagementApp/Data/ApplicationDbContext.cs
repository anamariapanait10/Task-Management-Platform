using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;
using Task = TaskManagementApp.Models.Task;

namespace TaskManagementApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Stat> Stats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // definire relatii cu modelele Bookmark si Article (FK)
            modelBuilder.Entity<TeamMember>()
                        .HasOne(u => u.User)
                        .WithMany(tm => tm.TeamMembers)
                        .HasForeignKey(u => u.UserId)
                        .OnDelete(DeleteBehavior.ClientNoAction);

            modelBuilder.Entity<TeamMember>()
                         .HasOne(tm => tm.Team)
                         .WithMany(tm => tm.TeamMembers)
                         .HasForeignKey(tm => tm.TeamId)
                         .OnDelete(DeleteBehavior.ClientNoAction);
        }
    }

}