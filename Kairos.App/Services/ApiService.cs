using Kairos.App.Handlers;
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
        //private const string BaseUrl = "https://localhost:7107";
        //private const string BaseUrl = "http://192.168.219.104:5011";
        //private const string BaseUrl = "https://kairos-api-7b6v.onrender.com";
        private const string TokenKey = "auth_token";
        private const string RefreshTokenKey = "refresh_token";

        public ApiService()
        {
            var handler = new AuthHandler { InnerHandler = new HttpClientHandler() };
            _http = new HttpClient(handler) { BaseAddress = new Uri(AppConfig.BaseUrl) };
            bool test = TodayFilterSettings.UseStarred;
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

        private async Task SaveRefreshTokenAsync(string refreshToken)
        {
            await SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
        }

        public async Task<bool> IsLoggedInAsync()
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            return !string.IsNullOrEmpty(token);
        }

        public void Logout()
        {
            SecureStorage.Remove(TokenKey);
            SecureStorage.Remove(RefreshTokenKey);
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
            await SaveRefreshTokenAsync(result.RefreshToken);

            return true;
        }

        private async Task<bool> RefreshAsync()
        {
            var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            var request = new RefreshRequest { RefreshToken = refreshToken };
            var response = await _http.PostAsJsonAsync("/api/Auth/refresh", request);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result == null || string.IsNullOrEmpty(result.Token))
                return false;

            await SaveTokenAsync(result.Token);
            await SaveRefreshTokenAsync(result.RefreshToken);
            return true;
        }

        public async Task<bool> GoogleLoginAsync(string idToken)
        {
            var request = new GoogleLoginRequest { IdToken = idToken };
            var response = await _http.PostAsJsonAsync("/api/Auth/google", request);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result == null || string.IsNullOrEmpty(result.Token))
                return false;

            await SaveTokenAsync(result.Token);
            await SaveRefreshTokenAsync(result.RefreshToken);
            return true;
        }

        private async Task<HttpResponseMessage> SendWithRefreshAsync(Func<Task<HttpResponseMessage>> requestFunc)
        {
            var response = await requestFunc();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshed = await RefreshAsync();

                if (refreshed)
                {
                    await SetAuthHeaderAsync();
                    response = await requestFunc();
                }
            }

            return response;
        }

        public async Task<List<ProjectResponse>> GetProjectsAsync()
        {
            //await SetAuthHeaderAsync();
            var result = await _http.GetFromJsonAsync<List<ProjectResponse>>("/api/Project");
            return result ?? new List<ProjectResponse>();
        }

        public async Task CreateProjectAsync(string name)
        {
            //await SetAuthHeaderAsync();
            var result = new CreateProjectRequest { Name = name };
            await _http.PostAsJsonAsync("/api/Project", result);
        }

        public async Task UpdateProjectAsync(int id, string name)
        {
            //await SetAuthHeaderAsync();
            var result = new CreateProjectRequest { Name = name };
            await _http.PutAsJsonAsync($"/api/Project/{id}", result);
        }

        public async Task DeleteProjectAsync(int id)
        {
            //await SetAuthHeaderAsync();
            await _http.DeleteAsync($"/api/Project/{id}");
        }

        public async Task UpdateTodoAsync(int id, string title, int priority)
        {
            //await SetAuthHeaderAsync();
            var result = new UpdateTodoRequest { Title = title, Priority = priority };
            var response = await _http.PutAsJsonAsync($"/api/Todo/{id}", result);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteTodoAsync(int id)
        {
            //await SetAuthHeaderAsync();
            await _http.DeleteAsync($"/api/Todo/{id}");
        }

        public async Task<List<TodoResponse>> GetTodosAsync()
        {
            //await SetAuthHeaderAsync();
            var result = await _http.GetFromJsonAsync<List<TodoResponse>>("/api/Todo");
            return result ?? new List<TodoResponse>();
        }

        public async Task CreateTodoAsync(int projectId, string title, int priority,DateTime? dueDate, bool hasDueTime)
        {
            //await SetAuthHeaderAsync();
            var result = new CreateTodoRequest
            {
                ProjectID = projectId,
                Title = title,
                Priority = priority,
                DueDate = dueDate,
                HasDueTime = hasDueTime
            };
            await _http.PostAsJsonAsync("/api/Todo", result);
        }

        public async Task SetCompletedAsync(int todoId, bool isCompleted)
        {
            //await SetAuthHeaderAsync();
            var result = new SetCompletedRequest { IsCompleted = isCompleted };
            await _http.PutAsJsonAsync($"/api/Todo/{todoId}/completed", result);
        }

        public async Task<List<TodoResponse>> GetTodayTodosAsync()
        {
            //await SetAuthHeaderAsync();
            var result = await _http.GetFromJsonAsync<List<TodoResponse>>("/api/Todo/today");
            return result ?? new List<TodoResponse>();
        }

        public async Task SetProjectTodayAsync(int id, bool isToday)
        {
            //await SetAuthHeaderAsync();
            var result = new SetTodayRequest { IsToday = isToday };
            await _http.PatchAsJsonAsync($"/api/Project/{id}/today", result);
        }
    }
}
