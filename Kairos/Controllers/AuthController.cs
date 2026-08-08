using Google.Apis.Auth;
using Kairos.Api.Models;
using Kairos.Data;
using Kairos.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Kairos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly KairosDbContext _context;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config, KairosDbContext context)
        {
            _userManager = userManager;
            _config = config;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            return Ok(new { message = "회원가입 성공", userId = user.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null )
                return Unauthorized(new { message = "이메일 또는 비밀번호가 올바르지 않습니다." });

            var valid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!valid)
                return Unauthorized(new { message = "이메일 또는 비밀번호가 올바르지 않습니다." });

            var accessToken = GenerateJwtToken(user);

            var refreshToken = await CreateRefreshTokenAsync(user.Id);

            return Ok(new LoginResponse { Token = accessToken, RefreshToken = refreshToken });
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { _config["Google:ClientId"]!, _config["Google:AndroidClientId"]! }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            catch 
            {
                return Unauthorized(new { message = "유효하지 않은 Google 토큰입니다." });
            }

            var email = payload.Email;
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    DisplayName = payload.Name
                };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(new { message = "사용자 생성 실패" });
            }

            var token = GenerateJwtToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.Id);
            return Ok(new LoginResponse { Token = token, RefreshToken = refreshToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequest request)
        {
            var stored = _context.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);

            if (stored == null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
                return Unauthorized(new { message = "유효하지 않은 리프레시 토큰입니다." });

            var user = await _userManager.FindByIdAsync(stored.UserID);
            if (user == null)
                return Unauthorized(new { message = "사용자를 찾을 수 없습니다." });

            var newAccessToken = GenerateJwtToken(user);

            stored.IsRevoked = true;
            var newRefreshToken = await CreateRefreshTokenAsync(user.Id);
            await _context.SaveChangesAsync();

            return Ok(new LoginResponse { Token = newAccessToken, RefreshToken = newRefreshToken });
        }

        private async Task<string> CreateRefreshTokenAsync(string userId)
        {
            var token = GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Token = token,
                UserID = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(14),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return token;
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                //expires: DateTime.Now.AddMinutes(1),

                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
