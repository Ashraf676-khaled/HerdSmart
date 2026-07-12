// HerdSmart.Tests/Features/Auth/RegisterCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Register;
using AutoMapper;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MockQueryable.Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _userManagerMock = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);
        _jwtServiceMock = new Mock<IJwtService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<RegisterCommandHandler>>();

        _handler = new RegisterCommandHandler(
    _contextMock.Object,
    _loggerMock.Object,              
    _jwtServiceMock.Object,         
    _userManagerMock.Object,         
    _refreshTokenServiceMock.Object,
    _mapperMock.Object);            
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = new RegisterCommand(
            "Test Farm",
            "Test Owner",
            "test@test.com",
            "Test@1234");

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(new AppUser());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenFarmNameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = new RegisterCommand(
            "Existing Farm",
            "Test Owner",
            "test@test.com",
            "Test@1234");

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((AppUser?)null);

        var tenants = new List<Tenant>
        {
            new() { Name = "Existing Farm" }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.Tenants).Returns(tenants.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnAuthResponse()
    {
        // Arrange
        var command = new RegisterCommand(
            "New Farm",
            "New Owner",
            "new@test.com",
            "Test@1234");

        var tenant = new Tenant { Name = command.FarmName };
        var owner = new AppUser
        {
            Email = command.Email,
            FullName = command.OwnerName,
            Role = UserRole.Owner
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((AppUser?)null);

        var tenants = new List<Tenant>().BuildMockDbSet();
        _contextMock.Setup(x => x.Tenants).Returns(tenants.Object);

        _mapperMock.Setup(x => x.Map<Tenant>(command)).Returns(tenant);
        _mapperMock.Setup(x => x.Map<AppUser>(command)).Returns(owner);

        _userManagerMock
            .Setup(x => x.CreateAsync(owner, command.Password))
            .ReturnsAsync(IdentityResult.Success);

        _jwtServiceMock
            .Setup(x => x.GenerateTokens(owner))
            .Returns(("access_token", "refresh_token"));

        _refreshTokenServiceMock
            .Setup(x => x.SaveRefreshTokenAsync(owner.Id, "refresh_token"))
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