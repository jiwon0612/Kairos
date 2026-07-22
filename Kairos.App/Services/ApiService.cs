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

        public async Task<List<TodoResponse>> GetTodosAsync()
        {
            var result = await _http.GetFromJsonAsync<List<TodoResponse>>("/api/Todo");
            return result ?? new List<TodoResponse>();
        }

        public async Task CreateTodoAsync(int projectId, string title)
        {
            var result = new CreateTodoRequest
            {
                ProjectID = projectId,
                Title = title
            };
            await _http.PostAsJsonAsync("/api/Todo", result);
        }

        public async Task SetCompletedAsync(int todoId,bool isCompleted)
        {
            var result = new SetCompletedRequest { IsCompleted = isCompleted };
            await _http.PutAsJsonAsync($"/api/Todo/{todoId}/completed", result);
        }
    }
}
