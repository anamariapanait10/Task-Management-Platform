using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class Stat
    {
        [Key]
        public int StatId { get; set; }

        [Required(ErrorMessage = "Numele statusului este obligatoriu")]
        public string StatName { get; set; }
    }
}
