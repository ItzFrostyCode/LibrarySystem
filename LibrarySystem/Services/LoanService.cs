using LibrarySystem.Models;
using LibrarySystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;

internal class LoanService
{
    private readonly LoanRepository _repo = new LoanRepository();
    private readonly BookService _bookService = new BookService();
    private readonly FineService _fineService = new FineService();

    public List<Loan> GetLoans()
    {
        var loans = _repo.GetAllLoans();
        var fines = _fineService.GetFines();

        // Use default fine percent (5%)
        decimal finePercent = 0.05m;

        foreach (var loan in loans)
        {
            // Detect overdue loans and update status
            if (loan.Status == "Borrowed" && loan.DueDate < DateTime.Now && loan.ReturnDate == null)
            {
                loan.Status = "Overdue";
                _repo.UpdateLoan(loan);

                // Create fine if not already exists
                bool fineExists = fines.Any(f => f.LoanID == loan.LoanID && f.Reason.StartsWith("Late return"));
                if (!fineExists)
                {
                    var book = _bookService.GetBooks().FirstOrDefault(b => b.BookID == loan.BookID);
                    if (book != null)
                    {
                        int daysLate = (DateTime.Now.Date - loan.DueDate.Date).Days;
                        decimal fineAmount = book.Price * finePercent * daysLate;
                        var fine = new Fine
                        {
                            LoanID = loan.LoanID,
                            FineAmount = fineAmount,
                            FineDate = DateTime.Now,
                            Reason = $"Late return ({daysLate} day(s) overdue)",
                            IsPaid = "No"
                        };
                        _fineService.AddFine(fine);
                    }
                }
            }
        }

        return loans;
    }

    public void LoanBook(Loan loan)
    {
        _repo.AddLoan(loan);
    }

    public void ReturnLoan(Loan loan)
    {
        var book = _bookService.GetBooks().Find(b => b.BookID == loan.BookID);
        if (book == null) throw new Exception("Book not found.");

        loan.Status = "Returned";
        loan.ReturnDate = DateTime.Now;
        _repo.UpdateLoan(loan);

        // Use default fine percent (5%)
        decimal finePercent = 0.05m;

        if (loan.ReturnDate > loan.DueDate)
        {
            int daysLate = (loan.ReturnDate.Value.Date - loan.DueDate.Date).Days;
            decimal fineAmount = book.Price * finePercent * daysLate;

            // Prevent duplicate late return fines
            var existingFine = _fineService.GetFines()
                .FirstOrDefault(f => f.LoanID == loan.LoanID && f.Reason.StartsWith("Late return"));
            if (existingFine == null)
            {
                var fine = new Fine
                {
                    LoanID = loan.LoanID,
                    FineAmount = fineAmount,
                    FineDate = DateTime.Now,
                    Reason = $"Late return ({daysLate} day(s) overdue)",
                    IsPaid = "No"
                };
                _fineService.AddFine(fine);
            }
        }
    }

    public void MarkLoanLost(Loan loan)
    {
        var book = _bookService.GetBooks().Find(b => b.BookID == loan.BookID);
        if (book == null) throw new Exception("Book not found.");

        if (book.TotalCopies > 0)
        {
            book.TotalCopies -= 1;
            _bookService.UpdateBook(book);
        }

        // Use default lost book multiplier (1.00)
        decimal lostBookMultiplier = 1.00m;

        // Prevent duplicate lost fines
        var existingFine = _fineService.GetFines()
            .FirstOrDefault(f => f.LoanID == loan.LoanID && f.Reason == "Book lost");
        if (existingFine == null)
        {
            var fine = new Fine
            {
                LoanID = loan.LoanID,
                FineAmount = book.Price * lostBookMultiplier,
                FineDate = DateTime.Now,
                Reason = "Book lost",
                IsPaid = "No"
            };
            _fineService.AddFine(fine);
        }

        loan.Status = "Lost";
        loan.ReturnDate = DateTime.Now;
        _repo.UpdateLoan(loan);
    }

    public void WaiveLoanFine(Loan loan)
    {
        // Prevent duplicate waived fines
        var existingFine = _fineService.GetFines()
            .FirstOrDefault(f => f.LoanID == loan.LoanID && f.Reason == "Waived Fine");
        if (existingFine == null)
        {
            var fine = new Fine
            {
                LoanID = loan.LoanID,
                FineAmount = 0,
                FineDate = DateTime.Now,
                Reason = "Waived Fine",
                IsPaid = "Yes"
            };
            _fineService.AddFine(fine);
        }

        loan.Status = "Returned";
        loan.ReturnDate = DateTime.Now;
        _repo.UpdateLoan(loan);
    }
}
