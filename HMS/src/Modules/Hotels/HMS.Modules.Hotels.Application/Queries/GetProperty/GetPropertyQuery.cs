using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetProperty;

public sealed record GetPropertyQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<PropertyDto>>;

public sealed class GetPropertyQueryHandler(IPropertyRepository repository)
    : IRequestHandler<GetPropertyQuery, HotelResult<PropertyDto>>
{
    public async Task<HotelResult<PropertyDto>> Handle(
        GetPropertyQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await repository.HasAccessAsync(request.ActorSubject, request.PropertyUid, false, cancellationToken))
        {
            return HotelResult<PropertyDto>.Forbidden("You cannot access this property.");
        }

        var property = await repository.GetByUidAsync(request.PropertyUid, cancellationToken);
        return property is null || property.IsArchived
            ? HotelResult<PropertyDto>.NotFound("Property was not found.")
            : HotelResult<PropertyDto>.Success(PropertyMapper.ToDto(property));
    }
}

