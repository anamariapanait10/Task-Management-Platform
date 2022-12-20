using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Continutul comentariului este obligatoriu")]
        public string CommentContent { get; set; }

        public DateTime CommentDate { get; set; }

        public int? TaskId { get; set; }
        public virtual Task? Task { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

    }
}
