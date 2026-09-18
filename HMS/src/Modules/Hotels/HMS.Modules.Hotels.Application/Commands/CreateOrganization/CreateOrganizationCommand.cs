using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.CreateOrganization;

public sealed record CreateOrganizationCommand(
    string Code,
    string Name,
    string? LegalName,
    string DefaultCurrency,
    string Timezone,
    string ActorSubject) : IRequest<HotelResult<OrganizationDto>>;

public sealed class CreateOrganizationCommandHandler(
    IOrganizationRepository repository)
    : IRequestHandler<CreateOrganizationCommand, HotelResult<OrganizationDto>>
{
    public async Task<HotelResult<OrganizationDto>> Handle(
        CreateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return HotelResult<OrganizationDto>.Validation("Code and name are required.");

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await repository.CodeExistsAsync(normalizedCode, cancellationToken))
            return HotelResult<OrganizationDto>.Conflict("An organization with this code already exists.");

        Organization organization;
        try
        {
            organization = Organization.Create(
                normalizedCode,
                request.Name,
                request.LegalName,
                request.DefaultCurrency,
                request.Timezone,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return HotelResult<OrganizationDto>.Validation(exception.Message);
        }

        await repository.InsertAsync(organization, cancellationToken);
        return HotelResult<OrganizationDto>.Success(ToDto(organization));
    }

    private static OrganizationDto ToDto(Organization item) => new(
        item.Uid,
        item.Code,
        item.Name,
        item.LegalName,
        item.DefaultCurrency,
        item.Timezone,
        item.Status.ToString().ToUpperInvariant(),
        item.IsActive,
        item.CreationDate);
}

