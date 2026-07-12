// HerdSmart.Tests/Features/Cattle/CreateCattleCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Cattle.Commands.CreateCattle;
using Application.Features.Cattle.Dtos;
using AutoMapper;
using HerdSmart.Domain.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Cattle;

public class CreateCattleCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateCattleCommandHandler>> _loggerMock;
    private readonly CreateCattleCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();

    public CreateCattleCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateCattleCommandHandler>>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _handler = new CreateCattleCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTagNumberExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = new CreateCattleCommand(
            "COW-001", "Holstein", Gender.Female,
            DateTimeOffset.UtcNow.AddYears(-2), null, null);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            new() { TagNumber = "COW-001", TenantId = _tenantId }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateCattle()
    {
        // Arrange
        var command = new CreateCattleCommand(
            "COW-002", "Holstein", Gender.Female,
            DateTimeOffset.UtcNow.AddYears(-2), null, null);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>()
            .BuildMockDbSet();

        var newCattle = new HerdSmart.Domain.Entities.Cattle
        {
            TagNumber = command.TagNumber,
            Breed = command.Breed,
            TenantId = _tenantId,
            Status = CattleStatus.Active
        };

        var response = new CattleResponse(
            Ulid.NewUlid(), command.TagNumber, command.Breed,
            command.Gender, command.BirthDate, CattleStatus.Active,
            null, null, DateTimeOffset.UtcNow);

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<HerdSmart.Domain.Entities.Cattle>(command))
            .Returns(newCattle);
        _mapperMock.Setup(x => x.Map<CattleResponse>(newCattle))
            .Returns(response);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.TagNumber.ShouldBe(command.TagNumber);
        result.Status.ShouldBe(CattleStatus.Active);
    }

    [Fact]
    public async Task Handle_WhenTagNumberExistsInDifferentTenant_ShouldCreateCattle()
    {
        // Arrange - نفس الـ TagNumber بس Tenant تاني
        var command = new CreateCattleCommand(
            "COW-001", "Holstein", Gender.Female,
            DateTimeOffset.UtcNow.AddYears(-2), null, null);

        // Cattle تابعة لـ Tenant تاني
        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            new() { TagNumber = "COW-001", TenantId = Ulid.NewUlid() } // ← Tenant مختلف
        }.BuildMockDbSet();

        var newCattle = new HerdSmart.Domain.Entities.Cattle
        {
            TagNumber = command.TagNumber,
            TenantId = _tenantId,
            Status = CattleStatus.Active
        };

        var response = new CattleResponse(
            Ulid.NewUlid(), command.TagNumber, command.Breed,
            command.Gender, command.BirthDate, CattleStatus.Active,
            null, null, DateTimeOffset.UtcNow);

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<HerdSmart.Domain.Entities.Cattle>(command))
            .Returns(newCattle);
        _mapperMock.Setup(x => x.Map<CattleResponse>(newCattle))
            .Returns(response);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull(); // ← مش هيرمي Exception
    }
}