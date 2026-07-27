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
        private const string TokenKey = "auth_token";

        public ApiService()
        {
            _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        private async Task SetAuthHeaderAsync()
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization
                    = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task SaveTokenAsync(string token)
        {
            await SecureStorage.SetAsync(TokenKey, token);
        }

        public async Task<bool> IsLoggedInAsync()
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            return !string.IsNullOrEmpty(token);
        }

        public void Logout()
        {
            SecureStorage.Remove(TokenKey);
            _http.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<bool> RegisterAsync(string email, string password)
        {
            var request = new RegisterRequest { Email = email, Password = password };
            var response = await _http.PostAsJsonAsync("/api/Auth/register", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var request = new LoginRequest { Email = email, Password = password };
            var response = await _http.PostAsJsonAsync("/api/Auth/login", request);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result == null || string.IsNullOrEmpty(result.Token))
                return false;

            await SaveTokenAsync(result.Token);
            return true;
        }

        public async Task<List<ProjectResponse>> GetProjectsAsync()
        {
            await SetAuthHeaderAsync();
            var result = await _http.GetFromJsonAsync<List<ProjectResponse>>("/api/Project");
            return result ?? new List<ProjectResponse>();
        }

        public async Task CreateProjectAsync(string name)
        {
            await SetAuthHeaderAsync();
            var result = new CreateProjectRequest { Name = name };
            await _http.PostAsJsonAsync("/api/Project", result);
        }

        public async Task UpdateProjectAsync(int id, string name)
        {
            await SetAuthHeaderAsync();
            var result = new CreateProjectRequest { Name = name };
            await _http.PutAsJsonAsync($"/api/Project/{id}", result);
        }

        public async Task DeleteProjectAsync(int id)
        {
            await SetAuthHeaderAsync();
            await _http.DeleteAsync($"/api/Project/{id}");
        }

        public async Task UpdateTodoAsync(int id, string title)
        {
            await SetAuthHeaderAsync();
            var result = new CreateTodoRequest { Title = title };
            await _http.PutAsJsonAsync($"/api/Todo/{id}", result);
        }

        public async Task DeleteTodoAsync(int id)
        {
            await SetAuthHeaderAsync();
            await _http.DeleteAsync($"/api/Todo/{id}");
        }

        public async Task<List<TodoResponse>> GetTodosAsync()
        {
            await SetAuthHeaderAsync();
            var result = await _http.GetFromJsonAsync<List<TodoResponse>>("/api/Todo");
            return result ?? new List<TodoResponse>();
        }

        public async Task CreateTodoAsync(int projectId, string title)
        {
            await SetAuthHeaderAsync();
            var result = new CreateTodoRequest
            {
                ProjectID = projectId,
                Title = title
            };
            await _http.PostAsJsonAsync("/api/Todo", result);
        }

        public async Task SetCompletedAsync(int todoId,bool isCompleted)
        {
            await SetAuthHeaderAsync();
            var result = new SetCompletedRequest { IsCompleted = isCompleted };
            await _http.PutAsJsonAsync($"/api/Todo/{todoId}/completed", result);
        }
    }
}
