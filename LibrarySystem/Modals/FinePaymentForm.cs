using LibrarySystem.Models;
using LibrarySystem.Services;
using System;
using System.Linq;
using System.Windows.Forms;

namespace LibrarySystem.Modals
{
    public partial class FinePaymentForm : Form
    {
        private Fine _fine;
        private Loan _loan;
        private User _user;
        private Book _book;

        public FinePaymentForm(Fine fine)
        {
            InitializeComponent();
            _fine = fine;

            // Load related data
            var loanService = new LoanService();
            var userService = new UserService();
            var bookService = new BookService();

            _loan = loanService.GetLoans().FirstOrDefault(l => l.LoanID == fine.LoanID);
            _user = _loan != null ? userService.GetUsers().FirstOrDefault(u => u.UserID == _loan.UserID) : null;
            _book = _loan != null ? bookService.GetBooks().FirstOrDefault(b => b.BookID == _loan.BookID) : null;

            // Auto-fill labels
            lblLoanID.Text = _loan != null ? _loan.LoanID.ToString() : "";
            lblFullName.Text = _user != null ? _user.FullName : "";
            lblTitle.Text = _book != null ? _book.Title : "";
            lblFineAmount.Text = fine.FineAmount.ToString("C");

            // Show fine percentage (hardcoded to 5%)
            lblFinePercentage.Text = "5 %";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnConfirmPayment_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to mark this fine as PAID?",
                "Confirm Payment",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _fine.IsPaid = "Yes";
                var fineService = new FineService();
                fineService.UpdateFine(_fine);

                MessageBox.Show("Fine marked as paid.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Set DialogResult and close the form to return to FineManagementForm
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
