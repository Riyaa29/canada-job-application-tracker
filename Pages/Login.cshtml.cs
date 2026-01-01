using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;

namespace JobApplicationTracker.Api.Pages
{
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Email { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        public string Error { get; set; } = "";

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var payload = JsonSerializer.Serialize(new
            {
                email = Email.Trim(),
                password = Password.Trim()
            });

            var response = await client.PostAsync(
                "http://localhost:5130/api/auth/login",
                new StringContent(payload, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                Error = "Invalid credentials";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            var token = JsonDocument.Parse(json).RootElement.GetProperty("token").GetString();

            Response.Cookies.Append("jwt", token!);

            return RedirectToPage("/Jobs");
        }
    }
}
