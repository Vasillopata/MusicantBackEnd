using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicantBackEnd.Models;
using MusicantBackEnd.Models.InputModels;
using MusicantBackEnd.Services;

namespace MusicantBackEnd.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenService _tokenService;

        public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, ILogger<AuthController> logger
        , ITokenService tokenService)
        {
            _tokenService = tokenService;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterInputModel model)
        {
            var user = new AppUser
            {
                Id = 0,
                UserName = model.UserName,
                Email = model.Email,
                Pfp = null,
                Banner = null,
                CreatedDate = DateTime.UtcNow,
                BirthDate = model.BirthDate,
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest("Подсигурете се, че имейлът е форматиран правилно, а паролата съдържа: главна буква, цифра и символ.");
            }

            var token = await _tokenService.GenerateToken(user);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("token", token, cookieOptions);

            return new JsonResult(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Невалидни данни.");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user.IsLocked)
            {
                return Unauthorized("Този акаунт е заключен от администратор.");
            }
            if (user == null)
            {
                return Unauthorized("Този имейл не е регистриран.");
            }
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized("Грешна парола");
            }

            var token = await _tokenService.GenerateToken(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("token", token, cookieOptions);

            return new JsonResult(new { token });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("token");
            _logger.LogInformation("User logged out.");

            return Ok();
        }
    }
}

