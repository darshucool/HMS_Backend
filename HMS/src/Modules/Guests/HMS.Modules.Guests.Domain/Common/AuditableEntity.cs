namespace HMS.Modules.Guests.Domain.Common;

public abstract class AuditableEntity
{
    public long Id { get; protected set; }
    public Guid Uid { get; protected set; } = Guid.NewGuid();
    public bool IsActive { get; protected set; } = true;
    public bool IsArchived { get; protected set; }
    public DateTimeOffset CreationDate { get; protected set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; protected set; }
    public DateTimeOffset? ModifiedDate { get; protected set; }
    public string? ModifiedBy { get; protected set; }

    protected void MarkCreated(string actorSubject)
    {
        CreatedBy = actorSubject;
        CreationDate = DateTimeOffset.UtcNow;
    }

    protected void MarkModified(string actorSubject)
    {
        ModifiedBy = actorSubject;
        ModifiedDate = DateTimeOffset.UtcNow;
    }
}
