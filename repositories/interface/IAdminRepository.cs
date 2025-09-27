using WebApplication2.models;

namespace WebApplication2.repositories;

public interface IAdminRepository {

    Admin? GetUserByEmail(string email);
}