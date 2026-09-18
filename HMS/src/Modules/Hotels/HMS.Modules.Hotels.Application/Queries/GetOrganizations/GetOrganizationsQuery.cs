using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetOrganizations;

public sealed record GetOrganizationsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IRequest<HotelResult<PagedResult<OrganizationDto>>>;

public sealed class GetOrganizationsQueryHandler(IOrganizationRepository repository)
    : IRequestHandler<GetOrganizationsQuery, HotelResult<PagedResult<OrganizationDto>>>
{
    public async Task<HotelResult<PagedResult<OrganizationDto>>> Handle(
        GetOrganizationsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1 || request.PageSize is < 1 or > 100)
            return HotelResult<PagedResult<OrganizationDto>>.Validation(
                "Page must be at least 1 and pageSize must be between 1 and 100.");

        var (items, totalCount) = await repository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            cancellationToken);

        return HotelResult<PagedResult<OrganizationDto>>.Success(
            new PagedResult<OrganizationDto>(items, request.Page, request.PageSize, totalCount));
    }
}

