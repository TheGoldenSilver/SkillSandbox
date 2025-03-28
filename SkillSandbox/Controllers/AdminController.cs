using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSandbox.Data;
using SkillSandbox.Models;
using System.Security.Claims;

namespace SkillSandbox.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var admin = await _context.Admins
                .Include(a => a.CreatedAssessments)
                .Include(a => a.CheckedAnswers)
                .FirstOrDefaultAsync(a => a.Id == adminId);

            var viewModel = new AdminDashboardViewModel
            {
                TotalAssessments = await _context.Assessments.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                PendingAnswers = await _context.AssessmentAnswers
                    .Where(a => a.CheckerId == null)
                    .CountAsync(),
                RecentAssessments = await _context.Assessments
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public IActionResult CreateAssessment()
        {
            return View(new Assessment());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssessment(Assessment assessment)
        {
            if (ModelState.IsValid)
            {
                var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                assessment.CreatedById = adminId;
                assessment.CreatedAt = DateTime.UtcNow;

                _context.Assessments.Add(assessment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageAssessments));
            }

            return View(assessment);
        }

        public async Task<IActionResult> ManageAssessments()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var assessments = await _context.Assessments
                .Include(a => a.Questions)
                .Where(a => a.CreatedById == adminId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(assessments);
        }

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

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users
                .Include(u => u.AssessmentAnswers)
                    .ThenInclude(a => a.Assessment)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(int userId, string email, string phoneNumber)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.Email = email;
            user.PhoneNumber = phoneNumber;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageUsers));
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalAssessments { get; set; }
        public int TotalUsers { get; set; }
        public int PendingAnswers { get; set; }
        public List<Assessment> RecentAssessments { get; set; }
    }
} 