// Application/Features/Cattle/Commands/DeleteCattle/DeleteCattleCommand.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Cattle.Commands.DeleteCattle;

public class DeleteCattleCommandHandler : IRequestHandler<DeleteCattleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<DeleteCattleCommandHandler> _logger;

    public DeleteCattleCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        ILogger<DeleteCattleCommandHandler> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(
        DeleteCattleCommand request,
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
            throw new BadRequestException("Dead cattle cannot be deleted");

        // Soft Delete عن طريق الـ SaveChangesAsync
        _context.Cattle.Remove(cattle);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cattle deleted: {CattleId}", cattle.Id);
    }
}