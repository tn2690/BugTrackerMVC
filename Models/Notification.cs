using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    // notification info
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Notification Title")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        [StringLength(1500, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Message { get; set; }

        [Display(Name = "Date Created")]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [Display(Name = "Viewed?")]
        public bool HasBeenViewed { get; set; }



        // ---------------- foreign keys ----------------

        public int? ProjectId { get; set; }

        public int? TicketId { get; set; }

        [Required]
        public string? SenderId { get; set; }

        [Required]
        public string? RecipientId { get; set; }

        [Display(Name = "Notification Type Id")]
        public int NotificationTypeId { get; set; }

        // ---------------- foreign keys ----------------

        // --------------- nav properties ---------------

        public virtual NotificationType? NotificationType { get; set; }

        public virtual Ticket? Ticket { get; set; }

        public virtual Project? Project { get; set; }

        public virtual BTUser? Sender { get; set; }

        public virtual BTUser? Recipient { get; set; }

        // --------------- nav properties ---------------
    }
}
