using Application.Dtos.Auth;
using Application.Exceptions;
using Application.Interfaces;
using Application.Service;
using Application.Validators;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FakeItEasy;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;

namespace Application.Tests
{
    public class AuthServiceTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthServiceTests()
        {
            _unitOfWork = A.Fake<IUnitOfWork>();
            _userRepository = A.Fake<IUserRepository>();
            A.CallTo(() => _unitOfWork.Users).Returns(_userRepository);

            var configData = new Dictionary<string, string?>
            {
                { "Jwt:Key", "TestSecretKeyForJWTAuthenticationSecure!@#$" },
                { "Jwt:Issuer", "Harmonia" },
                { "Jwt:Audience", "Harmonia" }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        [Fact]
        public async Task RegisterNewUser_Com_Dados_Validos_Cria_Usuario()
        {
            var request = new RegisterRequest
            {
                Name = "João Silva",
                Email = "joao@test.com",
                Password = "123456",
                Role = "Administrator"
            };

            A.CallTo(() => _userRepository.GetByEmail("joao@test.com")).Returns((User?)null);
            A.CallTo(() => _userRepository.Add(A<User>._)).ReturnsLazily((User u) => u);

            var service = new AuthService(_unitOfWork, _configuration);

            var result = await service.RegisterNewUser(request);

            Assert.NotNull(result);
            Assert.Equal("João Silva", result.Name);
            Assert.Equal("joao@test.com", result.Email);
            Assert.Equal("Administrator", result.Role);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(default)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task RegisterNewUser_Com_Email_Existente_Lanca_ValidationException()
        {
            var request = new RegisterRequest
            {
                Name = "João Silva",
                Email = "existing@test.com",
                Password = "123456",
                Role = "Administrator"
            };

            var usuarioExistente = new User { Email = "existing@test.com" };
            A.CallTo(() => _userRepository.GetByEmail("existing@test.com")).Returns(usuarioExistente);

            var service = new AuthService(_unitOfWork, _configuration);

            var exception = await Assert.ThrowsAsync<ValidationException>(() => service.RegisterNewUser(request));
            Assert.Contains("Já existe", exception.Message);
        }

        [Fact]
        public async Task AuthenticateUser_Com_Credenciais_Validas_Retorna_Token()
        {
            var usuario = new User
            {
                Id = Guid.NewGuid(),
                Name = "João Silva",
                Email = "joao@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = UserRole.Administrator
            };

            A.CallTo(() => _userRepository.GetByEmail("joao@test.com")).Returns(usuario);

            var request = new LoginRequest { Email = "joao@test.com", Password = "123456" };
            var service = new AuthService(_unitOfWork, _configuration);

            var result = await service.AuthenticateUser(request);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.Equal("João Silva", result.User.Name);
            Assert.Equal("Administrator", result.User.Role);
        }

        [Fact]
        public async Task AuthenticateUser_Com_Senha_Errada_Lanca_UnauthorizedException()
        {
            var usuario = new User
            {
                Id = Guid.NewGuid(),
                Email = "joao@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
            };

            A.CallTo(() => _userRepository.GetByEmail("joao@test.com")).Returns(usuario);

            var request = new LoginRequest { Email = "joao@test.com", Password = "senhaerrada" };
            var service = new AuthService(_unitOfWork, _configuration);

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => service.AuthenticateUser(request));
            Assert.Contains("inválidos", exception.Message);
        }

        [Fact]
        public async Task AuthenticateUser_Com_Usuario_Inexistente_Lanca_UnauthorizedException()
        {
            A.CallTo(() => _userRepository.GetByEmail("naoexiste@test.com")).Returns((User?)null);

            var request = new LoginRequest { Email = "naoexiste@test.com", Password = "123456" };
            var service = new AuthService(_unitOfWork, _configuration);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.AuthenticateUser(request));
        }
    }

    public class RegisterRequestValidatorTests
    {
        private readonly RegisterRequestValidator _validator = new();

        [Fact]
        public void Valida_Nome_Obrigatorio()
        {
            var request = new RegisterRequest { Name = "", Email = "test@test.com", Password = "123456", Role = "Seller" };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Valida_Email_Formato_Invalido()
        {
            var request = new RegisterRequest { Name = "João", Email = "emailinvalido", Password = "123456", Role = "Seller" };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Valida_Senha_Minimo_6_Caracteres()
        {
            var request = new RegisterRequest { Name = "João", Email = "test@test.com", Password = "123", Role = "Seller" };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("A senha deve ter no mínimo 6 caracteres.");
        }

        [Fact]
        public void Valida_Role_Invalido()
        {
            var request = new RegisterRequest { Name = "João", Email = "test@test.com", Password = "123456", Role = "RoleInvalido" };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Role);
        }

        [Fact]
        public void Aceita_Dados_Validos()
        {
            var request = new RegisterRequest { Name = "João Silva", Email = "joao@test.com", Password = "123456", Role = "Administrator" };
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
