using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTrackerMVC.Models
{
    // company info with image
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Name { get; set; }

        [Display(Name = "Company Description")]
        [StringLength(1800)]
        public string? Description { get; set; }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }

        [Display(Name = "Company Image")]
        public byte[]? ImageFileData { get; set; }

        [Display(Name = "File Extension")]
        public string? ImageFileType { get; set; }

        
        
        // ------------------------------------------ nav properties ------------------------------------------

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();

        // ------------------------------------------ nav properties ------------------------------------------
    }
}
