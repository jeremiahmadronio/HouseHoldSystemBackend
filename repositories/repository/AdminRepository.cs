using WebApplication2.data;
using WebApplication2.models;
using Microsoft.EntityFrameworkCore;


namespace WebApplication2.repositories.repository
{

    public class AdminRepository : IAdminRepository {

        private readonly ApplicationDbContext _context;

        public AdminRepository(ApplicationDbContext context) {

            _context = context;

        }

        public Admin? GetUserByEmail(string email) {
        
            return _context.Admins.FirstOrDefault(a => a.email == email);
        }

    }

}