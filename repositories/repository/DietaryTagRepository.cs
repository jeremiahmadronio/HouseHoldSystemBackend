using WebApplication2.data;
using WebApplication2.models;
using Microsoft.EntityFrameworkCore;
namespace WebApplication2.repositories.repository
{
    public class DietaryTagRepository : IDietaryTagRepository
    {
        private readonly ApplicationDbContext _context;
        public DietaryTagRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<DietaryTag>> getAllAsync() => await _context.DietaryTags.ToListAsync();

    }
}