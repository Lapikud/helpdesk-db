using Base.Contracts;

namespace Base.Domain.Identity;

public abstract class BaseUser<TUserRole> : BaseUser<Guid, TUserRole>, IDomainId
    where TUserRole : class //BaseUserRole<BaseUser<TUserRole>, BaseRole<TUserRole>>
{
}

public abstract class BaseUser<TKey, TUserRole> : IDomainId<TKey>
    where TKey : IEquatable<TKey>
    where TUserRole : class //BaseUserRole<TKey, BaseUser<TKey, TUserRole>, BaseRole<TKey, TUserRole>>
{
    public TKey Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public ICollection<TUserRole>? UserRoles { get; set; }
}