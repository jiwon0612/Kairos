using Kairos.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Kairos.App.Services
{
    internal class ApiService
    {
        private readonly HttpClient _http;

        private const string BaseUrl = "https://localhost:7107";

        public ApiService()
        {
            _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public async Task<List<ProjectResponse>> GetProjectsAsync()
        {
            var result = await _http.GetFromJsonAsync<List<ProjectResponse>>("/api/Project");
            return result ?? new List<ProjectResponse>();
        }

        public async Task CreateProjectAsync(string name)
        {
            var result = new CreateProjectRequest { Name = name };
            await _http.PostAsJsonAsync("/api/Project", result);
        }
    }
}
