using HMS.Modules.Guests.Domain.Common;
using HMS.Modules.Guests.Domain.Enums;

namespace HMS.Modules.Guests.Domain.Entities;

public sealed class GuestDocument : AuditableEntity
{
    private GuestDocument() { }

    public long OrganizationId { get; private set; }
    public long GuestId { get; private set; }
    public Guid GuestUid { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public string? IssuingCountry { get; private set; }
    public DateOnly? IssuedDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public string? FileUrl { get; private set; }
    public bool IsVerified { get; private set; }

    public static GuestDocument Create(
        long organizationId,
        long guestId,
        Guid guestUid,
        DocumentType documentType,
        string documentNumber,
        string? issuingCountry,
        DateOnly? issuedDate,
        DateOnly? expiryDate,
        string? fileUrl,
        string actorSubject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNumber);

        var cleanedNumber = documentNumber.Trim();
        if (cleanedNumber.Length > 100)
            throw new ArgumentException("Document number cannot exceed 100 characters.", nameof(documentNumber));

        if (expiryDate is not null && issuedDate is not null && expiryDate < issuedDate)
            throw new ArgumentException("Expiry date cannot be before issued date.", nameof(expiryDate));

        var document = new GuestDocument
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            GuestId = guestId,
            GuestUid = guestUid,
            DocumentType = documentType,
            DocumentNumber = cleanedNumber,
            IssuingCountry = OptionalCountryCode(issuingCountry),
            IssuedDate = issuedDate,
            ExpiryDate = expiryDate,
            FileUrl = Optional(fileUrl, 2000)
        };

        document.MarkCreated(actorSubject);
        return document;
    }

    private static string? Optional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.");

        return trimmed;
    }

    private static string? OptionalCountryCode(string? value)
    {
        var cleaned = Optional(value, 2);
        if (cleaned is null)
            return null;

        if (cleaned.Length != 2)
            throw new ArgumentException("Issuing country must be a 2-letter code.", nameof(value));

        return cleaned.ToUpperInvariant();
    }
}
