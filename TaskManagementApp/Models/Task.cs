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

        [Required(ErrorMessage = "Task Title is required")]
        [StringLength(30, ErrorMessage = "Task Title can not have more than 30 characters")]
        [MinLength(5, ErrorMessage = "Task Title must have at least 5 characters")]
        public string TaskTitle { get; set; }

        [Required(ErrorMessage = "Task Content is required")]
        public string TaskContent { get; set; }

        public int? StatId { get; set; }
        public virtual Stat? Stat { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "The task must be assigned to a project")]
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
