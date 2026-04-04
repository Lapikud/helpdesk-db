namespace Base.Contracts;

public interface IMapper<TUpperEntity, TLowerEntity> : IMapper<TUpperEntity, TLowerEntity, Guid>
    where TUpperEntity : class, IDomainId
    where TLowerEntity : class, IDomainId
{
}

public interface IMapper<TUpperEntity, TLowerEntity, TKey>
    where TKey : IEquatable<TKey>
    where TUpperEntity : class, IDomainId<TKey>
    where TLowerEntity : class, IDomainId<TKey>
{
    public TUpperEntity? Map(TLowerEntity? entity);
    public TLowerEntity? Map(TUpperEntity? entity);
}
