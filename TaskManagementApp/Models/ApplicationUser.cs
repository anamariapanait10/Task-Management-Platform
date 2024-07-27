using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<Project>? Projects { get; set; }

        public virtual ICollection<TeamMember>? TeamMembers { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
