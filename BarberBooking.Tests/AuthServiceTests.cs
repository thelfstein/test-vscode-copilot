using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using BarberBooking.Core.DTOs;
using BarberBooking.Core.Models;
using BarberBooking.Data.Services;

namespace BarberBooking.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>().Object;
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore,
                null, null, null, null, null, null, null, null);
            
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(x => x["Jwt:Secret"]).Returns("supersecretkeysupersecretkey123456789!@#$%^&*()");
            _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("BarberBooking");
            _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("BarberBookingClient");
            
            _authService = new AuthService(_userManagerMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-id",
                Email = "test@example.com",
                FullName = "Test User",
                UserType = "Client"
            };

            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password@123"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);
            
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("Test User", result.FullName);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
        {
            // Arrange
            var user = new ApplicationUser { Email = "test@example.com" };
            var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);
            
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_CreatesUser()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "Password@123",
                FullName = "New User",
                PhoneNumber = "(11) 99999-9999",
                UserType = "Client"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(null as ApplicationUser);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.Equal("newuser@example.com", result.Email);
            Assert.Equal("Client", result.UserType);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ReturnsNull()
        {
            // Arrange
            var existingUser = new ApplicationUser { Email = "existing@example.com" };
            var request = new RegisterRequest { Email = "existing@example.com", Password = "Password@123" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.Null(result);
        }
    }
}
