using System.ComponentModel.DataAnnotations;

namespace SkillSandbox.Models
{
    public class Assessment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedById { get; set; }
        public Admin CreatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<AssessmentQuestion> Questions { get; set; }
        public virtual ICollection<AssessmentAnswer> Answers { get; set; }
    }

    public class AssessmentQuestion
    {
        public int Id { get; set; }
        public int AssessmentId { get; set; }
        public Assessment Assessment { get; set; }

        [Required]
        public string Question { get; set; }

        // Navigation property
        public virtual ICollection<AssessmentAnswer> Answers { get; set; }
    }

    public class AssessmentAnswer
    {
        public int Id { get; set; }
        public int AssessmentId { get; set; }
        public Assessment Assessment { get; set; }
        public int QuestionId { get; set; }
        public AssessmentQuestion Question { get; set; }
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Answer { get; set; }
        public string Role { get; set; }
        public int? CheckerId { get; set; }
        public Admin Checker { get; set; }
        public decimal? Score { get; set; }
        public string Feedback { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
} 