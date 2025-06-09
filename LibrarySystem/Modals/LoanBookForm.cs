using LibrarySystem.Models;
using LibrarySystem.Services;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace LibrarySystem.Forms
{
    public partial class LoanBookForm : Form
    {
        private readonly LoanService _loanService = new LoanService();
        private readonly UserService _userService = new UserService();
        private readonly BookService _bookService = new BookService();

        public LoanBookForm()
        {
            InitializeComponent();
            this.Load += LoanBookForm_Load;

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

            this.Close();

        }
        private void LoanBookForm_Load(object sender, EventArgs e)
        {
            // User autocomplete
            var users = _userService.GetUsers();
            var userNames = users.Select(u => u.FullName).ToArray();
            var userAuto = new AutoCompleteStringCollection();
            userAuto.AddRange(userNames);
            txtUser.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtUser.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtUser.AutoCompleteCustomSource = userAuto;

            // Book autocomplete
            var books = _bookService.GetBooks();
            var bookTitles = books.Select(b => b.Title).ToArray();
            var bookAuto = new AutoCompleteStringCollection();
            bookAuto.AddRange(bookTitles);
            txtBooks.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtBooks.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtBooks.AutoCompleteCustomSource = bookAuto;

            lblLoanDate.Text = DateTime.Now.ToShortDateString();
            lblDueDate.Text = DateTime.Now.AddDays(7).ToShortDateString();
        }

        private void btnLoanBook_Click(object sender, EventArgs e)
        {
            // Validate user
            var user = _userService.GetUsers().FirstOrDefault(u => u.FullName.Equals(txtUser.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                MessageBox.Show("Please select a valid user.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (lstSelectedBooks.Items.Count == 0)
            {
                MessageBox.Show("Please select at least one book.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var books = _bookService.GetBooks();
            DateTime loanDate = DateTime.Now;
            DateTime dueDate = loanDate.AddDays(7);

            foreach (string item in lstSelectedBooks.Items)
            {
                var parts = item.Split(',');
                string title = parts[0];

                var book = books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                if (book == null)
                    continue;

                if (book.QuantityAvailable < 1)
                {
                    MessageBox.Show($"No copies available for '{title}'.", "Stock Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                // Create loan record (only one per book)
                var loan = new Loan
                {
                    UserID = user.UserID,
                    BookID = book.BookID,
                    LoanDate = loanDate,
                    DueDate = dueDate,
                    Status = "Borrowed"
                };
                _loanService.LoanBook(loan);

                // Update book quantity
                book.QuantityAvailable -= 1;
                _bookService.UpdateBook(book);
            }

            MessageBox.Show("Books loaned successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }



        private void txtBooks_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(txtBooks.Text))
            {
                string title = txtBooks.Text.Trim();
                var books = _bookService.GetBooks();
                var book = books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

                if (book == null)
                {
                    MessageBox.Show("Please select a valid book from the list.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtBooks.Clear();
                    return;
                }

                // Prevent duplicates
                bool alreadySelected = lstSelectedBooks.Items.Cast<string>().Any(item =>
                    item.Split(',')[0].Equals(title, StringComparison.OrdinalIgnoreCase));
                if (alreadySelected)
                {
                    MessageBox.Show("This book is already selected.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtBooks.Clear();
                    return;
                }

                // Add with quantity 1 (or just the title if you don't want quantity)
                lstSelectedBooks.Items.Add($"{title},1");
                txtBooks.Clear();
            }
        }




    }
}
