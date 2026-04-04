using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Domain.Identity;

namespace Base.Domain;

public abstract class BaseEntityUser<TUser> : BaseEntityUser<Guid, TUser>, IDomainId, IDomainUserId
    where TUser : class
{
}

public abstract class BaseEntityUser<TKey, TUser> : BaseEntity<TKey>, IDomainUserId<TKey>
    where TKey : IEquatable<TKey>
    where TUser : class
{
    public TKey UserId { get; set; } = default!;
    public TUser? User { get; set; }
}