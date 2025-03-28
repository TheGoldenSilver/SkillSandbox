using System.ComponentModel.DataAnnotations;

namespace SkillSandbox.Models
{
    public class AssessmentQuestion
    {
        public int Id { get; set; }

        [Required]
        public int AssessmentId { get; set; }
        public Assessment Assessment { get; set; }

        [Required]
        public string Question { get; set; }

        [Required]
        public string CorrectAnswer { get; set; }

        [Required]
        public int Points { get; set; }

        public string Explanation { get; set; }
    }
} 