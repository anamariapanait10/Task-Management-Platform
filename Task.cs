using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProjectApp.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul task-ului este obligatoriu")]
        [StringLength(30, ErrorMessage = "Titlul task-ului nu poate avea mai mult de 30 de caractere")]
        [MinLength(2, ErrorMessage = "Titlul task-ului trebuie sa aiba mai mult de 2 caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Continutul task-ului este obligatoriu")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Statusul task-ului este obligatoriu")]
        public string Status { get; set; }

        public DateTime? StarteDate { get; set; }

        public DateTime? DueDate { get; set; }

        public int? TeamMemberId { get; set; }

        public int ProjectId { get; set; }
    }
}
