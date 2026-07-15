// HerdSmart.Tests/Features/Vaccines/DeleteVaccineCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Vaccines.Commands.DeleteVaccine;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Vaccines;

public class DeleteVaccineCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<ILogger<DeleteVaccineCommandHandler>> _loggerMock;
    private readonly DeleteVaccineCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();

    public DeleteVaccineCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _loggerMock = new Mock<ILogger<DeleteVaccineCommandHandler>>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _handler = new DeleteVaccineCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenVaccineNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteVaccineCommand(Ulid.NewUlid());

        var vaccines = new List<Vaccine>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Vaccines).Returns(vaccines.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVaccineHasLinkedSchedules_ShouldThrowBadRequestException()
    {
        // Arrange
        var vaccineId = Ulid.NewUlid();
        var command = new DeleteVaccineCommand(vaccineId);

        var vaccines = new List<Vaccine>
        {
            new() { Id = vaccineId, TenantId = _tenantId, Name = "FMD" }
        }.BuildMockDbSet();

        var schedules = new List<VaccinationSchedule>
        {
            new() { VaccineId = vaccineId, Status = VaccinationStatus.Pending }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.Vaccines).Returns(vaccines.Object);
        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeleteVaccine()
    {
        // Arrange
        var vaccineId = Ulid.NewUlid();
        var command = new DeleteVaccineCommand(vaccineId);

        var vaccine = new Vaccine { Id = vaccineId, TenantId = _tenantId, Name = "FMD" };

        var vaccines = new List<Vaccine> { vaccine }
            .BuildMockDbSet();

        var schedules = new List<VaccinationSchedule>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Vaccines).Returns(vaccines.Object);
        _contextMock.Setup(x => x.VaccinationSchedules).Returns(schedules.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.Vaccines.Remove(vaccine), Times.Once);
    }
}