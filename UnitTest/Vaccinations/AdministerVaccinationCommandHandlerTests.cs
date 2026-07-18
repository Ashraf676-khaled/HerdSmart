// HerdSmart.Tests/Features/Vaccinations/AdministerVaccinationCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Vaccinations.Commands.AdministerVaccination;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Vaccinations;

public class AdministerVaccinationCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<ILogger<AdministerVaccinationCommandHandler>> _loggerMock;
    private readonly AdministerVaccinationCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();

    public AdministerVaccinationCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _loggerMock = new Mock<ILogger<AdministerVaccinationCommandHandler>>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _handler = new AdministerVaccinationCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenScheduleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AdministerVaccinationCommand(Ulid.NewUlid(), null);

        var schedules = new List<VaccinationSchedule>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyCompleted_ShouldThrowBadRequestException()
    {
        // Arrange
        var scheduleId = Ulid.NewUlid();
        var command = new AdministerVaccinationCommand(scheduleId, null);

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
    public async Task Handle_WhenVaccineHasNoInterval_ShouldCompleteWithoutBooster()
    {
        // Arrange
        var scheduleId = Ulid.NewUlid();
        var cattleId = Ulid.NewUlid();
        var vaccineId = Ulid.NewUlid();
        var command = new AdministerVaccinationCommand(scheduleId, null);

        var schedule = new VaccinationSchedule
        {
            Id = scheduleId,
            TenantId = _tenantId,
            CattleId = cattleId,
            VaccineId = vaccineId,
            Status = VaccinationStatus.Pending,
            Cattle = new Domain.Entities.Cattle { Id = cattleId, TagNumber = "COW-001" },
            Vaccine = new Vaccine { Id = vaccineId, Name = "Rabies", IntervalInDays = null }
        };

        var schedules = new List<VaccinationSchedule> { schedule }
            .BuildMockDbSet();

        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.ShouldBe(VaccinationStatus.Completed);
        result.AdministeredDate.ShouldNotBeNull();

        schedules.Verify(
            x => x.AddAsync(It.IsAny<VaccinationSchedule>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenVaccineHasInterval_ShouldCreateBoosterSchedule()
    {
        // Arrange
        var scheduleId = Ulid.NewUlid();
        var cattleId = Ulid.NewUlid();
        var vaccineId = Ulid.NewUlid();
        var administeredDate = DateTimeOffset.UtcNow;
        var command = new AdministerVaccinationCommand(scheduleId, administeredDate);

        var schedule = new VaccinationSchedule
        {
            Id = scheduleId,
            TenantId = _tenantId,
            CattleId = cattleId,
            VaccineId = vaccineId,
            Status = VaccinationStatus.Pending,
            Cattle = new Domain.Entities.Cattle { Id = cattleId, TagNumber = "COW-001" },
            Vaccine = new Vaccine { Id = vaccineId, Name = "FMD", IntervalInDays = 180 }
        };

        var schedules = new List<VaccinationSchedule> { schedule }
            .BuildMockDbSet();

        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.ShouldBe(VaccinationStatus.Completed);

        schedules.Verify(
            x => x.AddAsync(
                It.Is<VaccinationSchedule>(s =>
                    s.CattleId == cattleId &&
                    s.VaccineId == vaccineId &&
                    s.Status == VaccinationStatus.Pending &&
                    s.ScheduledDate == administeredDate.AddDays(180)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAdministeredDateNotProvided_ShouldUseUtcNow()
    {
        // Arrange
        var scheduleId = Ulid.NewUlid();
        var command = new AdministerVaccinationCommand(scheduleId, null);

        var schedule = new VaccinationSchedule
        {
            Id = scheduleId,
            TenantId = _tenantId,
            Status = VaccinationStatus.Pending,
            Cattle = new Domain.Entities.Cattle { TagNumber = "COW-001" },
            Vaccine = new Vaccine { Name = "FMD", IntervalInDays = null }
        };

        var schedules = new List<VaccinationSchedule> { schedule }
            .BuildMockDbSet();

        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var before = DateTimeOffset.UtcNow;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        var after = DateTimeOffset.UtcNow;

        // Assert
        result.AdministeredDate.ShouldNotBeNull();
        result.AdministeredDate!.Value.ShouldBeInRange(before, after);
    }
}