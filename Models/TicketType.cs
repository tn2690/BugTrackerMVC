using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    // type of ticket
    public class TicketType
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ticket Type")]
        public string? Name { get; set; }
    }
}
