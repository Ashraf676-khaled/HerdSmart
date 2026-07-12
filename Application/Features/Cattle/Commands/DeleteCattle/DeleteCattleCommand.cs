using MediatR;

public record DeleteCattleCommand(Ulid Id) : IRequest;
