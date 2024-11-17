using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Repository.IRepository;
using real_time_chat_web.Services;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace real_time_chat_web.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly string secretKey;
        private readonly IEmailService _emailService;

        public AuthRepository(ApplicationDbContext db, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager, IEmailService emailService)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _mapper = mapper;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        }
        public bool IsUniqueUser(string username)
        {
            if (_db.ApplicationUsers.FirstOrDefault(x => x.UserName == username) != null)
                return false;
            return true;
        }

        public async Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = await _userManager.FindByNameAsync(loginRequestDTO.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password))
            {
                return new TokenDTO
                {
                    AccessToken = "",
                    Message = "Username or password is incorrect."
                };
            }

            // Check if the email is confirmed
            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!isEmailConfirmed)
            {
                return new TokenDTO
                {
                    AccessToken = "",
                    Message = "Please verify your email before logging in."
                };
            }

            // Generate JWT token
            var jwtTokenId = $"JTI{Guid.NewGuid()}";
            var accessToken = await GetAccessToken(user, jwtTokenId);
            var refreshToken = await CreateNewRefreshToken(user.Id, jwtTokenId);

            return new TokenDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            // Kiểm tra tính hợp lệ của email
            if (!new EmailAddressAttribute().IsValid(registerationRequestDTO.UserName))
            {
                throw new Exception("The specified string is not in the form required for an e-mail address.");
            }

            ApplicationUser user = new()
            {
                UserName = registerationRequestDTO.UserName,
                Name = registerationRequestDTO.Name,
                Email = registerationRequestDTO.UserName,
                NormalizedEmail = registerationRequestDTO.UserName.ToUpper()
            };

            try
            {
                // Tạo người dùng và kiểm tra kết quả
                var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("user"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("user"));
                    }
                    await _userManager.AddToRoleAsync(user, "user");

                    // Xác nhận email
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _emailService.SendEmail(user.Email, "Email Confirmation", code);

                    return new UserDTO
                    {
                        UserName = user.UserName,
                        Email = user.Email,
                        ConfirmationMessage = "Please confirm your email with the code that you received!"
                    };
                }
                else
                {
                    // Ghi nhận và ném các lỗi mật khẩu hoặc bất kỳ lỗi nào khác
                    string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception(errors);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> GetAccessToken(ApplicationUser user, string jwtTokenId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var roles = await _userManager.GetRolesAsync(user);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Jti, jwtTokenId),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public async Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO)
        {
            // Find an existing refresh token
            var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Refresh_Token == tokenDTO.RefreshToken);
            if (existingRefreshToken == null)
            {
                existingRefreshToken.IsValid = false;
                return new TokenDTO();
            }

            // Compare data from existing refresh and access token provided and if there is any missmatch then consider it as a fraud
            var result = GetAccessTokenData(tokenDTO.AccessToken);
            if (!result.isSuccess || result.jwtTokenId != existingRefreshToken.JwtTokenId ||
                result.userId != existingRefreshToken.UserId)
            {
                existingRefreshToken.IsValid = false;
                _db.SaveChanges();
                return new TokenDTO();
            }
            // When someone tries to use not valid refresh token, fraud possible
            if (!existingRefreshToken.IsValid)
            {
                await _db.RefreshTokens.Where(x => x.UserId == existingRefreshToken.UserId &&
                x.JwtTokenId == existingRefreshToken.JwtTokenId).ExecuteUpdateAsync(x => x.SetProperty(u => u.IsValid, false));

                return new TokenDTO();
            }
            // If just expired then mark as invalid and return empty
            if (existingRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                existingRefreshToken.IsValid = false;
                _db.SaveChanges();
                return new TokenDTO();
            }
            // replace old refresh with a new one with updated expire date
            var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            // revoke existing refresh token
            existingRefreshToken.IsValid = false;
            _db.SaveChanges();
            // generate new access token
            var applicationUser = await _db.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == existingRefreshToken.UserId);
            if (applicationUser == null)
            {
                return new TokenDTO();
            }
            var newAccessToken = await GetAccessToken(applicationUser, existingRefreshToken.JwtTokenId);
            return new TokenDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };

        }
        private async Task<string> CreateNewRefreshToken(string userId, string jwtTokenId)
        {
            RefreshToken refreshToken = new()
            {
                JwtTokenId = jwtTokenId,
                UserId = userId,
                IsValid = true,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                Refresh_Token = Guid.NewGuid() + "-" + Guid.NewGuid(),
            };
            await _db.RefreshTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();
            return refreshToken.Refresh_Token;
        }

        private (bool isSuccess, string userId, string jwtTokenId) GetAccessTokenData(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);

                var jwtTokenId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti).Value;
                var userId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;

                return (true, userId, jwtTokenId);
            }
            catch
            {
                return (false, null, null);
            }
        }

        public async Task<ResponseTokenPasswordDTO> ForgotPassword(RequestForgotPasswordDTO request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return new ResponseTokenPasswordDTO();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (string.IsNullOrEmpty(token))
                return new ResponseTokenPasswordDTO();
            //send email
            await _emailService.SendEmail(user.Email, "Code to reset your password", token);

            return new ResponseTokenPasswordDTO()
            {
                Email = user.Email,
                Token = token
            };
        }
    }
}
