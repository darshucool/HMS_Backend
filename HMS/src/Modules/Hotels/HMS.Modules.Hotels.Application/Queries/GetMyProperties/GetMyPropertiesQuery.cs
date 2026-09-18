using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetMyProperties;

public sealed record GetMyPropertiesQuery(string ActorSubject)
    : IRequest<HotelResult<IReadOnlyList<MyPropertyDto>>>;

public sealed class GetMyPropertiesQueryHandler(IPropertyRepository repository)
    : IRequestHandler<GetMyPropertiesQuery, HotelResult<IReadOnlyList<MyPropertyDto>>>
{
    public async Task<HotelResult<IReadOnlyList<MyPropertyDto>>> Handle(
        GetMyPropertiesQuery request,
        CancellationToken cancellationToken)
    {
        var properties = await repository.GetMyPropertiesAsync(request.ActorSubject, cancellationToken);
        return HotelResult<IReadOnlyList<MyPropertyDto>>.Success(properties);
    }
}

