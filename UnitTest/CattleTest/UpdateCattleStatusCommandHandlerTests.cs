// HerdSmart.Tests/Features/Cattle/UpdateCattleStatusCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Cattle.Dtos;
using AutoMapper;
using HerdSmart.Domain.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Cattle;

public class UpdateCattleStatusCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UpdateCattleStatusCommandHandler>> _loggerMock;
    private readonly UpdateCattleStatusCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();

    public UpdateCattleStatusCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UpdateCattleStatusCommandHandler>>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _handler = new UpdateCattleStatusCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCattleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateCattleStatusCommand(Ulid.NewUlid(), CattleStatus.Sick);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCattleIsDead_ShouldThrowBadRequestException()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new UpdateCattleStatusCommand(cattleId, CattleStatus.Active);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            new() { Id = cattleId, TenantId = _tenantId, Status = CattleStatus.Dead }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenCattleIsSickAndStatusIsSold_ShouldThrowBadRequestException()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new UpdateCattleStatusCommand(cattleId, CattleStatus.Sold);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            new() { Id = cattleId, TenantId = _tenantId, Status = CattleStatus.Sick }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenValidStatusChange_ShouldUpdateStatus()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new UpdateCattleStatusCommand(cattleId, CattleStatus.Sick);

        var existingCattle = new HerdSmart.Domain.Entities.Cattle
        {
            Id = cattleId,
            TenantId = _tenantId,
            Status = CattleStatus.Active
        };

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            existingCattle
        }.BuildMockDbSet();

        var response = new CattleResponse(
            cattleId, "COW-001", "Holstein",
            Gender.Female, DateTimeOffset.UtcNow.AddYears(-2),
            CattleStatus.Sick, null, null, DateTimeOffset.UtcNow);

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<CattleResponse>(existingCattle))
            .Returns(response);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        existingCattle.Status.ShouldBe(CattleStatus.Sick);
    }
}