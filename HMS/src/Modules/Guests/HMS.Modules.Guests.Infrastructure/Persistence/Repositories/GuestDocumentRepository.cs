using Dapper;
using HMS.Modules.Guests.Application.Abstractions;
using HMS.Modules.Guests.Domain.Entities;
using HMS.Modules.Guests.Domain.Enums;

namespace HMS.Modules.Guests.Infrastructure.Persistence.Repositories;

public sealed class GuestDocumentRepository(IGuestsDbConnectionFactory connectionFactory)
    : IGuestDocumentRepository
{
    public async Task<bool> NumberExistsAsync(
        long organizationId,
        DocumentType documentType,
        string documentNumber,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.guest_documents
                WHERE organization_id = @OrganizationId
                  AND document_type = @DocumentType
                  AND document_number = @DocumentNumber
                  AND is_archived = false
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new
            {
                OrganizationId = organizationId,
                DocumentType = documentType.ToDatabaseValue(),
                DocumentNumber = documentNumber.Trim()
            },
            cancellationToken: cancellationToken));
    }

    public async Task InsertAsync(GuestDocument document, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.guest_documents
            (
                uid, organization_id, guest_id, document_type, document_number,
                issuing_country, issued_date, expiry_date, file_url, is_verified,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @GuestId, @DocumentType, @DocumentNumber,
                @IssuingCountry, @IssuedDate, @ExpiryDate, @FileUrl, @IsVerified,
                @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                document.Uid,
                document.OrganizationId,
                document.GuestId,
                DocumentType = document.DocumentType.ToDatabaseValue(),
                document.DocumentNumber,
                document.IssuingCountry,
                document.IssuedDate,
                document.ExpiryDate,
                document.FileUrl,
                document.IsVerified,
                document.IsActive,
                document.IsArchived,
                document.CreationDate,
                document.CreatedBy
            },
            cancellationToken: cancellationToken));
    }
}
