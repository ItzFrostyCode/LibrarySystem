using LibrarySystem.DataAccess;
using LibrarySystem.Models;

namespace LibrarySystem.Services
{
    public class AdminService
    {
        private readonly AdminRepository _repo = new AdminRepository();

        public AdminAccount Login(string username, string password)
        {
            return _repo.ValidateLogin(username, password);
        }
    }
}
