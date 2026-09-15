using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HMS.Modules.Identity.Application.Abstractions
{
    public interface IStaffRepository
    {
        Task<StaffLoginRecord?> GetForLoginAsync(
            string normalizedUsername,
            Guid propertyUid,
            CancellationToken cancellationToken);

        Task RecordSuccessfulLoginAsync(
            Guid staffUid,
            CancellationToken cancellationToken);

        Task RecordFailedLoginAsync(
            Guid staffUid,
            int maximumFailedAttempts,
            TimeSpan lockDuration,
            CancellationToken cancellationToken);
    }
}
