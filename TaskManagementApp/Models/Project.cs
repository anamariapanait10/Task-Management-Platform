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

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Project Title can not have more than 100 characters")]
        [MinLength(5, ErrorMessage = "Project Title must have at least 5 characters")]
        public string ProjectTitle { get; set; }

        [Required(ErrorMessage = "Project Content is required")]
        public string ProjectContent { get; set; }

        public DateTime ProjectDate { get; set; }

        [Required(ErrorMessage = "Project Organizer is required")]
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Task>? Tasks { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? UsersList { get; set; }

    }
}
