using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Repository.IRepository;
using System.Net;

namespace real_time_chat_web.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly APIResponse _response;

        public UserController(IUserRepository userRepo, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _userManager = userManager;
            _response = new APIResponse();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Bearer")]

        public async Task<ActionResult<APIResponse>> GetUsers([FromQuery] string? search)
        {
            try
            {
                IEnumerable<ApplicationUser> users;

                if (!string.IsNullOrEmpty(search))
                    users = await _userRepo.GetAllAsync(x => x.UserName.Contains(search));
                else
                    users = await _userRepo.GetAllAsync();

                _response.Result = _mapper.Map<List<ApplicationUserDTO>>(users);
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Errors = new List<string> { ex.Message };
                return BadRequest(_response);
            }
        }


        [HttpGet("{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<APIResponse>> GetUserByUserName(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    return BadRequest();

                var user = await _userRepo.GetAsync(x => x.UserName == username);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Errors = new List<string>() { "User not found" };
                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<ApplicationUserDTO>(user);
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Errors = new List<string>() { ex.Message };
                return BadRequest(_response);
            }
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<APIResponse>> CreateUser([FromBody] ApplicationUserCreateDTO userDto)
        {
            try
            {
                if (userDto == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Errors = new List<string> { "User data is null" };
                    return BadRequest(_response);
                }

                var existingUser = await _userRepo.GetAsync(u => u.Email == userDto.UserName);
                if (existingUser != null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Errors = new List<string> { "User with this email already exists" };
                    return BadRequest(_response);
                }

                var user = _mapper.Map<ApplicationUser>(userDto);

                await _userRepo.CreateAsync(user);

                _response.Result = _mapper.Map<ApplicationUserDTO>(user);
                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;
                return CreatedAtAction(nameof(GetUserByUserName), new { username = user.UserName }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Errors = new List<string> { ex.Message };
                return BadRequest(_response);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<APIResponse>> UpdateUser(string id, [FromBody] ApplicationUserUpdateDTO userDto)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || userDto == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Errors = new List<string> { "Invalid data" };
                    return BadRequest(_response);
                }

                var existingUser = await _userRepo.GetAsync(u => u.Id == id);
                if (existingUser == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Errors = new List<string> { "User not found" };
                    return NotFound(_response);
                }

                _mapper.Map(userDto, existingUser);

                await _userRepo.UpdateAsync(existingUser);

                _response.Result = _mapper.Map<ApplicationUserDTO>(existingUser);
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Errors = new List<string> { ex.Message };
                return BadRequest(_response);
            }
        }
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<APIResponse>> DeleteUser(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Errors = new List<string> { "Invalid user ID" };
                    return BadRequest(_response);
                }

                var user = await _userRepo.GetAsync(u => u.Id == id);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Errors = new List<string> { "User not found" };
                    return NotFound(_response);
                }

                await _userRepo.RemoveAsync(user);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = $"User with ID {id} has been successfully deleted.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Errors = new List<string> { ex.Message };
                return BadRequest(_response);
            }
        }
        [HttpPut("change-profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<APIResponse>> ChangeProfile([FromForm] ApplicationUserProfileDTO userDto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Errors = new List<string> { "User not found" };
                    return BadRequest(_response);
                }

                if (!string.IsNullOrEmpty(userDto.Name))
                {
                    user.Name = userDto.Name;
                }
                if (!string.IsNullOrEmpty(userDto.PhoneNumber))
                {
                    user.PhoneNumber = userDto.PhoneNumber;
                }

                if (userDto.Image != null)
                {
                    string fileName = user.Id + Path.GetExtension(userDto.Image.FileName);
                    string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProfileImage");

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    string filePath = Path.Combine(directoryPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        userDto.Image.CopyTo(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    user.ImageUrl = $"{baseUrl}/ProfileImage/{fileName}";
                }

                await _userRepo.UpdateAsync(user);
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = _mapper.Map<ApplicationUserDTO>(user);
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Errors = new List<string> { ex.Message };
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
        }


    }

}
