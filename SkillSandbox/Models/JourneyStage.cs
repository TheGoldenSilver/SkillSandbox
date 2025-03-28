using System.ComponentModel.DataAnnotations;

namespace SkillSandbox.Models
{
    public class JourneyStage
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public JourneyStageType StageType { get; set; }

        [Required]
        public JourneyStageStatus Status { get; set; } = JourneyStageStatus.NotStarted;

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; }
        public string Feedback { get; set; }

        // Stage-specific data
        public string AssessmentResults { get; set; }
        public string ConsultationNotes { get; set; }
        public DateTime? ConsultationDate { get; set; }
        public string InterviewFeedback { get; set; }
        public string EnrollmentRequirements { get; set; }
        public string TrainingProgress { get; set; }
        public string InternshipDetails { get; set; }
        public string PlacementStatus { get; set; }

        // Navigation properties
        public virtual ICollection<JobApplication> JobApplications { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<RequirementChecklist> Requirements { get; set; }
    }

    public class JobApplication
    {
        public int Id { get; set; }
        public int JourneyStageId { get; set; }
        public JourneyStage JourneyStage { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Activity { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public JobApplicationStatus Status { get; set; }

        public string Remarks { get; set; }
    }

    public class Payment
    {
        public int Id { get; set; }
        public int JourneyStageId { get; set; }
        public JourneyStage JourneyStage { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? PaidDate { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        public string Notes { get; set; }
    }

    public class RequirementChecklist
    {
        public int Id { get; set; }
        public int JourneyStageId { get; set; }
        public JourneyStage JourneyStage { get; set; }

        [Required]
        public string Requirement { get; set; }

        [Required]
        public bool IsCompleted { get; set; }

        public DateTime? CompletedDate { get; set; }
        public string Notes { get; set; }
    }

    public enum JourneyStageType
    {
        JourneyStart,
        WorkReadinessAssessment,
        Consultation,
        Interviews,
        Enrollment,
        Training,
        Internship,
        Placement
    }

    public enum JourneyStageStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }

    public enum JobApplicationStatus
    {
        Submitted,
        UnderReview,
        InterviewScheduled,
        InterviewCompleted,
        Offered,
        Rejected,
        Withdrawn
    }

    public enum PaymentStatus
    {
        Pending,
        Partial,
        Completed,
        Overdue
    }
} 