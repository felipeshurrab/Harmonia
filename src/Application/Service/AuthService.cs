using Application.Dtos.Auth;
using Application.Exceptions;
using Application.Interfaces;
using Application.Service.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<UserResponse> RegisterNewUser(RegisterRequest request)
        {
            var existingUser = await _unitOfWork.Users.GetByEmail(request.Email.ToLower());
            if (existingUser != null)
                throw new ValidationException("Já existe um usuário cadastrado com este e-mail.");

            var userRole = Enum.Parse<UserRole>(request.Role, ignoreCase: true);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = userRole,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.Add(newUser);
            await _unitOfWork.SaveChangesAsync();

            return MapToUserResponse(newUser);
        }

        public async Task<LoginResponse> AuthenticateUser(LoginRequest request)
        {
            var user = await _unitOfWork.Users.GetByEmail(request.Email.ToLower());
            
            bool credentialsAreInvalid = user == null || 
                !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            
            if (credentialsAreInvalid)
                throw new UnauthorizedException("E-mail ou senha inválidos.");

            var token = GenerateJwtToken(user!);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = MapToUserResponse(user!)
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"] 
                ?? throw new InvalidOperationException("JWT Key não configurada");
            
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: tokenClaims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }
    }
}
