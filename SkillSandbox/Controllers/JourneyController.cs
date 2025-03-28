using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSandbox.Data;
using SkillSandbox.Models;

namespace SkillSandbox.Controllers
{
    [Authorize]
    public class JourneyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JourneyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var stages = await _context.JourneyStages
                .Include(j => j.JobApplications)
                .Include(j => j.Payments)
                .Include(j => j.Requirements)
                .Where(j => j.UserId == userId)
                .OrderBy(j => j.StageType)
                .ToListAsync();

            if (!stages.Any())
            {
                stages = await InitializeJourneyStages(userId);
            }

            return View(stages);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStageStatus(int stageId, JourneyStageStatus status, string notes = null)
        {
            var stage = await _context.JourneyStages.FindAsync(stageId);
            if (stage == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (stage.UserId != userId)
            {
                return Forbid();
            }

            stage.Status = status;
            stage.Notes = notes;

            if (status == JourneyStageStatus.InProgress && !stage.StartedAt.HasValue)
            {
                stage.StartedAt = DateTime.UtcNow;
            }
            else if (status == JourneyStageStatus.Completed)
            {
                stage.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddJobApplication(int stageId, JobApplication application)
        {
            var stage = await _context.JourneyStages.FindAsync(stageId);
            if (stage == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (stage.UserId != userId)
            {
                return Forbid();
            }

            application.JourneyStageId = stageId;
            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StageDetails), new { id = stageId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateJobApplicationStatus(int applicationId, JobApplicationStatus status)
        {
            var application = await _context.JobApplications.FindAsync(applicationId);
            if (application == null)
            {
                return NotFound();
            }

            var stage = await _context.JourneyStages.FindAsync(application.JourneyStageId);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (stage.UserId != userId)
            {
                return Forbid();
            }

            application.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StageDetails), new { id = application.JourneyStageId });
        }

        [HttpPost]
        public async Task<IActionResult> AddPayment(int stageId, Payment payment)
        {
            var stage = await _context.JourneyStages.FindAsync(stageId);
            if (stage == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (stage.UserId != userId)
            {
                return Forbid();
            }

            payment.JourneyStageId = stageId;
            payment.Status = PaymentStatus.Pending;
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StageDetails), new { id = stageId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePaymentStatus(int paymentId, PaymentStatus status)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return NotFound();
            }

            var stage = await _context.JourneyStages.FindAsync(payment.JourneyStageId);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (stage.UserId != userId)
            {
                return Forbid();
            }

            payment.Status = status;
            if (status == PaymentStatus.Completed)
            {
                payment.PaidDate = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StageDetails), new { id = payment.JourneyStageId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRequirementStatus(int requirementId, bool isCompleted)
        {
            var requirement = await _context.Requirements.FindAsync(requirementId);
            if (requirement == null)
            {
                return NotFound();
            }

            var stage = await _context.JourneyStages.FindAsync(requirement.JourneyStageId);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (stage.UserId != userId)
            {
                return Forbid();
            }

            requirement.IsCompleted = isCompleted;
            if (isCompleted)
            {
                requirement.CompletedDate = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StageDetails), new { id = requirement.JourneyStageId });
        }

        private async Task<List<JourneyStage>> InitializeJourneyStages(int userId)
        {
            var stages = new List<JourneyStage>();
            foreach (JourneyStageType stageType in Enum.GetValues(typeof(JourneyStageType)))
            {
                var stage = new JourneyStage
                {
                    UserId = userId,
                    StageType = stageType,
                    Status = JourneyStageStatus.NotStarted
                };

                // Initialize requirements based on stage type
                stage.Requirements = GetDefaultRequirements(stageType);
                stages.Add(stage);
            }

            _context.JourneyStages.AddRange(stages);
            await _context.SaveChangesAsync();

            return stages;
        }

        private List<RequirementChecklist> GetDefaultRequirements(JourneyStageType stageType)
        {
            var requirements = new List<RequirementChecklist>();
            switch (stageType)
            {
                case JourneyStageType.Enrollment:
                    requirements.AddRange(new[]
                    {
                        new RequirementChecklist { Requirement = "Submit ID Document" },
                        new RequirementChecklist { Requirement = "Submit Transcript of Records" },
                        new RequirementChecklist { Requirement = "Complete Enrollment Form" },
                        new RequirementChecklist { Requirement = "Sign Program Agreement" }
                    });
                    break;
                case JourneyStageType.Training:
                    requirements.AddRange(new[]
                    {
                        new RequirementChecklist { Requirement = "Complete Core Training Modules" },
                        new RequirementChecklist { Requirement = "Attend Workshops" },
                        new RequirementChecklist { Requirement = "Submit Assignments" },
                        new RequirementChecklist { Requirement = "Complete Assessments" }
                    });
                    break;
                // Add more stage-specific requirements as needed
            }
            return requirements;
        }

        public async Task<IActionResult> StageDetails(int id)
        {
            var stage = await _context.JourneyStages
                .Include(j => j.JobApplications)
                .Include(j => j.Payments)
                .Include(j => j.Requirements)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (stage == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (stage.UserId != userId)
            {
                return Forbid();
            }

            return View(stage);
        }
    }
} 