using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSandbox.Data;
using SkillSandbox.Models;
using System.Security.Claims;

namespace SkillSandbox.Controllers
{
    [Authorize]
    public class AssessmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssessmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            // Get assessments that the user hasn't answered yet
            var userAnswers = await _context.AssessmentAnswers
                .Where(a => a.UserId == userId)
                .Select(a => a.AssessmentId)
                .Distinct()
                .ToListAsync();

            var availableAssessments = await _context.Assessments
                .Include(a => a.Questions)
                .Where(a => !userAnswers.Contains(a.Id))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(availableAssessments);
        }

        public async Task<IActionResult> TakeAssessment(int id)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Questions)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assessment == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            // Check if user has already answered this assessment
            var hasAnswered = await _context.AssessmentAnswers
                .AnyAsync(a => a.AssessmentId == id && a.UserId == userId);

            if (hasAnswered)
            {
                return RedirectToAction(nameof(AssessmentHistory));
            }

            return View(assessment);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAssessment(int id, List<AssessmentAnswer> answers)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Questions)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assessment == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.Users.FindAsync(userId);

            foreach (var answer in answers)
            {
                answer.AssessmentId = id;
                answer.UserId = userId;
                answer.Role = user.Role;
                answer.StartTime = DateTime.UtcNow;
                answer.Timestamp = DateTime.UtcNow;
            }

            _context.AssessmentAnswers.AddRange(answers);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AssessmentHistory));
        }

        public async Task<IActionResult> AssessmentHistory()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var answers = await _context.AssessmentAnswers
                .Include(a => a.Assessment)
                .Include(a => a.Question)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return View(answers);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReviewAnswers(int assessmentId)
        {
            var answers = await _context.AssessmentAnswers
                .Include(a => a.Assessment)
                .Include(a => a.Question)
                .Include(a => a.User)
                .Where(a => a.AssessmentId == assessmentId && a.CheckerId == null)
                .OrderBy(a => a.Timestamp)
                .ToListAsync();

            return View(answers);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SubmitReview(int answerId, decimal score, string feedback)
        {
            var answer = await _context.AssessmentAnswers.FindAsync(answerId);
            if (answer == null)
            {
                return NotFound();
            }

            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            answer.CheckerId = adminId;
            answer.Score = score;
            answer.Feedback = feedback;
            answer.EndTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ReviewAnswers), new { assessmentId = answer.AssessmentId });
        }
    }
} 