using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class TeamMember
    {
        [Key]
        public int TeamMemberId { get; set; }

        [Required(ErrorMessage = "A user must be added to the team")]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "The team must exist")]
        public int? TeamId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual Team? Team { get; set; }

        public virtual ICollection<Task>? Tasks { get; set; }
    }
}
