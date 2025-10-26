using WebApplication2.models;

namespace WebApplication2.repositories;
public interface IDietaryTagRepository
{
    Task<IEnumerable<DietaryTag>> getAllAsync();

}