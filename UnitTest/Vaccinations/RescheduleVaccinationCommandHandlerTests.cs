// HerdSmart.Tests/Features/Vaccinations/RescheduleVaccinationCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Vaccinations.Commands.RescheduleVaccination;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Vaccinations;

public class RescheduleVaccinationCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly RescheduleVaccinationCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();

    public RescheduleVaccinationCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _handler = new RescheduleVaccinationCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WhenScheduleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new RescheduleVaccinationCommand(Ulid.NewUlid(), DateTimeOffset.UtcNow);

        var schedules = new List<VaccinationSchedule>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCompleted_ShouldThrowBadRequestException()
    {
        // Arrange
        var scheduleId = Ulid.NewUlid();
        var command = new RescheduleVaccinationCommand(scheduleId, DateTimeOffset.UtcNow.AddDays(3));

        var schedules = new List<VaccinationSchedule>
        {
            new()
            {
                Id = scheduleId,
                TenantId = _tenantId,
                Status = VaccinationStatus.Completed,
                Cattle = new Domain.Entities.Cattle { TagNumber = "COW-001" },
                Vaccine = new Vaccine { Name = "FMD" }
            }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenOverdueRescheduledToFuture_ShouldRevertToPending()
    {
        // Arrange
        var scheduleId = Ulid.NewUlid();
        var newDate = DateTimeOffset.UtcNow.AddDays(5);
        var command = new RescheduleVaccinationCommand(scheduleId, newDate);

        var schedule = new VaccinationSchedule
        {
            Id = scheduleId,
            TenantId = _tenantId,
            Status = VaccinationStatus.Overdue,
            ScheduledDate = DateTimeOffset.UtcNow.AddDays(-3),
            Cattle = new Domain.Entities.Cattle { TagNumber = "COW-001" },
            Vaccine = new Vaccine { Name = "FMD" }
        };

        var schedules = new List<VaccinationSchedule> { schedule }
            .BuildMockDbSet();

        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.ShouldBe(VaccinationStatus.Pending);
        result.ScheduledDate.ShouldBe(newDate);
    }

    [Fact]
    public async Task Handle_WhenPendingRescheduled_ShouldKeepPendingStatus()
    {
        // Arrange
        var scheduleId = Ulid.NewUlid();
        var newDate = DateTimeOffset.UtcNow.AddDays(10);
        var command = new RescheduleVaccinationCommand(scheduleId, newDate);

        var schedule = new VaccinationSchedule
        {
            Id = scheduleId,
            TenantId = _tenantId,
            Status = VaccinationStatus.Pending,
            ScheduledDate = DateTimeOffset.UtcNow.AddDays(3),
            Cattle = new Domain.Entities.Cattle { TagNumber = "COW-001" },
            Vaccine = new Vaccine { Name = "FMD" }
        };

        var schedules = new List<VaccinationSchedule> { schedule }
            .BuildMockDbSet();

        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.ShouldBe(VaccinationStatus.Pending);
        result.ScheduledDate.ShouldBe(newDate);
    }
}