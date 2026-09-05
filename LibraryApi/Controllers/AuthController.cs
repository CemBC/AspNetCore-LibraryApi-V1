using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using LibraryApi.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LibraryApi.Services.Interfaces;

namespace LibraryApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;
        private readonly IValidator<RegisterRequest> _registerRequestValidator;

        private readonly IValidator<LoginRequest> _loginRequestValidator;

        private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;

        private readonly IValidator<ChangePasswordRequest> _changePasswordRequestValidator;

        private readonly IValidator<ForgotPasswordRequest> _forgotPasswordRequestValidator;
        private readonly IValidator<ResetPasswordRequest> _resetPasswordRequestValidator;

        private readonly IValidator<VerifyEmailRequest> _verifyEmailRequestValidator;
        private readonly IValidator<ResendVerificationRequest> _resendVerificationRequestValidator;

        private readonly IValidator<SendChangePasswordCodeRequest> _sendChangePasswordCodeRequestValidator;
        
        private readonly IValidator<SendChangeEmailCodeRequest> _sendChangeEmailCodeRequestValidator;

        private readonly IValidator<ChangeEmailRequest> _changeEmailRequestValidator;


        public AuthController(IAuthService authService, IValidator<RegisterRequest> registerRequestValidator, 
            IValidator<LoginRequest> loginRequestValidator, IValidator<UpdateProfileRequest> updateProfileValidator,
            IValidator<ChangePasswordRequest> changePasswordRequestValidator , IValidator<ForgotPasswordRequest> forgotPasswordRequestValidator,
            IValidator<ResetPasswordRequest> resetPasswordRequestValidator, IValidator<VerifyEmailRequest> verifyEmailRequestValidator,
            IValidator<ResendVerificationRequest> resendVerificationRequestValidator,IValidator<SendChangePasswordCodeRequest> sendChangePasswordCodeRequestValidator,
            IValidator<SendChangeEmailCodeRequest> sendChangeEmailCodeRequestValidator, IValidator<ChangeEmailRequest> changeEmailRequestValidator)
        {
            _authService = authService;
            _registerRequestValidator = registerRequestValidator;
            _loginRequestValidator = loginRequestValidator;
            _updateProfileValidator = updateProfileValidator;
            _changePasswordRequestValidator = changePasswordRequestValidator;
            _forgotPasswordRequestValidator = forgotPasswordRequestValidator;
            _resetPasswordRequestValidator = resetPasswordRequestValidator;
            _verifyEmailRequestValidator = verifyEmailRequestValidator;
            _resendVerificationRequestValidator = resendVerificationRequestValidator;
            _sendChangePasswordCodeRequestValidator = sendChangePasswordCodeRequestValidator;
            _sendChangeEmailCodeRequestValidator = sendChangeEmailCodeRequestValidator;
            _changeEmailRequestValidator = changeEmailRequestValidator;
        }

        [Authorize]
        [HttpPost("change-email/code")]
        public async Task<IActionResult> SendChangeEmailCode(SendChangeEmailCodeRequest request)
        {
            var validationResult = await _sendChangeEmailCodeRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            string? userId =User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is null)
            {
                return Unauthorized();
            }

            await _authService.SendChangeEmailCodeAsync(int.Parse(userId), request);

            return NoContent();
        }

        [Authorize]
        [HttpPut("change-email")]
        public async Task<ActionResult<CurrentUserResponse>> ChangeEmail(ChangeEmailRequest request)
        {
            var validationResult =await _changeEmailRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            string? userId =User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is null)
            {
                return Unauthorized();
            }

            CurrentUserResponse response =await _authService.ChangeEmailAsync(int.Parse(userId),request);

            return Ok(response);
        }


        [Authorize]
        [HttpPost("change-password/code")]
        public async Task<IActionResult> SendChangePasswordCode(SendChangePasswordCodeRequest request)
        {
            var validationResult =
                await _sendChangePasswordCodeRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            string? userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is null)
            {
                return Unauthorized();
            }

            await _authService.SendChangePasswordCodeAsync(
                int.Parse(userId),
                request);

            return NoContent();
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var validationResult =
                await _changePasswordRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            string? userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is null)
            {
                return Unauthorized();
            }

            await _authService.ChangePasswordAsync(
                int.Parse(userId),
                request);

            return NoContent();
        }



        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            var validationResult = await _forgotPasswordRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await _authService.SendForgotPasswordCodeAsync(request.Email);

            return NoContent();
        }

        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var validationResult =await _resetPasswordRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await _authService.ResetPasswordAsync(request);

            return NoContent();
        }



        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
        {
            var validationResult = await _verifyEmailRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await _authService.VerifyEmailAsync(request);

            return NoContent();
        }


        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification(ResendVerificationRequest request)
        {
            var validationResult = await _resendVerificationRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await _authService.ResendVerificationCodeAsync(request.Email);

            return NoContent();
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
        [HttpPut("me")]
        public async Task<ActionResult<CurrentUserResponse> > UpdateProfile( UpdateProfileRequest request)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is null) return Unauthorized();
            

            var validationResult = await _updateProfileValidator.ValidateAsync(request);

            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);
            

            CurrentUserResponse response = await _authService.UpdateProfileAsync(int.Parse(userId),request);

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
