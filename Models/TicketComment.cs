using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    // comments for a ticket
    public class TicketComment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Member Comment")]
        [StringLength(5000)]
        public string? Comment { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Created")]
        public DateTime Created { get; set; }



        // ---------------- foreign keys ----------------
        public int TicketId { get; set; }

        [Required]
        public string? BTUserId { get; set; }

        // ---------------- foreign keys ----------------

        // --------------- nav properties ---------------

        public virtual Ticket? Ticket { get; set; }

        [Display(Name = "Team Member")]
        public virtual BTUser? BTUser { get; set; }

        // --------------- nav properties ---------------
    }
}
