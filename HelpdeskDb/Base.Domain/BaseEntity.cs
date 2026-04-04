using System.ComponentModel.DataAnnotations;

namespace Base.Domain;

using Base.Contracts;

public abstract class BaseEntity : BaseEntity<Guid>, IDomainId
{
}

public abstract class BaseEntity<TKey>: IDomainId<TKey>, IDomainMeta
    where TKey : IEquatable<TKey>
{
    public TKey Id { get; set; } = default!;
    
    [MaxLength(64)]
    public string CreatedBy { get; set; } = "System";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(64)]
    public string ChangedBy { get; set; } = "System";
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    
    public string? SysNotes { get; set; }
}
