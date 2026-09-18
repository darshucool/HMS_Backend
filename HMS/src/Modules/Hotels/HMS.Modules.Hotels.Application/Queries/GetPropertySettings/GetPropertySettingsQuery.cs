using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetPropertySettings;

public sealed record GetPropertySettingsQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<PropertySettingsDto>>;

public sealed class GetPropertySettingsQueryHandler(IPropertyRepository repository)
    : IRequestHandler<GetPropertySettingsQuery, HotelResult<PropertySettingsDto>>
{
    public async Task<HotelResult<PropertySettingsDto>> Handle(
        GetPropertySettingsQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await repository.HasAccessAsync(request.ActorSubject, request.PropertyUid, false, cancellationToken))
        {
            return HotelResult<PropertySettingsDto>.Forbidden("You cannot access settings for this property.");
        }

        var settings = await repository.GetSettingsAsync(request.PropertyUid, cancellationToken);
        return settings is null
            ? HotelResult<PropertySettingsDto>.NotFound("Property settings were not found.")
            : HotelResult<PropertySettingsDto>.Success(
                PropertyMapper.ToSettingsDto(request.PropertyUid, settings));
    }
}

