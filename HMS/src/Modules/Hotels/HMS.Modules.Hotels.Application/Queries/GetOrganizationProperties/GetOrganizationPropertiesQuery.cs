using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetOrganizationProperties;

public sealed record GetOrganizationPropertiesQuery(
    Guid OrganizationUid) : IRequest<HotelResult<IReadOnlyList<PropertyDto>>>;

public sealed class GetOrganizationPropertiesQueryHandler(
    IOrganizationRepository organizationRepository,
    IPropertyRepository propertyRepository)
    : IRequestHandler<GetOrganizationPropertiesQuery, HotelResult<IReadOnlyList<PropertyDto>>>
{
    public async Task<HotelResult<IReadOnlyList<PropertyDto>>> Handle(
        GetOrganizationPropertiesQuery request,
        CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByUidAsync(
            request.OrganizationUid,
            cancellationToken);

        if (organization is null || organization.IsArchived)
            return HotelResult<IReadOnlyList<PropertyDto>>.NotFound("Organization was not found.");

        var items = await propertyRepository.GetByOrganizationUidAsync(
            request.OrganizationUid,
            cancellationToken);

        return HotelResult<IReadOnlyList<PropertyDto>>.Success(items);
    }
}
