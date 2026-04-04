using Base.Contracts;
using Base.DAL.Contracts;
using Base.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Base.DAL.EF;

public class BaseRepository<TDalEntity, TDomainEntity> : BaseRepository<TDalEntity, TDomainEntity, Guid>,
    IBaseRepository<TDalEntity>
    where TDalEntity : class, IDomainId
    where TDomainEntity : class, IDomainId
{
    public BaseRepository(DbContext repositoryDbContext, IMapper<TDalEntity, TDomainEntity> mapper)
        : base(repositoryDbContext, mapper)
    {
    }
}

public class BaseRepository<TDalEntity, TDomainEntity, TKey> : IBaseRepository<TDalEntity, TKey>
    where TDalEntity : class, IDomainId<TKey>
    where TDomainEntity : class, IDomainId<TKey>
    where TKey : IEquatable<TKey>
{
    protected DbContext RepositoryDbContext;
    protected DbSet<TDomainEntity> RepositoryDbSet;
    protected IMapper<TDalEntity, TDomainEntity, TKey> Mapper;
    // protected EntityMapper<TDalEntity, TDomainEntity> Mapper;

    public BaseRepository(DbContext repositoryDbContext, IMapper<TDalEntity, TDomainEntity, TKey> mapper)
    {
        RepositoryDbContext = repositoryDbContext;
        Mapper = mapper;
        RepositoryDbSet = RepositoryDbContext.Set<TDomainEntity>();
    }


    protected virtual IQueryable<TDomainEntity> GetQuery(TKey? userId = default!)
    {
        var query = RepositoryDbSet.AsQueryable();

        if (ShouldUseUserId(userId))
        {
            query = query.Where(e => ((IDomainUserId<TKey>)e).UserId.Equals(userId));
        }

        return query;
    }

    public virtual IEnumerable<TDalEntity> All(TKey? userId = default!)
    {
        return GetQuery(userId)
            .ToList()
            .Select(e => Mapper.Map(e)!);
    }

    public virtual async Task<IEnumerable<TDalEntity>> AllAsync(TKey? userId = default!)
    {
        return (await GetQuery(userId)
                .ToListAsync())
            .Select(e => Mapper.Map(e)!);
    }

    public virtual TDalEntity? Find(TKey id, TKey? userId = default!)
    {
        var query = GetQuery(userId);
        var res = query.FirstOrDefault(e => e.Id.Equals(id));
        return Mapper.Map(res);
    }

    public virtual async Task<TDalEntity?> FindAsync(TKey id, TKey? userId = default!)
    {
        var query = GetQuery(userId);
        var res = await query.FirstOrDefaultAsync(e => e.Id.Equals(id));
        return Mapper.Map(res);
    }

    public virtual TDalEntity Add(TDalEntity entity, TKey? userId = default!)
    {
        var dbEntity = Mapper.Map(entity);
        
        if (ShouldUseUserId(userId))
        {
           ((IDomainUserId<TKey>) dbEntity!).UserId = userId!;
        }
        
        var entry = RepositoryDbSet.Add(dbEntity!);
        return Mapper.Map(entry.Entity)!;
    }
    
    public virtual async Task<TDalEntity> AddAsync(TDalEntity entity, TKey? userId = default!)
    {
        var dbEntity = Mapper.Map(entity);
        
        if (ShouldUseUserId(userId))
        {
            ((IDomainUserId<TKey>) dbEntity!).UserId = userId!;
        }
        
        var entry = await RepositoryDbContext.AddAsync(dbEntity!);
        return Mapper.Map(entry.Entity)!;
    }

    // public virtual TDalEntity Update(TDalEntity entity)
    // {
    //     return Mapper.Map(RepositoryDbSet.Update(Mapper.Map(entity)!).Entity)!;
    // }
    
    public virtual TDalEntity? Update(TDalEntity entity, TKey? userId = default!)
    {
        var domainEntity = Mapper.Map(entity)!;

        if (ShouldUseUserId(userId))
        {
            var dbEntity =  RepositoryDbSet
                .AsNoTracking()
                .FirstOrDefault(e => e.Id.Equals(entity.Id));

            if (dbEntity == null || !((IDomainUserId<TKey>)dbEntity).UserId.Equals(userId)) return null;
            if (ShouldUseUserId(userId) && !((IDomainUserId<TKey>)dbEntity).UserId.Equals(userId)) return null;
        }

        return Mapper.Map(RepositoryDbSet.Update(domainEntity).Entity)!;
    }

    public virtual async Task<TDalEntity?> UpdateAsync(TDalEntity entity, TKey? userId = default!)
    {
        var domainEntity = Mapper.Map(entity)!;

        if (ShouldUseUserId(userId))
        {
            var dbEntity = await RepositoryDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id.Equals(entity.Id));

            if (dbEntity == null || !((IDomainUserId<TKey>)dbEntity).UserId.Equals(userId)) return null;
            if (ShouldUseUserId(userId) && !((IDomainUserId<TKey>)dbEntity).UserId.Equals(userId)) return null;
        }

        return Mapper.Map(RepositoryDbSet.Update(domainEntity).Entity)!;
    }

    
    private bool ShouldUseUserId(TKey? userId = default!)
    {
        return typeof(IDomainUserId<TKey>).IsAssignableFrom(typeof(TDomainEntity)) &&
               userId != null &&
               !EqualityComparer<TKey>.Default.Equals(userId, default);
    }

    public virtual void Remove(TDalEntity entity, TKey? userId = default!)
    {
        Remove(entity.Id, userId);
    }

    public virtual void Remove(TKey id, TKey? userId)
    {
        var query = GetQuery(userId);
        query = query.Where(e => e.Id.Equals(id));
        var dbEntity = query.FirstOrDefault();
        if (dbEntity != null)
        {
            RepositoryDbSet.Remove(dbEntity);
        }
    }

    public virtual async Task RemoveAsync(TKey id, TKey? userId = default!)
    {
        var query = GetQuery(userId);
        query = query.Where(e => e.Id.Equals(id));
        var dbEntity = await query.FirstOrDefaultAsync();
        Console.WriteLine("dbentity: - " + dbEntity);
        if (dbEntity != null)
        {
            RepositoryDbSet.Remove(dbEntity);
        }
    }

    public virtual bool Exists(TKey id, TKey? userId = default)
    {
        var query = GetQuery(userId);
        return query.Any(e => e.Id.Equals(id));
    }

    public virtual async Task<bool> ExistsAsync(TKey id, TKey? userId = default)
    {
        var query = GetQuery(userId);
        return await query.AnyAsync(e => e.Id.Equals(id));
    }
}
