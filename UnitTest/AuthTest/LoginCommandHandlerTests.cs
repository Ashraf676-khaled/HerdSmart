// HerdSmart.Tests/Features/Auth/LoginCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Login;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);
        _jwtServiceMock = new Mock<IJwtService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _loggerMock = new Mock<ILogger<LoginCommandHandler>>();

        _handler = new LoginCommandHandler(
            _userManagerMock.Object,
            _refreshTokenServiceMock.Object,
            _loggerMock.Object,
            _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEmailNotFound_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("wrong@test.com", "Password@123");

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((AppUser?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenPasswordInvalid_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("test@test.com", "WrongPassword");
        var user = new AppUser { Email = command.Email };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var command = new LoginCommand("test@test.com", "Password@123");
        var user = new AppUser
        {
            Email = command.Email,
            FullName = "Test User",
            Role = UserRole.Owner,
            TenantId = Ulid.NewUlid()
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);

        _jwtServiceMock
            .Setup(x => x.GenerateTokens(user))
            .Returns(("access_token", "refresh_token"));

        _refreshTokenServiceMock
            .Setup(x => x.SaveRefreshTokenAsync(user.Id, "refresh_token"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.AccessToken.ShouldBe("access_token");
        result.RefreshToken.ShouldBe("refresh_token");
        result.Role.ShouldBe("Owner");
    }
}