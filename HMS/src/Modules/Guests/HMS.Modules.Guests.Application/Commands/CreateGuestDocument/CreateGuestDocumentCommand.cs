using HMS.Modules.Guests.Application.Abstractions;
using HMS.Modules.Guests.Application.Common;
using HMS.Modules.Guests.Application.DTOs;
using HMS.Modules.Guests.Domain.Entities;
using HMS.Modules.Guests.Domain.Enums;
using MediatR;

namespace HMS.Modules.Guests.Application.Commands.CreateGuestDocument;

public sealed record CreateGuestDocumentCommand(
    Guid GuestUid,
    DocumentType DocumentType,
    string DocumentNumber,
    string? IssuingCountry,
    DateOnly? IssuedDate,
    DateOnly? ExpiryDate,
    string? FileUrl,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<GuestResult<GuestDocumentDto>>;

public sealed class CreateGuestDocumentCommandHandler(
    IPropertyAccess propertyAccess,
    IGuestRepository guestRepository,
    IGuestDocumentRepository documentRepository)
    : IRequestHandler<CreateGuestDocumentCommand, GuestResult<GuestDocumentDto>>
{
    public async Task<GuestResult<GuestDocumentDto>> Handle(
        CreateGuestDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var guest = await guestRepository.GetByUidAsync(request.GuestUid, cancellationToken);
        if (guest is null || guest.IsArchived)
            return GuestResult<GuestDocumentDto>.NotFound("Guest was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessToOrganizationAsync(
                request.ActorSubject,
                guest.OrganizationId,
                true,
                cancellationToken))
        {
            return GuestResult<GuestDocumentDto>.Forbidden("You cannot add documents for this guest.");
        }

        if (await documentRepository.NumberExistsAsync(
                guest.OrganizationId,
                request.DocumentType,
                request.DocumentNumber,
                cancellationToken))
        {
            return GuestResult<GuestDocumentDto>.Conflict(
                "This document number already exists for the organization.");
        }

        GuestDocument document;
        try
        {
            document = GuestDocument.Create(
                guest.OrganizationId,
                guest.Id,
                guest.Uid,
                request.DocumentType,
                request.DocumentNumber,
                request.IssuingCountry,
                request.IssuedDate,
                request.ExpiryDate,
                request.FileUrl,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return GuestResult<GuestDocumentDto>.Validation(exception.Message);
        }

        await documentRepository.InsertAsync(document, cancellationToken);
        return GuestResult<GuestDocumentDto>.Success(GuestMapper.ToDto(document));
    }
}
