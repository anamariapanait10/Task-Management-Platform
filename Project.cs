using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProjectApp.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul proiectului este obligatoriu")]
        [StringLength(50, ErrorMessage = "Titlul proiectului nu poate avea mai mult de 50 de caractere")]
        [MinLength(10, ErrorMessage = "Titlul proiectului trebuie sa aiba mai mult de 10 caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Rezumatul proiectului este obligatoriu")]
        public string Content { get; set; }

        //UserId poate fi null????
        [Required(ErrorMessage = "Organizatorul proiectului este obligatoriu")]
        public string UserId { get; set; }

        public string? TeamId { get; set; }

        public virtual ICollection<Task>? Tasks { get; set; }
    }
}
