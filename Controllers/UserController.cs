using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using real_time_chat_web.Data;
using real_time_chat_web.Migrations;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Repository;
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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly APIResponse _response;
        private readonly ApplicationDbContext _db;
        private readonly IRoomsUserRepository _roomsUserRepo;

        public UserController(IUserRepository userRepo, IRoomsUserRepository roomsUserRepo, ApplicationDbContext db,IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _response = new APIResponse();
            _db = db;
            _roomsUserRepo = roomsUserRepo;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<APIResponse>> GetUsers([FromQuery] string? search)
        {
            try
            {
                var users = string.IsNullOrEmpty(search)
                    ? await _userManager.Users.ToListAsync()
                    : await _userManager.Users.Where(c => c.UserName.Contains(search)).ToListAsync();

                var userDTOs = new List<ApplicationUserDTO>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDTOs.Add(new ApplicationUserDTO
                    {
                        Id = user.Id,
                        Name = user.Name,
                        UserName = user.UserName,
                        EmailConfirmed = user.EmailConfirmed.ToString(),
                        PhoneNumber = user.PhoneNumber,
                        Role = string.Join(",", roles),
                        ImageUrl = user.ImageUrl
                    });
                }

                _response.Result = userDTOs;
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
        [Authorize( AuthenticationSchemes = "Bearer")]
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
                var userDto = _mapper.Map<ApplicationUserDTO>(user);
                userDto.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                _response.Result = userDto;
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

                if (!string.IsNullOrEmpty(userDto.Role))
                {
                    var roleExists = await _roleManager.RoleExistsAsync(userDto.Role);
                    if (!roleExists)
                    {
                        var roleCreateResult = await _roleManager.CreateAsync(new IdentityRole(userDto.Role));
                        if (!roleCreateResult.Succeeded)
                        {
                            _response.IsSuccess = false;
                            _response.StatusCode = HttpStatusCode.BadRequest;
                            _response.Errors = roleCreateResult.Errors.Select(e => e.Description).ToList();
                            return BadRequest(_response);
                        }
                    }

                    var roleAssignResult = await _userManager.AddToRoleAsync(user, userDto.Role);
                    if (!roleAssignResult.Succeeded)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.Errors = roleAssignResult.Errors.Select(e => e.Description).ToList();
                        return BadRequest(_response);
                    }
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userDTO = _mapper.Map<ApplicationUserDTO>(user);
                userDTO.Role = userRoles.FirstOrDefault();

                _response.Result = userDTO;
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


                if (!string.IsNullOrEmpty(userDto.Role))
                {
                    var roleExists = await _roleManager.RoleExistsAsync(userDto.Role);
                    if (!roleExists)
                    {
                        var roleCreateResult = await _roleManager.CreateAsync(new IdentityRole(userDto.Role));
                        if (!roleCreateResult.Succeeded)
                        {
                            _response.IsSuccess = false;
                            _response.StatusCode = HttpStatusCode.BadRequest;
                            _response.Errors = roleCreateResult.Errors.Select(e => e.Description).ToList();
                            return BadRequest(_response);
                        }
                    }

                    var currentRoles = await _userManager.GetRolesAsync(existingUser);
                    if (!currentRoles.Contains(userDto.Role))
                    {
                        var removeRolesResult = await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                        if (!removeRolesResult.Succeeded)
                        {
                            _response.IsSuccess = false;
                            _response.StatusCode = HttpStatusCode.BadRequest;
                            _response.Errors = removeRolesResult.Errors.Select(e => e.Description).ToList();
                            return BadRequest(_response);
                        }

                        var addRoleResult = await _userManager.AddToRoleAsync(existingUser, userDto.Role);
                        if (!addRoleResult.Succeeded)
                        {
                            _response.IsSuccess = false;
                            _response.StatusCode = HttpStatusCode.BadRequest;
                            _response.Errors = addRoleResult.Errors.Select(e => e.Description).ToList();
                            return BadRequest(_response);
                        }
                    }
                }

                var userRoles = await _userManager.GetRolesAsync(existingUser);

                var userDTO = _mapper.Map<ApplicationUserDTO>(existingUser);
                userDTO.Role = userRoles.FirstOrDefault();

                _response.Result = userDTO;
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
                
                var listMessages = _db.Messages.Where(x => x.UserId == user.Id).ToList();
                _db.RemoveRange(listMessages);

                var listRooms = _db.rooms.Where(x => x.CreatedBy == user.Id).ToList();
                _db.RemoveRange(listRooms);

                var listUserRooms = _db.RoomsUser.Where(x => x.IdUser == user.Id).ToList();
                _db.RemoveRange(listUserRooms);


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
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(userDto.Image.FileName);
                    string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProfileImage");

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    string filePath = Path.Combine(directoryPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await userDto.Image.CopyToAsync(fileStream);
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
