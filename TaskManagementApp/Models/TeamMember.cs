using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class TeamMember
    {
        [Key]
        public int TeamMemberId { get; set; }

        [Required(ErrorMessage = "Trebuie introdus un user intr-o echipa")]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Trebuie sa existe echipa")]
        public int? TeamId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual Team? Team { get; set; }

        public virtual ICollection<Task>? Tasks { get; set; }
    }
}
