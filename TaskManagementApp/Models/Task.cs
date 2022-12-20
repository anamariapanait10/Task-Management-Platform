using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "Titlul task-ului este obligatoriu")]
        [StringLength(30, ErrorMessage = "Titlul task-ului nu poate avea mai mult de 30 de caractere")]
        [MinLength(5, ErrorMessage = "Titlul task-ului trebuie sa aiba mai mult de 5 caractere")]
        public string TaskTitle { get; set; }

        [Required(ErrorMessage = "Continutul task-ului este obligatoriu")]
        public string TaskContent { get; set; }

        public int? StatId { get; set; }
        public virtual Stat? Stat { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Taskul trebuie sa fie asignat unui proiect")]
        public int? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public int? TeamMemberId { get; set; }
        public virtual TeamMember? TeamMember { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? StatsList { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? ProjectsList { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? TeamMembersList { get; set; }
    }
}
