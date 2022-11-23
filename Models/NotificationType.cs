using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    // type of notification
    public class NotificationType
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Notification Type")]
        public string? Name { get; set; }
    }
}
