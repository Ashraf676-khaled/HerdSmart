// HerdSmart.Tests/Features/Vaccinations/CreateVaccinationScheduleCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Vaccinations.Commands.CreateVaccinationSchedule;
using AutoMapper;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Vaccinations;

public class CreateVaccinationScheduleCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateVaccinationScheduleCommandHandler>> _loggerMock;
    private readonly CreateVaccinationScheduleCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();

    public CreateVaccinationScheduleCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateVaccinationScheduleCommandHandler>>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _handler = new CreateVaccinationScheduleCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object,
            _loggerMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCattleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CreateVaccinationScheduleCommand(
            Ulid.NewUlid(), Ulid.NewUlid(), DateTimeOffset.UtcNow);

        var cattle = new List<Domain.Entities.Cattle>()
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
        var command = new CreateVaccinationScheduleCommand(
            cattleId, Ulid.NewUlid(), DateTimeOffset.UtcNow);

        var cattle = new List<Domain.Entities.Cattle>
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
    public async Task Handle_WhenVaccineNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new CreateVaccinationScheduleCommand(
            cattleId, Ulid.NewUlid(), DateTimeOffset.UtcNow);

        var cattle = new List<Domain.Entities.Cattle>
        {
            new() { Id = cattleId, TenantId = _tenantId, Status = CattleStatus.Active }
        }.BuildMockDbSet();

        var vaccines = new List<Vaccine>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.Vaccines).Returns(vaccines.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenActiveScheduleAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var vaccineId = Ulid.NewUlid();
        var command = new CreateVaccinationScheduleCommand(
            cattleId, vaccineId, DateTimeOffset.UtcNow);

        var cattle = new List<Domain.Entities.Cattle>
        {
            new() { Id = cattleId, TenantId = _tenantId, Status = CattleStatus.Active }
        }.BuildMockDbSet();

        var vaccines = new List<Vaccine>
        {
            new() { Id = vaccineId, TenantId = _tenantId, Name = "FMD" }
        }.BuildMockDbSet();

        var schedules = new List<VaccinationSchedule>
        {
            new()
            {
                CattleId = cattleId,
                VaccineId = vaccineId,
                Status = VaccinationStatus.Pending
            }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.Vaccines).Returns(vaccines.Object);
        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateSchedule()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var vaccineId = Ulid.NewUlid();
        var scheduledDate = DateTimeOffset.UtcNow.AddDays(5);
        var command = new CreateVaccinationScheduleCommand(cattleId, vaccineId, scheduledDate);

        var existingCattle = new Domain.Entities.Cattle
        {
            Id = cattleId,
            TenantId = _tenantId,
            TagNumber = "COW-001",
            Status = CattleStatus.Active
        };

        var existingVaccine = new Vaccine
        {
            Id = vaccineId,
            TenantId = _tenantId,
            Name = "FMD"
        };

        var cattle = new List<Domain.Entities.Cattle> { existingCattle }
            .BuildMockDbSet();

        var vaccines = new List<Vaccine> { existingVaccine }
            .BuildMockDbSet();

        var schedules = new List<VaccinationSchedule>()
            .BuildMockDbSet();

        var newSchedule = new VaccinationSchedule
        {
            CattleId = cattleId,
            VaccineId = vaccineId,
            ScheduledDate = scheduledDate
        };

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.Vaccines).Returns(vaccines.Object);
        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<VaccinationSchedule>(command))
            .Returns(newSchedule);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.CattleTagNumber.ShouldBe("COW-001");
        result.VaccineName.ShouldBe("FMD");
        result.Status.ShouldBe(VaccinationStatus.Pending);
    }
}