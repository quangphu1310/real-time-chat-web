using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Repository;
using real_time_chat_web.Repository.IRepository;
using System.Net;

namespace real_time_chat_web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private APIResponse _apiResponse; 
        public UserController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
        {
            _userRepo = userRepo;
            _userManager = userManager;
            _apiResponse = new APIResponse();
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var tokenDTO = await _userRepo.Login(loginRequestDTO);
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
            bool isUnique = _userRepo.IsUniqueUser(requestDTO.UserName);
            if (!isUnique)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.Errors.Add("User already exists");
                return BadRequest(_apiResponse);
            }
            var user = await _userRepo.Register(requestDTO);
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
        [HttpPost("refresh")]
        public async Task<IActionResult> GetNewTokenFromRefreshToken(TokenDTO tokenDTO)
        {
            if (ModelState.IsValid)
            {
                var tokenDTOResponse = await _userRepo.RefreshAccessToken(tokenDTO);
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
        [HttpPost("EmailVerification")]
        public async Task<IActionResult> EmailVerification(string? email, string? code)
        {
            if (email == null || code == null)
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
            var isVerified = await _userManager.ConfirmEmailAsync(user, code);
            if (isVerified.Succeeded)
            {
                return Ok("Email Verified Successfully");
            }
            _apiResponse.IsSuccess = false;
            _apiResponse.StatusCode = HttpStatusCode.BadRequest;
            _apiResponse.Errors.Add("Something went wrong!");
            return BadRequest(_apiResponse);

        }
    }
}
