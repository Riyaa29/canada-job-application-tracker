using JobApplicationTracker.Api.Data;
using JobApplicationTracker.Api.DTOs;
using JobApplicationTracker.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobApplicationTracker.Api.Controllers
{
    [ApiController]
    [Route("api/job-applications")]
    [Authorize]
    public class JobApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper to get logged-in user id from JWT
        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateJobApplicationDto dto)
        {
            var job = new JobApplication
            {
                CompanyName = dto.CompanyName,
                Role = dto.Role,
                City = dto.City,
                Notes = dto.Notes,
                UserId = GetUserId()
            };

            _context.JobApplications.Add(job);
            await _context.SaveChangesAsync();

            return Ok(job);
        }

        // READ (with filters)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? city, [FromQuery] string? status)
        {
            var userId = GetUserId();

            var query = _context.JobApplications
                .Where(j => j.UserId == userId);

            if (!string.IsNullOrEmpty(city))
                query = query.Where(j => j.City == city);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(j => j.Status == status);

            var results = await query
                .OrderByDescending(j => j.ApplicationDate)
                .ToListAsync();

            return Ok(results);
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateJobApplicationDto dto)
        {
            var userId = GetUserId();

            var job = await _context.JobApplications
                .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

            if (job == null)
                return NotFound();

            job.Status = dto.Status;
            job.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return Ok(job);
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();

            var job = await _context.JobApplications
                .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

            if (job == null)
                return NotFound();

            _context.JobApplications.Remove(job);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
