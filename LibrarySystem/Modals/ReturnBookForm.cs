using LibrarySystem.Models;
using LibrarySystem.Services;
using System;
using System.Linq;
using System.Windows.Forms;

namespace LibrarySystem.Forms
{
    public partial class ReturnBookForm : Form
    {
        private readonly UserService _userService = new UserService();
        private readonly LoanService _loanService = new LoanService();
        private readonly BookService _bookService = new BookService();
        private readonly FineService _fineService = new FineService();

        public ReturnBookForm(int userId, string fullName, int bookId)
        {
            InitializeComponent();
            txtUserID.Text = userId.ToString();
            lblFullName.Text = fullName;
            txtBooktoReturn.Text = bookId.ToString();
        }

        private void txtUserID_TextChanged(object sender, EventArgs e)
        {
            string userIdText = txtUserID.Text.Trim();
            int userId;
            if (!int.TryParse(userIdText, out userId))
            {
                lblFullName.Text = "";
                lstUserBorrowedBooks.Items.Clear();
                return;
            }

            var user = _userService.GetUsers().FirstOrDefault(u => u.UserID == userId);
            lblFullName.Text = user != null ? user.FullName : "Unknown";

            lstUserBorrowedBooks.Items.Clear();
            var userLoans = _loanService.GetLoans()
                .Where(l => l.UserID == userId && (l.Status == "Borrowed" || l.Status == "Overdue"))
                .ToList();
            var books = _bookService.GetBooks();
            foreach (var loan in userLoans)
            {
                var book = books.FirstOrDefault(b => b.BookID == loan.BookID);
                string display = book != null
                    ? $"ID: {loan.BookID}, Title: {book.Title}"
                    : $"ID: {loan.BookID}, Title: Unknown";
                lstUserBorrowedBooks.Items.Add(display);
            }
        }

        private void txtBooktoReturn_TextChanged(object sender, EventArgs e)
        {
            string userIdText = txtUserID.Text.Trim();
            int userId;
            if (!int.TryParse(userIdText, out userId))
            {
                ClearBookDetails();
                btnWaived.Enabled = false;
                return;
            }

            string bookIdText = txtBooktoReturn.Text.Trim();
            int bookId;
            if (!int.TryParse(bookIdText, out bookId))
            {
                ClearBookDetails();
                btnWaived.Enabled = false;
                return;
            }

            var loan = _loanService.GetLoans()
                .FirstOrDefault(l => l.UserID == userId && l.BookID == bookId && (l.Status == "Borrowed" || l.Status == "Overdue"));

            if (loan == null)
            {
                ClearBookDetails("Not found");
                btnWaived.Enabled = false;
                return;
            }

            var book = _bookService.GetBooks().FirstOrDefault(b => b.BookID == bookId);
            lblBookSelected.Text = book != null ? book.Title : $"BookID: {bookId}";
            lblLoanDate.Text = loan.LoanDate.ToShortDateString();
            lblDueDate.Text = loan.DueDate.ToShortDateString();
            lblReturnDate.Text = loan.ReturnDate.HasValue ? loan.ReturnDate.Value.ToShortDateString() : "Not returned";

            DateTime today = DateTime.Today;
            bool isOverdue = loan.DueDate < today && (loan.Status == "Borrowed" || loan.Status == "Overdue");

            // Use hardcoded fine percent (5%)
            decimal finePercent = 5;

            lblOverdueStatus.Text = isOverdue ? "Overdue" : "On Time";
            lblFinePercentage.Text = "5%";
            decimal fineAmount = 0m;
            if (isOverdue && book != null)
            {
                int daysLate = (today - loan.DueDate.Date).Days;
                fineAmount = book.Price * (finePercent / 100m) * daysLate;
            }
            lblFineAmount.Text = fineAmount > 0 ? fineAmount.ToString("C") : "0.00";

            // Enable Waive button only if overdue
            btnWaived.Enabled = isOverdue;
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtUserID.Text.Trim(), out int userId) ||
                !int.TryParse(txtBooktoReturn.Text.Trim(), out int bookId))
            {
                MessageBox.Show("Please enter valid User ID and Book ID.");
                return;
            }

            var loan = _loanService.GetLoans()
                .FirstOrDefault(l => l.UserID == userId && l.BookID == bookId && (l.Status == "Borrowed" || l.Status == "Overdue"));
            if (loan == null)
            {
                MessageBox.Show("Loan record not found.");
                return;
            }

            var book = _bookService.GetBooks().FirstOrDefault(b => b.BookID == bookId);
            if (book == null)
            {
                MessageBox.Show("Book not found.");
                return;
            }

            DateTime today = DateTime.Today;
            bool isOverdue = loan.DueDate < today;

            // Use hardcoded fine percent (5%)
            decimal finePercent = 5;

            decimal fineAmount = 0m;
            int daysLate = 0;
            if (isOverdue)
            {
                daysLate = (today - loan.DueDate.Date).Days;
                fineAmount = book.Price * (finePercent / 100m) * daysLate;
                var existingFine = _fineService.GetFines()
                    .FirstOrDefault(f => f.LoanID == loan.LoanID && f.Reason.StartsWith("Late return"));
                if (existingFine == null)
                {
                    var fine = new Fine
                    {
                        LoanID = loan.LoanID,
                        FineAmount = fineAmount,
                        FineDate = today,
                        Reason = $"Late return ({daysLate} day(s) overdue)",
                        IsPaid = "No"
                    };
                    _fineService.AddFine(fine);
                }
            }

            loan.ReturnDate = today;
            loan.Status = "Returned";
            _loanService.ReturnLoan(loan);

            book.QuantityAvailable += 1;
            _bookService.UpdateBook(book);

            MessageBox.Show(isOverdue
                ? $"Book returned with a fine of {fineAmount:C}."
                : "Book returned successfully.");

            txtUserID_TextChanged(null, null);
            txtBooktoReturn_TextChanged(null, null);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnLost_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtUserID.Text.Trim(), out int userId) ||
                !int.TryParse(txtBooktoReturn.Text.Trim(), out int bookId))
            {
                MessageBox.Show("Please enter valid User ID and Book ID.");
                return;
            }

            var loan = _loanService.GetLoans()
                .FirstOrDefault(l => l.UserID == userId && l.BookID == bookId && (l.Status == "Borrowed" || l.Status == "Overdue"));
            if (loan == null)
            {
                MessageBox.Show("Loan record not found or already processed.");
                return;
            }

            var book = _bookService.GetBooks().FirstOrDefault(b => b.BookID == bookId);
            if (book == null)
            {
                MessageBox.Show("Book not found.");
                return;
            }

            var existingFine = _fineService.GetFines()
                .FirstOrDefault(f => f.LoanID == loan.LoanID && f.Reason == "Book lost");
            if (existingFine != null)
            {
                MessageBox.Show("This loan is already marked as lost and fined.");
                return;
            }

            decimal fineAmount = book.Price;
            var fine = new Fine
            {
                LoanID = loan.LoanID,
                FineAmount = fineAmount,
                FineDate = DateTime.Today,
                Reason = "Book lost",
                IsPaid = "No"
            };
            _fineService.AddFine(fine);

            loan.Status = "Lost";
            loan.ReturnDate = DateTime.Today;
            _loanService.MarkLoanLost(loan);

            MessageBox.Show($"Book marked as lost. Fine: {fineAmount:C}");

            txtUserID_TextChanged(null, null);
            txtBooktoReturn_TextChanged(null, null);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnWaived_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtUserID.Text.Trim(), out int userId) ||
                !int.TryParse(txtBooktoReturn.Text.Trim(), out int bookId))
            {
                MessageBox.Show("Please enter valid User ID and Book ID.");
                return;
            }

            var loan = _loanService.GetLoans()
                .FirstOrDefault(l => l.UserID == userId && l.BookID == bookId && (l.Status == "Borrowed" || l.Status == "Overdue"));
            if (loan == null)
            {
                MessageBox.Show("Loan record not found or already processed.");
                return;
            }

            var book = _bookService.GetBooks().FirstOrDefault(b => b.BookID == bookId);
            if (book == null)
            {
                MessageBox.Show("Book not found.");
                return;
            }

            // Only allow waiving if overdue
            DateTime today = DateTime.Today;
            if (loan.DueDate >= today)
            {
                MessageBox.Show("This loan is not overdue and cannot be waived.");
                return;
            }

            var existingFine = _fineService.GetFines()
                .FirstOrDefault(f => f.LoanID == loan.LoanID && f.Reason == "Waived Fine");
            if (existingFine != null)
            {
                MessageBox.Show("This loan is already marked as waived.");
                return;
            }

            var fine = new Fine
            {
                LoanID = loan.LoanID,
                FineAmount = 0,
                FineDate = today,
                Reason = "Waived Fine",
                IsPaid = "Yes"
            };
            _fineService.AddFine(fine);

            loan.ReturnDate = today;
            loan.Status = "Waived";
            _loanService.WaiveLoanFine(loan);

            book.QuantityAvailable += 1;
            _bookService.UpdateBook(book);

            MessageBox.Show("Book returned and fine waived.");

            txtUserID_TextChanged(null, null);
            txtBooktoReturn_TextChanged(null, null);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ClearBookDetails(string notFoundText = "")
        {
            lblBookSelected.Text = notFoundText;
            lblLoanDate.Text = "";
            lblDueDate.Text = "";
            lblReturnDate.Text = "";
            lblOverdueStatus.Text = "";
            lblFinePercentage.Text = "";
            lblFineAmount.Text = "";
            btnWaived.Enabled = false;
        }
    }
}
