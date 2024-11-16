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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly APIResponse _response;

        public UserController(IUserRepository userRepo, IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
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
                        Role = string.Join(",", roles)
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

                // Kiểm tra xem người dùng có tồn tại dựa trên email/UserName không
                var existingUser = await _userRepo.GetAsync(u => u.Email == userDto.UserName);
                if (existingUser != null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Errors = new List<string> { "User with this email already exists" };
                    return BadRequest(_response);
                }

                var user = _mapper.Map<ApplicationUser>(userDto);

                // Thêm người dùng mới vào cơ sở dữ liệu
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Errors = createResult.Errors.Select(e => e.Description).ToList();
                    return BadRequest(_response);
                }

                // Kiểm tra và thêm role nếu cần thiết
                if (!string.IsNullOrEmpty(userDto.Role))
                {
                    var roleExists = await _roleManager.RoleExistsAsync(userDto.Role);
                    if (!roleExists)
                    {
                        // Nếu role chưa tồn tại, tạo mới
                        var roleCreateResult = await _roleManager.CreateAsync(new IdentityRole(userDto.Role));
                        if (!roleCreateResult.Succeeded)
                        {
                            _response.IsSuccess = false;
                            _response.StatusCode = HttpStatusCode.BadRequest;
                            _response.Errors = roleCreateResult.Errors.Select(e => e.Description).ToList();
                            return BadRequest(_response);
                        }
                    }

                    // Gán role cho người dùng
                    var roleAssignResult = await _userManager.AddToRoleAsync(user, userDto.Role);
                    if (!roleAssignResult.Succeeded)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.Errors = roleAssignResult.Errors.Select(e => e.Description).ToList();
                        return BadRequest(_response);
                    }
                }

                // Lấy role hiện tại của người dùng để đưa vào DTO
                var userRoles = await _userManager.GetRolesAsync(user);
                var userDTO = _mapper.Map<ApplicationUserDTO>(user);
                userDTO.Role = userRoles.FirstOrDefault();

                // Trả về phản hồi
                _response.Result = userDTO;
                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;

                return CreatedAtAction(nameof(GetUserByUserName), new { username = user.UserName }, _response);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung
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

                // Kiểm tra xem người dùng có tồn tại hay không
                var existingUser = await _userRepo.GetAsync(u => u.Id == id);
                if (existingUser == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Errors = new List<string> { "User not found" };
                    return NotFound(_response);
                }

                // Cập nhật thông tin cơ bản
                _mapper.Map(userDto, existingUser);
                var updateResult = await _userManager.UpdateAsync(existingUser);
                if (!updateResult.Succeeded)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Errors = updateResult.Errors.Select(e => e.Description).ToList();
                    return BadRequest(_response);
                }

                // Kiểm tra và xử lý vai trò mới (nếu có)
                if (!string.IsNullOrEmpty(userDto.Role))
                {
                    // Kiểm tra role có tồn tại không, nếu không thì tạo mới
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

                    // Lấy danh sách role hiện tại và cập nhật nếu cần thiết
                    var currentRoles = await _userManager.GetRolesAsync(existingUser);
                    if (!currentRoles.Contains(userDto.Role))
                    {
                        // Loại bỏ tất cả vai trò hiện tại
                        var removeRolesResult = await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                        if (!removeRolesResult.Succeeded)
                        {
                            _response.IsSuccess = false;
                            _response.StatusCode = HttpStatusCode.BadRequest;
                            _response.Errors = removeRolesResult.Errors.Select(e => e.Description).ToList();
                            return BadRequest(_response);
                        }

                        // Gán vai trò mới
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

                // Lấy lại thông tin vai trò cập nhật
                var userRoles = await _userManager.GetRolesAsync(existingUser);

                // Chuẩn bị dữ liệu để trả về
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
