using WebApplication2.data;
using WebApplication2.models;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Mozilla;


namespace WebApplication2.repositories.repository
{

    public class GamesRepository : IGamesRepository
    {

        private readonly ApplicationDbContext _context;

        public GamesRepository(ApplicationDbContext context)
        {

            _context = context;

        }

        //add games
        public  void AddGames(Games games) { 
        
            _context.Games.Add(games);
            _context.SaveChanges();
        }


        //Display Games
        public List<Games> GetAllGames() { 
        
            return _context.Games.ToList();

        }

        //get game by id
        public Games? getGamesById(Guid id) {

            return _context.Games.FirstOrDefault(g => g.id == id);
        
        }

        public int? GetTotalGames() {
            return _context.Games.Count();
                }

        public void UpdateGame(Games game)
        {
            _context.Games.Update(game);
            _context.SaveChanges();
        }

        public void DeleteGames(Guid id)
        {
            var game = _context.Games.FirstOrDefault(g => g.id == id);
            if (game != null)
            {
                _context.Games.Remove(game);
                _context.SaveChanges();
            }
        }



    }

}