using WebApplication2.repositories;
using WebApplication2.dto.GamesDTO;

namespace WebApplication2.service;

public interface IGamesService
{
    //create games
    bool createGames(AddGamesDTO dto,out string message);
   

}
