// HerdSmart.Tests/Features/Users/CreateUserCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.User.Commands.CreateUser;
using AutoMapper;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Users;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateUserCommandHandler>> _loggerMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);
        _jwtServiceMock = new Mock<IJwtService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateUserCommandHandler>>();

        _handler = new CreateUserCommandHandler(
            _userManagerMock.Object,
            _jwtServiceMock.Object,
            _mapperMock.Object,
            _tenantProviderMock.Object,
            _refreshTokenServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = new CreateUserCommand("Test Vet", "vet@test.com", "Pass@123", UserRole.Vet);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(new AppUser());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnAuthResponse()
    {
        // Arrange
        var tenantId = Ulid.NewUlid();
        var command = new CreateUserCommand("Test Vet", "vet@test.com", "Pass@123", UserRole.Vet);
        var user = new AppUser
        {
            Email = command.Email,
            FullName = command.FullName,
            Role = UserRole.Vet
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((AppUser?)null);

        _tenantProviderMock
            .Setup(x => x.GetTenantId())
            .Returns(tenantId);

        _mapperMock
            .Setup(x => x.Map<AppUser>(command))
            .Returns(user);

        _userManagerMock
            .Setup(x => x.CreateAsync(user, command.Password))
            .ReturnsAsync(IdentityResult.Success);

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
        result.Role.ShouldBe("Vet");
        result.TenantId.ShouldBe(tenantId);
    }
}