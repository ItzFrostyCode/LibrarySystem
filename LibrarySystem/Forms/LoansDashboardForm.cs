using LibrarySystem.Models;
using LibrarySystem.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LibrarySystem.Forms
{
    public partial class LoansDashboardForm : Form
    {
        private readonly LoanService _loanService = new LoanService();
        private readonly BookService _bookService = new BookService();
        private readonly UserService _userService = new UserService();

        public LoansDashboardForm()
        {
            InitializeComponent();

            // Populate status combo box
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("All");
            cmbStatus.Items.Add("Borrowed");
            cmbStatus.Items.Add("Returned");
            cmbStatus.Items.Add("Waived");
            cmbStatus.Items.Add("Overdue");
            cmbStatus.SelectedIndex = 0;

            // Event handlers
            lstUsersLoanedaBook.SelectedIndexChanged += lstUsersLoanedaBook_SelectedIndexChanged;
            txtTitle.TextChanged += FilterLoans;
            cmbStatus.SelectedIndexChanged += FilterLoans;
            dtpFrom.ValueChanged += FilterLoans;
            dtpTo.ValueChanged += FilterLoans;
            btnClearFilters.Click += btnClearFilters_Click;
            btnLoanBook.Click += btnLoanBook_Click;
            btnReturnBook.Click += btnReturnBook_Click;
            dgvLoans.SelectionChanged += (s, e) => UpdateReturnBookButtonState();

            LoadUsersWithLoans();

            ApplyCustomButtonStyle(btnDashboard);
            ApplyCustomButtonStyle(btnUserManagement);
            ApplyCustomButtonStyle(btnBookManagement);
            ApplyCustomButtonStyle(btnFines);
            ApplyCustomButtonStyle(btnLogout);

            this.dgvLoans.RowPostPaint += new DataGridViewRowPostPaintEventHandler(this.dgvLoans_RowPostPaint);
            dgvLoans.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void UpdateReturnBookButtonState()
        {
            btnReturnBook.Enabled = false;

            if (dgvLoans.SelectedRows.Count == 0)
                return;

            var statusCell = dgvLoans.SelectedRows[0].Cells["Status"];
            if (statusCell == null || statusCell.Value == null)
                return;

            string status = statusCell.Value.ToString();
            btnReturnBook.Enabled = status == "Borrow" || status == "Overdue";
        }


        private void dgvLoans_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            // Draw the row number in the row header
            Rectangle headerBounds = new Rectangle(
                e.RowBounds.Left, e.RowBounds.Top,
                grid.RowHeadersWidth, e.RowBounds.Height);

            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        // Load users who have at least one loan, plus "ALL"
        private void LoadUsersWithLoans()
        {
            lstUsersLoanedaBook.Items.Clear();
            var loans = _loanService.GetLoans();
            var users = _userService.GetUsers();

            var userIdsWithLoans = loans.Select(l => l.UserID).Distinct().ToList();
            var usersWithLoans = users.Where(u => userIdsWithLoans.Contains(u.UserID)).ToList();

            // Add "ALL" option
            lstUsersLoanedaBook.Items.Add(new UserListBoxItem
            {
                User = null,
                Display = "ALL"
            });

            foreach (var user in usersWithLoans)
            {
                lstUsersLoanedaBook.Items.Add(new UserListBoxItem
                {
                    User = user,
                    Display = $"{user.FullName} (User ID: {user.UserID})"
                });
            }

            if (lstUsersLoanedaBook.Items.Count > 0)
                lstUsersLoanedaBook.SelectedIndex = 0;
            else
                ClearUserDisplay();
        }

        private void lstUsersLoanedaBook_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplaySelectedUser();
            SetDatePickersToUserLoans();
            FilterLoans(null, null);
            UpdateReturnBookButtonState();
        }

        private void DisplaySelectedUser()
        {
            var selectedItem = lstUsersLoanedaBook.SelectedItem as UserListBoxItem;
            if (selectedItem == null || selectedItem.User == null)
            {
                ClearUserDisplay();
                return;
            }
            var user = selectedItem.User;
            lblUserID.Text = user.UserID.ToString();
            lblFullName.Text = user.FullName;
            lblUserType.Text = user.UserType;
            lblEmail.Text = user.Email;
            lblContactNumber.Text = user.ContactNumber;
        }

        private void ClearUserDisplay()
        {
            lblUserID.Text = "";
            lblFullName.Text = "";
            lblUserType.Text = "";
            lblEmail.Text = "";
            lblContactNumber.Text = "";
            dgvLoans.DataSource = null;
        }

        private void SetDatePickersToUserLoans()
        {
            var selectedItem = lstUsersLoanedaBook.SelectedItem as UserListBoxItem;
            var loans = _loanService.GetLoans()
                .Where(l => l.Status == "Borrowed" || l.Status == "Returned" || l.Status == "Waived" || l.Status == "Overdue")
                .ToList();

            if (selectedItem != null && selectedItem.User != null)
            {
                loans = loans.Where(l => l.UserID == selectedItem.User.UserID).ToList();
            }

            if (loans.Count > 0)
            {
                dtpFrom.Value = loans.Min(l => l.LoanDate);
                dtpTo.Value = loans.Max(l => l.LoanDate);
            }
            else
            {
                dtpFrom.Value = DateTime.Now.AddYears(-1);
                dtpTo.Value = DateTime.Now;
            }
        }

        private void FilterLoans(object sender, EventArgs e)
        {
            var selectedItem = lstUsersLoanedaBook.SelectedItem as UserListBoxItem;
            var loans = _loanService.GetLoans()
                .Where(l => l.Status == "Borrowed" || l.Status == "Returned" || l.Status == "Waived" || l.Status == "Overdue")
                .ToList();

            if (selectedItem != null && selectedItem.User != null)
            {
                loans = loans.Where(l => l.UserID == selectedItem.User.UserID).ToList();
            }

            var books = _bookService.GetBooks();

            // Filter by title
            string titleFilter = txtTitle.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(titleFilter))
            {
                loans = loans.Where(l =>
                    books.FirstOrDefault(b => b.BookID == l.BookID)?.Title.ToLower().Contains(titleFilter) == true
                ).ToList();
            }

            // Filter by status
            string selectedStatus = cmbStatus.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "All")
            {
                string statusFilter = selectedStatus == "Return" ? "Returned" : selectedStatus;
                loans = loans.Where(l => l.Status == statusFilter).ToList();
            }

            // Filter by date range
            DateTime from = dtpFrom.Value.Date;
            DateTime to = dtpTo.Value.Date;
            loans = loans.Where(l => l.LoanDate.Date >= from && l.LoanDate.Date <= to).ToList();

            var loanDisplayList = loans.Select(l => new
            {
                l.LoanID,
                Title = books.FirstOrDefault(b => b.BookID == l.BookID)?.Title ?? "Unknown",
                l.LoanDate,
                l.DueDate,
                l.ReturnDate,
                Status = l.Status == "Borrowed" ? "Borrow" :
                         l.Status == "Returned" ? "Return" :
                         l.Status == "Waived" ? "Waive" :
                         l.Status == "Overdue" ? "Overdue" : l.Status
            }).ToList();

            dgvLoans.DataSource = loanDisplayList;
            dgvLoans.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Auto-select the first row if available
            if (dgvLoans.Rows.Count > 0)
            {
                dgvLoans.ClearSelection();
                dgvLoans.Rows[0].Selected = true;
            }
            UpdateReturnBookButtonState();
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtTitle.Text = "";
            cmbStatus.SelectedIndex = 0;
            SetDatePickersToUserLoans();
            FilterLoans(null, null);
        }

        private void btnLoanBook_Click(object sender, EventArgs e)
        {
            using (var loanBookForm = new LoanBookForm())
            {
                if (loanBookForm.ShowDialog() == DialogResult.OK)
                {
                    // Get the latest loan
                    var loans = _loanService.GetLoans();
                    var lastLoan = loans.OrderByDescending(l => l.LoanID).FirstOrDefault();
                    if (lastLoan != null)
                    {
                        var user = _userService.GetUsers().FirstOrDefault(u => u.UserID == lastLoan.UserID);
                        if (user != null)
                        {
                            // Add user to list if not already present
                            bool exists = false;
                            foreach (UserListBoxItem item in lstUsersLoanedaBook.Items)
                            {
                                if (item.User != null && item.User.UserID == user.UserID)
                                {
                                    exists = true;
                                    break;
                                }
                            }
                            if (!exists)
                            {
                                lstUsersLoanedaBook.Items.Add(new UserListBoxItem
                                {
                                    User = user,
                                    Display = $"{user.FullName} (User ID: {user.UserID})"
                                });
                            }

                            // Select the user and load their loans
                            for (int i = 0; i < lstUsersLoanedaBook.Items.Count; i++)
                            {
                                var item = lstUsersLoanedaBook.Items[i] as UserListBoxItem;
                                if (item != null && item.User != null && item.User.UserID == user.UserID)
                                {
                                    lstUsersLoanedaBook.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnReturnBook_Click(object sender, EventArgs e)
        {
            if (!btnReturnBook.Enabled)
                return;

            if (dgvLoans.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a loan to return.");
                return;
            }

            var selectedRow = dgvLoans.SelectedRows[0];
            int loanId = Convert.ToInt32(selectedRow.Cells["LoanID"].Value);
            var loans = _loanService.GetLoans();
            var loan = loans.FirstOrDefault(l => l.LoanID == loanId);
            if (loan == null)
            {
                MessageBox.Show("Loan not found.");
                return;
            }

            var user = _userService.GetUsers().FirstOrDefault(u => u.UserID == loan.UserID);
            var book = _bookService.GetBooks().FirstOrDefault(b => b.BookID == loan.BookID);

            if (user == null || book == null)
            {
                MessageBox.Show("User or Book not found.");
                return;
            }

            using (var returnBookForm = new ReturnBookForm(user.UserID, user.FullName, book.BookID))
            {
                if (returnBookForm.ShowDialog() == DialogResult.OK)
                {
                    FilterLoans(null, null);
                }
            }
        }

        private class UserListBoxItem
        {
            public User User { get; set; }
            public string Display { get; set; }
            public override string ToString() => Display;
        }

        // navigation start
        private void btnDashboard_Click(object sender, EventArgs e)
        {
            var dashboardForm = new DashboardForm();
            dashboardForm.Show();
            this.Close();
        }

        private void btnUserManagement_Click(object sender, EventArgs e)
        {
            var userManagementForm = new UserManagementForm();
            userManagementForm.Show();
            this.Close();
        }

        private void btnBookManagement_Click(object sender, EventArgs e)
        {
            var bookManagementForm = new BookManagementForm();
            bookManagementForm.Show();
            this.Close();
        }

        private void btnFines_Click(object sender, EventArgs e)
        {
            var fineManagementForm = new FineManagementForm();
            fineManagementForm.Show();
            this.Close();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            var loginForm = new LoginForm();
            loginForm.Show();
            this.Close();
        }
        // navigation end

        // Apply custom style to buttons Start
        private void ApplyCustomButtonStyle(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 230, 230);
            btn.BackColor = Color.White;
            btn.ForeColor = Color.Black;
            btn.UseVisualStyleBackColor = false;
            btn.Paint -= CustomButton_Paint;
            btn.Paint += CustomButton_Paint;
        }

        private void CustomButton_Paint(object sender, PaintEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw drop shadow (rectangle, not rounded)
            using (var shadowBrush = new SolidBrush(Color.FromArgb(25, 0, 0, 0)))
            {
                var shadowRect = new RectangleF(
                    0f,
                    0.89f, // y offset
                    btn.Width,
                    btn.Height
                );
                e.Graphics.FillRectangle(shadowBrush, shadowRect);
            }

            // Draw button background (rectangle, not rounded)
            using (var backBrush = new SolidBrush(btn.BackColor))
            {
                var rect = new RectangleF(0, 0, btn.Width, btn.Height);
                e.Graphics.FillRectangle(backBrush, rect);
            }

            int imageWidth = 0;
            int imageHeight = 0;
            int imagePadding = 8; // space between image and text

            // Draw button image (if any) on the left, vertically centered
            if (btn.Image != null)
            {
                imageWidth = btn.Image.Width;
                imageHeight = btn.Image.Height;
                int imageX = 12; // left padding
                int imageY = (btn.Height - imageHeight) / 2;
                e.Graphics.DrawImage(btn.Image, imageX, imageY, imageWidth, imageHeight);
            }

            // Draw button text to the right of the image, vertically centered
            Rectangle textRect = btn.ClientRectangle;
            textRect.X += imageWidth + 12 + imagePadding; // image width + left padding + space
            textRect.Width -= (imageWidth + 12 + imagePadding);

            TextRenderer.DrawText(
                e.Graphics,
                btn.Text,
                btn.Font,
                textRect,
                btn.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left
            );
        }
        // Apply custom style to buttons End
    }
}
