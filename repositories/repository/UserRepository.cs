using WebApplication2.data;
using WebApplication2.models;
using Microsoft.EntityFrameworkCore;


namespace WebApplication2.repositories.repository
{
    public class UserRepository : IUserRepository
    {

        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        //get User
        public async Task<IEnumerable<User>> getAllAsync() => await _context.Users.ToListAsync();


        //verify email
        public bool EmailExists(string email)
        {
            return _context.Users.Any(u => u.email == email);
        }

        //login
        public User? GetUserByEmail(string email) {

            return _context.Users.FirstOrDefault(u => u.email == email);
        }

        //createuser
        public void AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        //resetPass
        public void UpdateUser(User user) { 
        
            _context.Update(user);
            _context.SaveChanges();
        }


        public User? GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.username == username);
        }

        public void DeleteUser(User user)
        {
            if (user == null) return;
            _context.Users.Remove(user);
            _context.SaveChanges();
        }


        public int? GetTotalUser() { 
        
            return _context.Users.Count();
        }


    }
}
