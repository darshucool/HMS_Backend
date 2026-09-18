using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using HMS.Modules.Hotels.Domain.Enums;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.CreateProperty;

public sealed record CreatePropertyCommand(
    Guid OrganizationUid,
    string Code,
    string Name,
    string Slug,
    PropertyType PropertyType,
    string? Description,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? District,
    string? Province,
    string? PostalCode,
    string CountryCode,
    decimal? Latitude,
    decimal? Longitude,
    string? Phone,
    string? Email,
    string Timezone,
    string DefaultCurrency,
    string ActorSubject) : IRequest<HotelResult<PropertyDto>>;

public sealed class CreatePropertyCommandHandler(
    IOrganizationRepository organizationRepository,
    IPropertyRepository propertyRepository)
    : IRequestHandler<CreatePropertyCommand, HotelResult<PropertyDto>>
{
    public async Task<HotelResult<PropertyDto>> Handle(
        CreatePropertyCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code)
            || string.IsNullOrWhiteSpace(request.Name)
            || string.IsNullOrWhiteSpace(request.Slug))
        {
            return HotelResult<PropertyDto>.Validation(
                "Property code, name and slug are required.");
        }

        var organization = await organizationRepository.GetByUidAsync(
            request.OrganizationUid,
            cancellationToken);

        if (organization is null || organization.IsArchived)
            return HotelResult<PropertyDto>.NotFound("Organization was not found.");

        if (await propertyRepository.CodeExistsAsync(organization.Id, request.Code.Trim(), cancellationToken))
            return HotelResult<PropertyDto>.Conflict("This property code already exists for the organization.");

        if (await propertyRepository.SlugExistsAsync(request.Slug.Trim(), cancellationToken))
            return HotelResult<PropertyDto>.Conflict("This property URL slug is already in use.");

        Property property;
        try
        {
            property = Property.Create(
                organization.Id,
                organization.Uid,
                request.Code,
                request.Name,
                request.Slug,
                request.PropertyType,
                request.Description,
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.District,
                request.Province,
                request.PostalCode,
                request.CountryCode,
                request.Latitude,
                request.Longitude,
                request.Phone,
                request.Email,
                request.Timezone,
                request.DefaultCurrency,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return HotelResult<PropertyDto>.Validation(exception.Message);
        }

        var defaultSettings = PropertySettings.CreateDefault(organization.Id, request.ActorSubject);
        await propertyRepository.InsertAsync(property, defaultSettings, cancellationToken);

        return HotelResult<PropertyDto>.Success(PropertyMapper.ToDto(property));
    }
}
