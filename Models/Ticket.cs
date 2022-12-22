using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    // ticket info
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ticket Title")]
        public string? Title { get; set; }

        [Required]
        [Display(Name = "Ticket Description")]
        [StringLength(2000)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Created")]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Updated")]
        public DateTime? Updated { get; set; }

        [Display(Name = "Archived?")]
        public bool Archived { get; set; }

        [Display(Name = "Archived by Project?")]
        public bool ArchivedByProject { get; set; }

        
        
        // ---------------- foreign keys ----------------

        public int ProjectId { get; set; }

        public int TicketTypeId {get; set; }

        public int TicketStatusId { get; set; }

        public int TicketPriorityId { get; set; }

        [Display(Name = "Developer")]
        public string? DeveloperUserId { get; set; }

        [Required]
        [Display(Name = "Submitter")]
        public string? SubmitterUserId { get; set; }

        // ---------------- foreign keys ----------------

        // ------------------------------------------ nav properties ------------------------------------------

        public virtual Project? Project { get; set; }

        [Display(Name = "Priority")]
        public virtual TicketPriority? TicketPriority { get; set; }

        [Display(Name = "Type")]
        public virtual TicketType? TicketType { get; set; }

        [Display(Name = "Status")]
        public virtual TicketStatus? TicketStatus { get; set; }

        [DisplayName("Ticket Developer")]
        public virtual BTUser? DeveloperUser { get; set; }

        [DisplayName("Submitted By")]
        public virtual BTUser? SubmitterUser { get; set; }

        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();

        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();

        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();

        public virtual ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();

        // ------------------------------------------ nav properties ------------------------------------------
    }
}
