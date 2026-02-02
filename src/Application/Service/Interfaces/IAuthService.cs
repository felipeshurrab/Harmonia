using Application.Dtos.Auth;

namespace Application.Service.Interfaces
{
    public interface IAuthService
    {
        Task<UserResponse> RegisterNewUser(RegisterRequest request);
        Task<LoginResponse> AuthenticateUser(LoginRequest request);
    }
}
