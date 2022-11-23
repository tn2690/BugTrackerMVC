using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    // status of the ticket
    public class TicketStatus
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string? Name { get; set; }
    }
}
