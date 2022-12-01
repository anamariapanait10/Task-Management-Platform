using System.ComponentModel.DataAnnotations;

namespace FinalProjectApp.Models
{
    public class TeamMember
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        public int? TeamId { get; set; }
    }
}
