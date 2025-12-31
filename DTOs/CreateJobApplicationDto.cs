namespace JobApplicationTracker.Api.DTOs
{
    public class CreateJobApplicationDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
