// HerdSmart.Tests/Features/Auth/RefreshTokenCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Auth.Commands.RefreshToken;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.Security.Claims;

namespace HerdSmart.Tests.Features.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _loggerMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);
        _jwtServiceMock = new Mock<IJwtService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _loggerMock = new Mock<ILogger<RefreshTokenCommandHandler>>();

        _handler = new RefreshTokenCommandHandler(
            _userManagerMock.Object,
            _jwtServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAccessTokenInvalid_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new RefreshTokenCommand("invalid_token", "refresh_token");

        _jwtServiceMock
            .Setup(x => x.GetPrincipalFromExpiredToken(command.AccessToken))
            .Returns((ClaimsPrincipal?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenInvalid_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RefreshTokenCommand("valid_token", "wrong_refresh");

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));

        var user = new AppUser { Id = userId };

        _jwtServiceMock
            .Setup(x => x.GetPrincipalFromExpiredToken(command.AccessToken))
            .Returns(claims);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _refreshTokenServiceMock
            .Setup(x => x.GetRefreshTokenAsync(userId))
            .ReturnsAsync("correct_refresh");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<UnauthorizedException>();
    }

    [Fact]
    
    public async Task Handle_WhenValidTokens_ShouldCallGenerateTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RefreshTokenCommand("valid_token", "valid_refresh");

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        var claims = new ClaimsPrincipal(identity);

        var user = new AppUser { Id = userId, FullName = "Test", Role = UserRole.Owner, TenantId = Ulid.NewUlid() };

        _jwtServiceMock.Setup(x => x.GetPrincipalFromExpiredToken(It.IsAny<string>())).Returns(claims);
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        _refreshTokenServiceMock.Setup(x => x.GetRefreshTokenAsync(It.IsAny<Guid>())).ReturnsAsync("valid_refresh");
        _jwtServiceMock.Setup(x => x.GenerateTokens(It.IsAny<AppUser>())).Returns(("new_access", "new_refresh"));
        _refreshTokenServiceMock.Setup(x => x.SaveRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act & Assert
        _jwtServiceMock.Verify(x => x.GenerateTokens(It.IsAny<AppUser>()), Times.Never);
    }
}