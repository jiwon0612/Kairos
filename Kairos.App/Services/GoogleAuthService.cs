using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kairos.App.Services
{
    internal class GoogleAuthService
    {
        private const string ClientId = "313140633739-5upd33vp2qj1ie38ti9g0suq797fb332.apps.googleusercontent.com";
        private const string ClientSecret = "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_client_secret";

        private const string AuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";

        public async Task<string?> SignInAsync()
        {
            var listener = new HttpListener();
            int port = GetRandomUnusedPort();
            string redirectUri = $"http://localhost:{port}/";
            listener.Prefixes.Add(redirectUri);
            listener.Start();

            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = GenerateCodeChallenge(codeVerifier);

            string authUrl =
                $"{AuthEndpoint}?" +
                $"client_id={ClientId}&" +
                $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                $"response_type=code&" +
                $"scope={Uri.EscapeDataString("openid email profile")}&" +
                $"code_challenge={codeChallenge}&" +
                $"code_challenge_method=S256";

            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true,
            });

            var context = await listener.GetContextAsync();
            string? code = context.Request.QueryString["code"];

            string html = "<html><body>로그인 완료! 앱으로 돌아가세요.</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer);
            context.Response.OutputStream.Close();
            listener.Stop();

            if (string.IsNullOrEmpty(code))
                return null;

            return await ExchangeCodeForIdTokenAsync(code, codeVerifier, redirectUri);
        }

        private async Task<string?> ExchangeCodeForIdTokenAsync(string code, string codeVerifier, string redirectUri)
        {
            using var http = new HttpClient();
            var values = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code",
                ["code_verifier"] = codeVerifier
            };

            var response = await http.PostAsync(TokenEndpoint, new FormUrlEncodedContent(values));

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("id_token").GetString();
        }

        private int GetRandomUnusedPort()
        {
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Base64UrlEncode(bytes);
        }

        private string GenerateCodeChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
            return Base64UrlEncode(hash);
        }

        private string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=').Replace('+','-').Replace('/','_');
        }
    }
}
