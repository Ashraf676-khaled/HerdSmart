// HerdSmart.Tests/Features/Cattle/UpdateCattleCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Cattle.Commands.UpdateCattle;
using Application.Features.Cattle.Dtos;
using AutoMapper;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Cattle;

public class UpdateCattleCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UpdateCattleCommandHandler>> _loggerMock;
    private readonly UpdateCattleCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();

    public UpdateCattleCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UpdateCattleCommandHandler>>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _handler = new UpdateCattleCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCattleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateCattleCommand(
            Ulid.NewUlid(), "COW-001", "Holstein",
            DateTimeOffset.UtcNow.AddYears(-2), null, null);

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
        var command = new UpdateCattleCommand(
            cattleId, "COW-001", "Holstein",
            DateTimeOffset.UtcNow.AddYears(-2), null, null);

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
    public async Task Handle_WhenTagNumberAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new UpdateCattleCommand(
            cattleId, "COW-002", "Holstein",
            DateTimeOffset.UtcNow.AddYears(-2), null, null);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            new() { Id = cattleId, TenantId = _tenantId,
                    TagNumber = "COW-001", Status = CattleStatus.Active },
            new() { Id = Ulid.NewUlid(), TenantId = _tenantId,
                    TagNumber = "COW-002", Status = CattleStatus.Active }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldUpdateCattle()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new UpdateCattleCommand(
            cattleId, "COW-001-UPDATED", "Jersey",
            DateTimeOffset.UtcNow.AddYears(-2), "BULL-001", null);

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

        var response = new CattleResponse(
            cattleId, "COW-001-UPDATED", "Jersey",
            Gender.Female, command.BirthDate,
            CattleStatus.Active, "BULL-001", null, DateTimeOffset.UtcNow);

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<CattleResponse>(existingCattle))
            .Returns(response);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        existingCattle.TagNumber.ShouldBe("COW-001-UPDATED");
        existingCattle.Breed.ShouldBe("Jersey");
        existingCattle.FatherTagNumber.ShouldBe("BULL-001");
    }
}