namespace JobApplicationTracker.Api.DTOs
{
    public class UpdateJobApplicationDto
    {
        public string Status { get; set; } = "Applied";
        public string? Notes { get; set; }
    }
}
