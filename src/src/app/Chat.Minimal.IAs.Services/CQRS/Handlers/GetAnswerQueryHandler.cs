using Chat.Minimal.IAs.Services.CQRS.Handlers;
using Chat.Minimal.IAs.Services.CQRS.Queries;
using Chat.Minimal.IAs.Services.DTOs;

namespace Chat.Minimal.IAs.Services.CQRS.Handlers;

public class GetAnswerQueryHandler : IQueryHandler<GetAnswerQuery, AnswerDto?>
{
    public Task<AnswerDto?> HandleAsync(GetAnswerQuery query, CancellationToken cancellationToken = default)
    {
        // Implementação futura se necessário persistência individual de respostas
        // Por enquanto retorna null ou não implementado
        return Task.FromResult<AnswerDto?>(null);
    }
}
