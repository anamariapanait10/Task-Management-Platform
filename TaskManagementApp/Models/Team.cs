using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required(ErrorMessage = "Team Name is required")]
        [StringLength(50, ErrorMessage = "Team Name can not have more than 50 characters")]
        [MinLength(5, ErrorMessage = "Team Name must have at least 5 characters")]
        public string TeamName { get; set; }

        public DateTime TeamDate { get; set; }

        [Required(ErrorMessage = "The team must have a project assigned")]
        public int? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public virtual ICollection<TeamMember>? TeamMembers { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? ProjectsList { get; set; }
    }
}
