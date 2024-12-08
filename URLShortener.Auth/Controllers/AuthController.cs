using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using URLShortener.Auth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace URLShortener.Auth.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName // Set other fields
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User registered successfully" });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid credentials" });

            // Generate JWT token
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["JwtSettings:Issuer"],
                _configuration["JwtSettings:Audience"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback)),
                Items =
                {
                    { "LoginProvider", "Google" },
                    { "scheme", "Google" }
                }
            };
            
            return Challenge(properties, "Google");
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                var authenticateResult = await HttpContext.AuthenticateAsync("Google");
                if (!authenticateResult.Succeeded)
                    return Redirect($"{_configuration["FrontendUrl"]}/auth-callback?error=Authentication failed");

                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                    return Redirect($"{_configuration["FrontendUrl"]}/auth-callback?error=External login info not found");

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                        return Redirect($"{_configuration["FrontendUrl"]}/auth-callback?error=Failed to create user");

                    await _userManager.AddLoginAsync(user, info);
                }

                var token = GenerateJwtToken(user);
                return Redirect($"{_configuration["FrontendUrl"]}/auth-callback?token={token}");
            }
            catch (Exception ex)
            {
                return Redirect($"{_configuration["FrontendUrl"]}/auth-callback?error={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            user.FullName = request.FullName;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to update profile" });

            return Ok(new { message = "Profile updated successfully" });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to change password. Please check your current password." });

            return Ok(new { message = "Password changed successfully" });
        }

        [Authorize]
        [HttpPost("update-avatar")]
        public async Task<IActionResult> UpdateAvatar([FromForm] IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
                return BadRequest(new { message = "No avatar file provided" });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            try
            {
                using var memoryStream = new MemoryStream();
                await avatar.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                
                // Convert to Base64 for storage
                user.Avatar = Convert.ToBase64String(imageBytes);
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return BadRequest(new { message = "Failed to update avatar" });

                return Ok(new { message = "Avatar updated successfully" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Failed to process avatar image" });
            }
        }
    }

    public class UpdateProfileRequest
    {
        [Required]
        public string FullName { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}
