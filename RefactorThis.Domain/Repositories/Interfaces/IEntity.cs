namespace RefactorThis.Domain.Repositories.Interfaces;

public interface IEntity<T>
{
    T Id { get; set; }
}