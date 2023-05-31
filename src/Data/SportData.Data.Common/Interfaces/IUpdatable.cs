namespace SportData.Data.Common.Interfaces;

public interface IUpdatable<T>
{
    bool Update(T other);
}