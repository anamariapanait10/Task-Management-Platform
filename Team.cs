using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProjectApp.Models
{
    public class Team
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele echipei este obligatoriu")]
        [StringLength(50, ErrorMessage = "Numele echipei nu poate avea mai mult de 50 de caractere")]
        [MinLength(5, ErrorMessage = "Numele echipei trebuie sa aiba mai mult de 5 caractere")]
        public string Name { get; set; }
    }
}
