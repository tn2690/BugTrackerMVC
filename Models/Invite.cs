using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    // invitations to join
    public class Invite
    {
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Sent")]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Joined")]
        public DateTime? JoinDate { get; set; }

        public Guid CompanyToken { get; set; }

        [Required]
        [Display(Name = "Invitee Email")]
        public string? InviteeEmail { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? InviteeFirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? InviteeLastName { get; set; }

        [StringLength(1500)]
        public string? Message { get; set; }

        public bool IsValid { get; set; }

        
        // ---------------- foreign keys ----------------

        public int CompanyId { get; set; }

        public int ProjectId { get; set; }

        [Required]
        public string? InvitorId { get; set; }

        public string? InviteeId { get; set; }

        // ---------------- foreign keys ----------------

        // --------------- nav properties ---------------

        public virtual Company? Company { get; set; }

        public virtual Project? Project { get; set; }

        public virtual BTUser? Invitor { get; set; }

        public virtual BTUser? Invitee { get; set; }

        // --------------- nav properties ---------------
    }
}
