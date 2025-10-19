using WebApplication2.models;

namespace WebApplication2.repositories;

   public  interface IUserRepository
    {
    Task<IEnumerable<User>> getAllAsync();

    bool EmailExists(string email);

    User? GetUserByEmail(string email);

    void AddUser(User user);

    void UpdateUser(User user);



    User? GetUserByUsername(string username);
    void DeleteUser(User user); 



}

