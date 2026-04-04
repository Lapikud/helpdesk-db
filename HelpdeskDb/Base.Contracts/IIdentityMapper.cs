namespace Base.Contracts;

public interface IIdentityMapper<TUpperEntity, TLowerEntity> : IIdentityMapper<TUpperEntity, TLowerEntity, Guid>
    where TUpperEntity : class
    where TLowerEntity : class
{
}

public interface IIdentityMapper<TUpperEntity, TLowerEntity, TKey>
    where TKey : IEquatable<TKey>
    where TUpperEntity : class
    where TLowerEntity : class
{
    public TUpperEntity? Map(TLowerEntity? entity);
    public TLowerEntity? Map(TUpperEntity? entity);
}