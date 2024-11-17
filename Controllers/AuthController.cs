using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Repository;
using real_time_chat_web.Repository.IRepository;
using real_time_chat_web.Services;
using System.Net;

namespace real_time_chat_web.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private APIResponse _apiResponse; 
        public AuthController(IAuthRepository authRepo, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _authRepo = authRepo;
            _userManager = userManager;
            _emailService = emailService;
            _apiResponse = new APIResponse();
        }
        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var tokenDTO = await _authRepo.Login(loginRequestDTO);
            if (string.IsNullOrEmpty(tokenDTO.AccessToken))
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add(tokenDTO.Message);
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_apiResponse);
            }

            _apiResponse.IsSuccess = true;
            _apiResponse.Result = tokenDTO;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterationRequestDTO requestDTO)
        {
            bool isUnique = _authRepo.IsUniqueUser(requestDTO.UserName);
            if (!isUnique)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.Errors.Add("User already exists");
                return BadRequest(_apiResponse);
            }
            try
            {
                var user = await _authRepo.Register(requestDTO);
                if (user == null)
                {
                    _apiResponse.IsSuccess = false;
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.Errors.Add("Error while registeration!");
                    return BadRequest(_apiResponse);
                }
                _apiResponse.IsSuccess = true;
                _apiResponse.Result = user;
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.Errors.Add(ex.Message);
                return BadRequest(_apiResponse);
            }
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> GetNewTokenFromRefreshToken(TokenDTO tokenDTO)
        {
            if (ModelState.IsValid)
            {
                var tokenDTOResponse = await _authRepo.RefreshAccessToken(tokenDTO);
                if (tokenDTOResponse == null || string.IsNullOrEmpty(tokenDTOResponse.AccessToken))
                {
                    _apiResponse.IsSuccess = false;
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.Result = "Invalid Input";
                    return BadRequest(_apiResponse);
                }
                _apiResponse.IsSuccess = true;
                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.Result = tokenDTOResponse;
                return Ok(_apiResponse);
            }
            _apiResponse.IsSuccess = false;
            _apiResponse.StatusCode = HttpStatusCode.BadRequest;
            _apiResponse.Result = "Invalid Input";
            return BadRequest(_apiResponse);
        }
        [HttpPost("email-verification")]
        public async Task<IActionResult> EmailVerification(RequestEmailVerification request)
        {
            if (request.Email == null || request.Code == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.Errors.Add("Invalid Input");
                return BadRequest(_apiResponse);
            }
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.Errors.Add("Invalid Input");
                return BadRequest(_apiResponse);
            }
            var isVerified = await _userManager.ConfirmEmailAsync(user, request.Code);
            if (isVerified.Succeeded)
            {
                _apiResponse.IsSuccess = true;
                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.Result = "Email Verified Successfully";
                return Ok(_apiResponse);
            }
            _apiResponse.IsSuccess = false;
            _apiResponse.StatusCode = HttpStatusCode.BadRequest;
            _apiResponse.Errors.Add("Something went wrong!");
            return BadRequest(_apiResponse);

        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(RequestForgotPasswordDTO request)
        {
            if (ModelState.IsValid) {
                var tokenResponse = await _authRepo.ForgotPassword(request);
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    _apiResponse.IsSuccess = false;
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.Result = "Invalid Input";
                    return BadRequest(_apiResponse);
                }
                _apiResponse.IsSuccess = true;
                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.Result = "Please reset password with the code that you received";
                return Ok(_apiResponse);
            }
            _apiResponse.IsSuccess = false;
            _apiResponse.StatusCode = HttpStatusCode.BadRequest;
            _apiResponse.Errors.Add("Something went wrong!");
            return BadRequest(_apiResponse);
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(RequestResetPasswordDTO request)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _apiResponse.IsSuccess = false;
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.Result = "Invalid Input";
                    return BadRequest(_apiResponse);
                }
                var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
                if (result.Succeeded)
                {
                    _apiResponse.IsSuccess = true;
                    _apiResponse.StatusCode=HttpStatusCode.OK;
                    _apiResponse.Result = "Password reset is successfully";
                    return Ok(_apiResponse);
                }
            }
            _apiResponse.IsSuccess = false;
            _apiResponse.StatusCode = HttpStatusCode.BadRequest;
            _apiResponse.Errors.Add("Something went wrong!");
            return BadRequest(_apiResponse);
        }
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO request)
        {
            if (!ModelState.IsValid)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.Errors.Add("Invalid input.");
                return BadRequest(_apiResponse);
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.Errors.Add("New password and confirm password do not match.");
                return BadRequest(_apiResponse);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.Errors.Add("User not found.");
                return BadRequest(_apiResponse);
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                foreach (var error in result.Errors)
                {
                    _apiResponse.Errors.Add(error.Description);
                }
                return BadRequest(_apiResponse);
            }

            _apiResponse.IsSuccess = true;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            _apiResponse.Result = "Password changed successfully.";
            return Ok(_apiResponse);
        }
        [HttpPost("resend-email-verification")]
        public async Task<IActionResult> ResendEmailVerification(string email)
        {
            if (email == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.Errors.Add("Invalid Input");
                return BadRequest(_apiResponse);
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.Errors.Add("Invalid Input");
                return BadRequest(_apiResponse);
            }
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailService.SendEmail(user.Email, "Email Confirmation", code);

            _apiResponse.IsSuccess = true;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            _apiResponse.Result = "Verification email sent successfully";
            return Ok(_apiResponse);
        }
    }
}
