using WebApplication2.models;

namespace WebApplication2.repositories;

public interface IGamesRepository
{
    //add games
    void AddGames(Games games);

    //display games
    List<Games> GetAllGames();

    //get by id
    Games? getGamesById(Guid id);

    
}