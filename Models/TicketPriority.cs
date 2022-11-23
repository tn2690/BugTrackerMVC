using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    // the order of importance for a ticket
    public class TicketPriority
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ticket Priority")]
        public string? Name { get; set; }
    }
}
