using WebApplication2.data;
using WebApplication2.models;
using Microsoft.EntityFrameworkCore;


namespace WebApplication2.repositories.repository
{

    public class GamesRepository : IGamesRepository
    {

        private readonly ApplicationDbContext _context;

        public GamesRepository(ApplicationDbContext context)
        {

            _context = context;

        }

        public  void AddGames(Games games) { 
        
            _context.Games.Add(games);
            _context.SaveChanges();
        }

    }

}