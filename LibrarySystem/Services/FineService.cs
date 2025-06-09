using LibrarySystem.DataAccess;
using LibrarySystem.Models;
using System.Collections.Generic;

namespace LibrarySystem.Services
{
    public class FineService
    {
        private readonly FineRepository _repo = new FineRepository();

        public List<Fine> GetFines()
        {
            return _repo.GetAllFines();
        }

        public void AddFine(Fine fine)
        {
            _repo.AddFine(fine);
        }

        public void UpdateFine(Fine fine)
        {
            _repo.UpdateFine(fine);
        }



    }
}
