// HerdSmart.Tests/Features/MilkLogs/CreateMilkLogCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.MilkLogs.Commands.CreateMilkLog;
using AutoMapper;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.MilkLogs;

public class CreateMilkLogCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateMilkLogCommandHandler>> _loggerMock;
    private readonly CreateMilkLogCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();

    public CreateMilkLogCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateMilkLogCommandHandler>>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _handler = new CreateMilkLogCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object,
            _loggerMock.Object,
            _mapperMock.Object
            );
    }

    [Fact]
    public async Task Handle_WhenCattleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CreateMilkLogCommand(
            Ulid.NewUlid(), 10.5, MilkShift.Morning, null);

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
        var command = new CreateMilkLogCommand(
            cattleId, 10.5, MilkShift.Morning, null);

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
    public async Task Handle_WhenCattleIsDry_ShouldThrowBadRequestException()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new CreateMilkLogCommand(
            cattleId, 10.5, MilkShift.Morning, null);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            new() { Id = cattleId, TenantId = _tenantId, Status = CattleStatus.Dry }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenSameShiftExistsToday_ShouldThrowConflictException()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new CreateMilkLogCommand(
            cattleId, 10.5, MilkShift.Morning, null);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            new() { Id = cattleId, TenantId = _tenantId,
                    TagNumber = "COW-001", Status = CattleStatus.Active }
        }.BuildMockDbSet();

        // نفس الـ Shift في نفس اليوم
        var milkLogs = new List<MilkProductionLog>
        {
            new()
            {
                CattleId = cattleId,
                TenantId = _tenantId,
                Shift = MilkShift.Morning,
                LoggedAt = DateTimeOffset.UtcNow
            }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.MilkProductionLogs).Returns(milkLogs.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateMilkLog()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new CreateMilkLogCommand(
            cattleId, 15.5, MilkShift.Morning, null);

        var existingCattle = new HerdSmart.Domain.Entities.Cattle
        {
            Id = cattleId,
            TenantId = _tenantId,
            TagNumber = "COW-001",
            Status = CattleStatus.Active
        };

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            existingCattle
        }.BuildMockDbSet();

        var milkLogs = new List<MilkProductionLog>()
            .BuildMockDbSet();

        var newMilkLog = new MilkProductionLog
        {
            CattleId = cattleId,
            TenantId = _tenantId,
            AmountInLiters = command.AmountInLiters,
            Shift = command.Shift,
            LoggedAt = DateTimeOffset.UtcNow
        };

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.MilkProductionLogs).Returns(milkLogs.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<MilkProductionLog>(command))
            .Returns(newMilkLog);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.AmountInLiters.ShouldBe(15.5);
        result.Shift.ShouldBe(MilkShift.Morning);
        result.CattleTagNumber.ShouldBe("COW-001");
    }
}