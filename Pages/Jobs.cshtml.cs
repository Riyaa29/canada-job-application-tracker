using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace JobApplicationTracker.Api.Pages
{
    [IgnoreAntiforgeryToken]
    public class JobsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public JobsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<JobDto> Jobs { get; set; } = new();

        [BindProperty]
        public string CompanyName { get; set; } = "";

        [BindProperty]
        public string Role { get; set; } = "";

        [BindProperty]
        public string City { get; set; } = "";

        public async Task OnGetAsync()
        {
            await LoadJobs();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = Request.Cookies["jwt"];
            if (token == null) return RedirectToPage("/Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var payload = JsonSerializer.Serialize(new
            {
                companyName = CompanyName,
                role = Role,
                city = City
            });

            await client.PostAsync(
                "http://localhost:5130/api/job-applications",
                new StringContent(payload, Encoding.UTF8, "application/json")
            );

            return RedirectToPage();
        }

        private async Task LoadJobs()
        {
            var token = Request.Cookies["jwt"];
            if (token == null) return;

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetStringAsync(
                "http://localhost:5130/api/job-applications");

            Jobs = JsonSerializer.Deserialize<List<JobDto>>(
                response,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            )!;

        }
    }

    public class JobDto
    {
        public string CompanyName { get; set; } = "";
        public string Role { get; set; } = "";
        public string City { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
