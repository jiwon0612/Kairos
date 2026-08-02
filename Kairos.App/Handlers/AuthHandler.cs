using Kairos.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Kairos.App.Handlers
{
    public class AuthHandler : DelegatingHandler
    {
        private const string TokenKey = "auth_token";
        private const string RefreshTokenKey = "refresh_token";
        private const string BaseUrl = "http://192.168.219.104:5011";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshed = await TryRefreshAsync();
                if (refreshed)
                {
                    var newToken = await SecureStorage.GetAsync(TokenKey);
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        private async Task<bool> TryRefreshAsync()
        {
            var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            using var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            var req = new RefreshRequest { RefreshToken = refreshToken };
            var response = await client.PostAsJsonAsync("/api/Auth/refresh", req);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result == null || string.IsNullOrEmpty(result.Token) || string.IsNullOrEmpty(result.RefreshToken))
                return false;

            await SecureStorage.SetAsync(TokenKey, result.Token);
            await SecureStorage.SetAsync(RefreshTokenKey, result.RefreshToken);
            return true;
        }

        private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);
            
            if (request.Content != null)
            {
                var contentStream = await request.Content.ReadAsStreamAsync();
                clone.Content = new StreamContent(contentStream);
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.Add(header.Key, header.Value);
                }
            }
            
            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return clone;
        }
    }
}
