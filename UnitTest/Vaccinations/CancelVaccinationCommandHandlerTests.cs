// HerdSmart.Tests/Features/Vaccinations/CancelVaccinationCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Vaccinations.Commands.CancelVaccination;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Vaccinations;

public class CancelVaccinationCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<ILogger<CancelVaccinationCommandHandler>> _loggerMock;
    private readonly CancelVaccinationCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();

    public CancelVaccinationCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _loggerMock = new Mock<ILogger<CancelVaccinationCommandHandler>>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _handler = new CancelVaccinationCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenScheduleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CancelVaccinationCommand(Ulid.NewUlid());

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
        var command = new CancelVaccinationCommand(scheduleId);

        var schedules = new List<VaccinationSchedule>
        {
            new()
            {
                Id = scheduleId,
                TenantId = _tenantId,
                Status = VaccinationStatus.Completed
            }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldSoftDeleteSchedule()
    {
        // Arrange
        var scheduleId = Ulid.NewUlid();
        var command = new CancelVaccinationCommand(scheduleId);

        var schedule = new VaccinationSchedule
        {
            Id = scheduleId,
            TenantId = _tenantId,
            Status = VaccinationStatus.Pending
        };

        var schedules = new List<VaccinationSchedule> { schedule }
            .BuildMockDbSet();

        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        schedule.DeletedAt.ShouldNotBeNull();
    }
}