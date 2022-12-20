using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManagementApp.Data;
using TaskManagementApp.Models;

namespace TaskManagementApp.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba mai mult de 5 caractere")]
        public string ProjectTitle { get; set; }

        [Required(ErrorMessage = "Descrierea proiectului este obligatorie")]
        public string ProjectContent { get; set; }

        public DateTime ProjectDate { get; set; }

        [Required(ErrorMessage = "Organizatorul proiectului este obligatoriu")]
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Task>? Tasks { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? UsersList { get; set; }

    }
}
