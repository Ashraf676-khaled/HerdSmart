// HerdSmart.Tests/Features/MilkLogs/UpdateMilkLogCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.MilkLogs.Commands.UpdateMilkLog;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.MilkLogs;

public class UpdateMilkLogCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<ILogger<UpdateMilkLogCommandHandler>> _loggerMock;
    private readonly UpdateMilkLogCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateMilkLogCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _loggerMock = new Mock<ILogger<UpdateMilkLogCommandHandler>>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);
        _tenantProviderMock.Setup(x => x.GetUserId()).Returns(_userId);
        _tenantProviderMock.Setup(x => x.GetUserRole()).Returns("Worker");

        _handler = new UpdateMilkLogCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenMilkLogNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateMilkLogCommand(Ulid.NewUlid(), 10.5, MilkShift.Morning);

        var milkLogs = new List<MilkProductionLog>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.MilkProductionLogs).Returns(milkLogs.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLogOlderThan24Hours_ShouldThrowBadRequestException()
    {
        // Arrange
        var logId = Ulid.NewUlid();
        var command = new UpdateMilkLogCommand(logId, 10.5, MilkShift.Morning);

        var milkLogs = new List<MilkProductionLog>
        {
            new()
            {
                Id = logId,
                TenantId = _tenantId,
                LoggedAt = DateTimeOffset.UtcNow.AddHours(-25),
                CreatedBy = _userId,
                Cattle = new HerdSmart.Domain.Entities.Cattle { TagNumber = "COW-001" }
            }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.MilkProductionLogs).Returns(milkLogs.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenWorkerUpdatesAnotherWorkerLog_ShouldThrowForbiddenException()
    {
        // Arrange
        var logId = Ulid.NewUlid();
        var command = new UpdateMilkLogCommand(logId, 10.5, MilkShift.Morning);

        var milkLogs = new List<MilkProductionLog>
        {
            new()
            {
                Id = logId,
                TenantId = _tenantId,
                LoggedAt = DateTimeOffset.UtcNow.AddHours(-1),
                CreatedBy = Guid.NewGuid(), // ← Worker تاني
                Cattle = new HerdSmart.Domain.Entities.Cattle { TagNumber = "COW-001" }
            }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.MilkProductionLogs).Returns(milkLogs.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenShiftAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var logId = Ulid.NewUlid();
        var command = new UpdateMilkLogCommand(logId, 10.5, MilkShift.Afternoon);

        var existingLog = new MilkProductionLog
        {
            Id = logId,
            TenantId = _tenantId,
            CattleId = Ulid.NewUlid(),
            Shift = MilkShift.Morning,
            LoggedAt = DateTimeOffset.UtcNow.AddHours(-1),
            CreatedBy = _userId,
            Cattle = new HerdSmart.Domain.Entities.Cattle { TagNumber = "COW-001" }
        };

        var milkLogs = new List<MilkProductionLog>
        {
            existingLog,
            new()
            {
                Id = Ulid.NewUlid(),
                CattleId = existingLog.CattleId,
                TenantId = _tenantId,
                Shift = MilkShift.Afternoon, // ← نفس الـ Shift الجديد
                LoggedAt = DateTimeOffset.UtcNow
            }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.MilkProductionLogs).Returns(milkLogs.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldUpdateMilkLog()
    {
        // Arrange
        var logId = Ulid.NewUlid();
        var command = new UpdateMilkLogCommand(logId, 20.0, MilkShift.Morning);

        var existingLog = new MilkProductionLog
        {
            Id = logId,
            TenantId = _tenantId,
            AmountInLiters = 10.0,
            Shift = MilkShift.Morning,
            LoggedAt = DateTimeOffset.UtcNow.AddHours(-1),
            CreatedBy = _userId,
            Cattle = new HerdSmart.Domain.Entities.Cattle { TagNumber = "COW-001" }
        };

        var milkLogs = new List<MilkProductionLog>
        {
            existingLog
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.MilkProductionLogs).Returns(milkLogs.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        existingLog.AmountInLiters.ShouldBe(20.0);
        existingLog.UpdatedAt.ShouldNotBeNull();
    }
}