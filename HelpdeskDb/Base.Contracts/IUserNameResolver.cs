namespace Base.Contracts;

public interface IUserNameResolver
{
    string CurrentUserName { get; }
}