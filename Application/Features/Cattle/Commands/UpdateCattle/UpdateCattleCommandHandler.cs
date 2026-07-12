// Application/Features/Cattle/Commands/UpdateCattle/UpdateCattleCommand.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Cattle.Dtos;
using AutoMapper;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Cattle.Commands.UpdateCattle;


public class UpdateCattleCommandHandler : IRequestHandler<UpdateCattleCommand, CattleResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCattleCommandHandler> _logger;

    public UpdateCattleCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        IMapper mapper,
        ILogger<UpdateCattleCommandHandler> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CattleResponse> Handle(
        UpdateCattleCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var cattle = await _context.Cattle
            .FirstOrDefaultAsync(
                c => c.Id == request.Id && c.TenantId == tenantId,
                cancellationToken);

        if (cattle is null)
            throw new NotFoundException("Cattle not found");

        // Business Rule
        if (cattle.Status == CattleStatus.Dead)
            throw new BadRequestException("Cannot update dead cattle");

        // تحقق من TagNumber لو اتغير
        if (cattle.TagNumber != request.TagNumber)
        {
            if (await _context.Cattle.AnyAsync(
                c => c.TagNumber == request.TagNumber && c.TenantId == tenantId,
                cancellationToken))
                throw new ConflictException($"TagNumber {request.TagNumber} already exists");
        }

        cattle.TagNumber = request.TagNumber;
        cattle.Breed = request.Breed;
        cattle.BirthDate = request.BirthDate;
        cattle.FatherTagNumber = request.FatherTagNumber;
        cattle.MotherTagNumber = request.MotherTagNumber;
        cattle.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cattle updated: {CattleId}", cattle.Id);

        return _mapper.Map<CattleResponse>(cattle);
    }
}