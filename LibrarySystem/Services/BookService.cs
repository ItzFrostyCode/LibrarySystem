using LibrarySystem.DataAccess;
using LibrarySystem.Models;
using System.Collections.Generic;

namespace LibrarySystem.Services
{
    public class BookService
    {
        private readonly BookRepository _repo = new BookRepository();

        public List<Book> GetBooks() => _repo.GetAllBooks();
        public void AddBook(Book book) => _repo.AddBook(book);
        public void UpdateBook(Book book) => _repo.UpdateBook(book);
        public void DeleteBook(int bookId) => _repo.DeleteBook(bookId);
    }
}
