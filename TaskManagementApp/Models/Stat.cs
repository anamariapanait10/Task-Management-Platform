using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class Stat
    {
        [Key]
        public int StatId { get; set; }

        [Required(ErrorMessage = "Status Name is required")]
        public string StatName { get; set; }
    }
}
