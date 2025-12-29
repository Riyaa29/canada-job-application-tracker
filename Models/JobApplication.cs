namespace JobApplicationTracker.Api.Models
{
    public class JobApplication
    {
        public int Id { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Status { get; set; } = "Applied";

        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        //foreign key
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}