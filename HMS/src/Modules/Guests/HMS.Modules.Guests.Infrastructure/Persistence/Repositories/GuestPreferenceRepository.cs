using Dapper;
using HMS.Modules.Guests.Application.Abstractions;
using HMS.Modules.Guests.Domain.Entities;
using HMS.Modules.Guests.Domain.Enums;

namespace HMS.Modules.Guests.Infrastructure.Persistence.Repositories;

public sealed class GuestPreferenceRepository(IGuestsDbConnectionFactory connectionFactory)
    : IGuestPreferenceRepository
{
    public async Task InsertAsync(GuestPreference preference, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.guest_preferences
            (
                uid, organization_id, guest_id, preference_type, preference_key,
                preference_value, notes, is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @GuestId, @PreferenceType, @PreferenceKey,
                @PreferenceValue, @Notes, @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                preference.Uid,
                preference.OrganizationId,
                preference.GuestId,
                PreferenceType = preference.PreferenceType.ToDatabaseValue(),
                preference.PreferenceKey,
                preference.PreferenceValue,
                preference.Notes,
                preference.IsActive,
                preference.IsArchived,
                preference.CreationDate,
                preference.CreatedBy
            },
            cancellationToken: cancellationToken));
    }
}
