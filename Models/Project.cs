using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTrackerMVC.Models
{
    // project info
    public class Project
    {
        public int Id { get; set; }

        // foreign key
        public int CompanyId { get; set; }

        [Required]
        [Display(Name = "Project Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Name { get; set; }

        [Required]
        [Display(Name = "Project Description")]
        [StringLength(2500)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Created")]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        // foreign key
        public int ProjectPriorityId { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFormFile { get; set; }

        [Display(Name = "Project Image")]
        public byte[]? ImageFileData { get; set; }

        [Display(Name = "File Extension")]
        public string? ImageFileType { get; set; }

        public bool Archived { get; set; }

        // ------------------------------------------ nav properties ------------------------------------------

        public virtual Company? Company { get; set; }

        public virtual ProjectPriority? ProjectPriority { get; set; }
        
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();

        // ------------------------------------------ nav properties ------------------------------------------
    }
}
