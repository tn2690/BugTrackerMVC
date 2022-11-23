using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    // the history of a ticket
    public class TicketHistory
    {
        public int Id { get; set; }

        [Display(Name = "Updated Ticket Property")]
        public string? PropertyName { get; set; }

        [Display(Name = "Ticket Description")]
        [StringLength(5000)]
        public string? Description { get; set; }

        [Display(Name = "Date Created")]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [Display(Name = "Previous Value")]
        public string? OldValue { get; set; }

        [Display(Name = "Current Value")]
        public string? NewValue { get; set; }



        // ---------------- foreign keys ----------------

        [Required]
        public string? BTUserId { get; set; }

        public int TicketId { get; set; }

        // ---------------- foreign keys ----------------

        // --------------- nav properties ---------------

        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? BTUser { get; set; }

        // --------------- nav properties ---------------
    }
}
