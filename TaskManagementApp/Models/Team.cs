using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required(ErrorMessage = "Numele echipei este obligatoriu")]
        [StringLength(50, ErrorMessage = "Numele echipei nu poate avea mai mult de 50 de caractere")]
        [MinLength(5, ErrorMessage = "Numele echipei trebuie sa aiba mai mult de 5 caractere")]
        public string TeamName { get; set; }

        public DateTime TeamDate { get; set; }

        [Required(ErrorMessage = "Proiectul este obligatoriu")]
        public int? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public virtual ICollection<TeamMember>? TeamMembers { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? ProjectsList { get; set; }
    }
}
