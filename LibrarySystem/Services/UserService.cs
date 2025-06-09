using LibrarySystem.DataAccess;
using LibrarySystem.Models;
using System.Collections.Generic;

namespace LibrarySystem.Services
{
    public class UserService
    {
        private readonly UserRepository _repo = new UserRepository();
        public List<User> GetUsers()
        {
            return _repo.GetAllUsers();
        }

        public void AddUser(User user)
        {
            _repo.AddUser(user);
        }

        public void UpdateUser(User user)
        {
            _repo.UpdateUser(user);
        }

        public void DeleteUser(int userId)
        {
            _repo.DeleteUser(userId);
        }

    }
}
