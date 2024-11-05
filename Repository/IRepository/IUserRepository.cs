using real_time_chat_web.Models.DTO;

namespace real_time_chat_web.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string username);
        Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO);
        Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO);
        Task<ResponseTokenPasswordDTO> ForgotPassword(RequestForgotPasswordDTO request);
    }
}
