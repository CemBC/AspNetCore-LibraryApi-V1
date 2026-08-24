using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using FluentValidation;
using LibraryApi.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LibraryApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterRequest> _registerRequestValidator;

        private readonly IValidator<LoginRequest> _loginRequestValidator;

        public AuthController(IAuthService authService, IValidator<RegisterRequest> registerRequestValidator, IValidator<LoginRequest> loginRequestValidator)
        {
            _authService = authService;
            _registerRequestValidator = registerRequestValidator;
            _loginRequestValidator = loginRequestValidator;
        }


        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var validationResult = await _loginRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            AuthResponse response = await _authService.LoginAsync(request);

            return Ok(response);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
        {
            string? userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (userId is null)
                return Unauthorized();


            CurrentUserResponse response =
                await _authService.GetCurrentUserAsync(
                    int.Parse(userId));


            return Ok(response);
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string? UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (UserId is null) return Unauthorized();

            await _authService.LogoutAsync(int.Parse(UserId));

            return NoContent();
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> RefreshToken(RefreshTokenRequest request)
        {
            AuthResponse response = await _authService.RefreshTokenAsync(request);

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var validationResult = await _registerRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await _authService.RegisterAsync(request);

            return StatusCode(StatusCodes.Status201Created);
        }
    }
}
