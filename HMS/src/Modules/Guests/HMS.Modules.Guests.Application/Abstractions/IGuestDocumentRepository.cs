using HMS.Modules.Guests.Domain.Entities;
using HMS.Modules.Guests.Domain.Enums;

namespace HMS.Modules.Guests.Application.Abstractions;

public interface IGuestDocumentRepository
{
    Task<bool> NumberExistsAsync(
        long organizationId,
        DocumentType documentType,
        string documentNumber,
        CancellationToken cancellationToken);
    Task InsertAsync(GuestDocument document, CancellationToken cancellationToken);
}
