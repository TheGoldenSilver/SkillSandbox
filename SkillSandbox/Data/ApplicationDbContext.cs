using Microsoft.EntityFrameworkCore;
using SkillSandbox.Models;

namespace SkillSandbox.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }
        public DbSet<AssessmentAnswer> AssessmentAnswers { get; set; }
        public DbSet<JourneyStage> JourneyStages { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<RequirementChecklist> Requirements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Assessment>()
                .HasOne(a => a.CreatedBy)
                .WithMany(a => a.CreatedAssessments)
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssessmentQuestion>()
                .HasOne(q => q.Assessment)
                .WithMany(a => a.Questions)
                .HasForeignKey(q => q.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssessmentAnswer>()
                .HasOne(a => a.Assessment)
                .WithMany(a => a.Answers)
                .HasForeignKey(a => a.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssessmentAnswer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssessmentAnswer>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssessmentAnswer>()
                .HasOne(a => a.Checker)
                .WithMany(a => a.CheckedAnswers)
                .HasForeignKey(a => a.CheckerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JourneyStage>()
                .HasOne(j => j.User)
                .WithMany()
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobApplication>()
                .HasOne(j => j.JourneyStage)
                .WithMany(j => j.JobApplications)
                .HasForeignKey(j => j.JourneyStageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.JourneyStage)
                .WithMany(j => j.Payments)
                .HasForeignKey(p => p.JourneyStageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RequirementChecklist>()
                .HasOne(r => r.JourneyStage)
                .WithMany(j => j.Requirements)
                .HasForeignKey(r => r.JourneyStageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}