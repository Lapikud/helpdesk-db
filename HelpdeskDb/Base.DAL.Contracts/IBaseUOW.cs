namespace Base.DAL.Contracts;

public interface IBaseUOW
{
    public Task<int> SaveChangesAsync();
}