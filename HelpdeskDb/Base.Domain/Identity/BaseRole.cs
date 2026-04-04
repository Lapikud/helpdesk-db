using Base.Contracts;

namespace Base.Domain.Identity;

public abstract class BaseRole<TUserRole> : BaseRole<Guid, TUserRole>, IDomainId
    where TUserRole : class //BaseUserRole<BaseUser<TUserRole>, BaseRole<TUserRole>>
{
}


public abstract class BaseRole<TKey, TUserRole> : IDomainId<TKey>
    where TKey : IEquatable<TKey>
    where TUserRole : class //BaseUserRole<TKey, BaseUser<TKey, TUserRole>, BaseRole<TKey, TUserRole>>
{
    public TKey Id { get; set; } = default!;
    public string? Name { get; set; }
    public ICollection<TUserRole>? UserRoles { get; set; }
}
