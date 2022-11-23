using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    // the degree of importance for a project
    public class ProjectPriority
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Priority")]
        public string? Name { get; set; }
    }
}
