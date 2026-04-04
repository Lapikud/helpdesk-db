using Base.BLL.Contracts;
using Base.Contracts;
using Base.DAL.Contracts;

namespace Base.BLL;

public class BaseService<TBllEntity, TDalEntity, TDalRepository> : BaseService<TBllEntity, TDalEntity, TDalRepository, Guid>, IBaseService<TBllEntity>
    where TBllEntity : class, IDomainId
    where TDalEntity : class, IDomainId
    where TDalRepository: class, IBaseRepository<TDalEntity>

{
    public BaseService(IBaseUOW serviceUOW, TDalRepository serviceRepository, IMapper<TBllEntity, TDalEntity, Guid> mapper) : base(serviceUOW, serviceRepository, mapper)
    {
    }
}

public class BaseService<TBllEntity, TDalEntity, TDalRepository, TKey>: IBaseService<TBllEntity, TKey>
    where TBllEntity : class, IDomainId<TKey>
    where TDalEntity : class, IDomainId<TKey>
    where TDalRepository: class, IBaseRepository<TDalEntity, TKey>
    where TKey : IEquatable<TKey>
{
    protected readonly IBaseUOW ServiceUOW;
    protected readonly TDalRepository ServiceRepository;
    protected readonly IMapper<TBllEntity, TDalEntity, TKey> Mapper;


    public BaseService(IBaseUOW serviceUOW, TDalRepository serviceRepository, IMapper<TBllEntity, TDalEntity, TKey> mapper)
    {
        ServiceUOW = serviceUOW;
        ServiceRepository = serviceRepository;
        Mapper = mapper;
    }
    
    

    public virtual IEnumerable<TBllEntity> All(TKey? userId = default)
    {
        var entities = ServiceRepository.All(userId);
        return entities.Select(e => Mapper.Map(e)!).ToList(); 
    }

    public virtual async Task<IEnumerable<TBllEntity>> AllAsync(TKey? userId = default)
    {
        var entities = await ServiceRepository.AllAsync(userId);
        return entities.Select(e => Mapper.Map(e)!).ToList(); 
    }

    public virtual TBllEntity? Find(TKey id, TKey? userId = default)
    {
        var entity = ServiceRepository.Find(id, userId);
        return Mapper.Map(entity);
    }

    public virtual async Task<TBllEntity?> FindAsync(TKey id, TKey? userId = default)
    {
        var entity = await ServiceRepository.FindAsync(id, userId);
        return Mapper.Map(entity);
    }

    public virtual TBllEntity Add(TBllEntity entity, TKey? userId = default)
    {
        var dalEntity = Mapper.Map(entity);
        var addedEntity = ServiceRepository.Add(dalEntity!, userId);
        return Mapper.Map(addedEntity)!;
    }
    
    public virtual async Task<TBllEntity> AddAsync(TBllEntity entity, TKey? userId = default)
    {
        var dalEntity = Mapper.Map(entity);
        var addedEntity = await ServiceRepository.AddAsync(dalEntity!, userId);
        return Mapper.Map(addedEntity)!;
    }

    public virtual TBllEntity? Update(TBllEntity entity, TKey? userId = default)
    {
        var dalEntity = Mapper.Map(entity);
        var updatedEntity = ServiceRepository.Update(dalEntity!, userId);
        return Mapper.Map(updatedEntity);
    }
    
    public virtual async Task<TBllEntity?> UpdateAsync(TBllEntity entity, TKey? userId = default)
    {
        var dalEntity = Mapper.Map(entity);
        var updatedEntity = await ServiceRepository.UpdateAsync(dalEntity!, userId);
        return Mapper.Map(updatedEntity);
    }

    public virtual void Remove(TBllEntity entity, TKey? userId = default)
    {
        Remove(entity.Id, userId);
    }

    public virtual void Remove(TKey id, TKey? userId = default)
    {
        var entity = ServiceRepository.Find(id, userId);
        if (entity != null)
        {
            ServiceRepository.Remove(entity, userId);
        }
    }

    public virtual async Task RemoveAsync(TKey id, TKey? userId = default)
    {
        var entity = await ServiceRepository.FindAsync(id, userId);
        if (entity != null)
        {
            await ServiceRepository.RemoveAsync(id, userId);
        }
    }

    public virtual bool Exists(TKey id, TKey? userId = default)
    {
        var entity = ServiceRepository.Find(id, userId);
        return entity != null;
    }

    public virtual async Task<bool> ExistsAsync(TKey id, TKey? userId = default)
    {
        var entity = await ServiceRepository.FindAsync(id, userId);
        return entity != null;
    }
}