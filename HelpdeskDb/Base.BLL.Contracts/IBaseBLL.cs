namespace Base.BLL.Contracts;

public interface IBaseBLL
{
    public Task<int> SaveChangesAsync();
}
