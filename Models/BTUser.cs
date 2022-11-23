using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTrackerMVC.Models
{
    // extend functionality from IdentityUser to a BT user with user image
    public class BTUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFormFile { get; set; }

        [Display(Name = "User Image")]
        public byte[]? ImageFileData { get; set; }

        [Display(Name = "File Extension")]
        public string? ImageFileType { get; set; }

        // foreign key
        public int CompanyId { get; set; }

        // nav properties
        public virtual Company? Company { get; set; }

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    }
}
