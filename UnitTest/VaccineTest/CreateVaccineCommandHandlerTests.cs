// HerdSmart.Tests/Features/Vaccines/CreateVaccineCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Vaccines;
using Application.Features.Vaccines.Commands.CreateVaccine;
using AutoMapper;
using HerdSmart.Domain.Entities;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Vaccines;

public class CreateVaccineCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateVaccineCommandHandler>> _loggerMock;
    private readonly CreateVaccineCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();

    public CreateVaccineCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateVaccineCommandHandler>>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _handler = new CreateVaccineCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = new CreateVaccineCommand("FMD", 4, 2.0, 180);

        var vaccines = new List<Vaccine>
        {
            new() { TenantId = _tenantId, Name = "FMD" }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.Vaccines).Returns(vaccines.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenNameExistsWithDifferentCase_ShouldThrowConflictException()
    {
        // Arrange
        var command = new CreateVaccineCommand("fmd", 4, 2.0, 180);

        var vaccines = new List<Vaccine>
        {
            new() { TenantId = _tenantId, Name = "FMD" }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.Vaccines).Returns(vaccines.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateVaccine()
    {
        // Arrange
        var command = new CreateVaccineCommand("Brucellosis", 6, 3.5, null);

        var vaccines = new List<Vaccine>()
            .BuildMockDbSet();

        var newVaccine = new Vaccine
        {
            TenantId = _tenantId,
            Name = command.Name,
            TargetAgeInMonths = command.TargetAgeInMonths,
            Dosage = command.Dosage,
            IntervalInDays = command.IntervalInDays
        };

        _contextMock.Setup(x => x.Vaccines).Returns(vaccines.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<Vaccine>(command)).Returns(newVaccine);
        _mapperMock.Setup(x => x.Map<VaccineResponse>(It.IsAny<Vaccine>()))
            .Returns(new VaccineResponse(
                newVaccine.Id, newVaccine.Name, newVaccine.TargetAgeInMonths,
                newVaccine.Dosage, newVaccine.IntervalInDays));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Brucellosis");
        result.TargetAgeInMonths.ShouldBe(6);
        result.IntervalInDays.ShouldBeNull();
    }
}