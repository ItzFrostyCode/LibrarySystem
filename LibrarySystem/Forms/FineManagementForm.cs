using LibrarySystem.Modals;
using LibrarySystem.Models;
using LibrarySystem.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LibrarySystem.Forms
{
    public partial class FineManagementForm : Form
    {
        private readonly FineService _fineService = new FineService();
        private readonly LoanService _loanService = new LoanService();
        private readonly BookService _bookService = new BookService();
        private readonly UserService _userService = new UserService();

        private Fine selectedFine = null;
        private User selectedUser = null;

        public FineManagementForm()
        {
            InitializeComponent();

            // Set DataGridView selection mode for clarity
            dgvFines.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFines.MultiSelect = false;

            // Populate isPaid filter
            cmbisPaid.Items.Clear();
            cmbisPaid.Items.Add("All");
            cmbisPaid.Items.Add("Yes");
            cmbisPaid.Items.Add("No");
            cmbisPaid.SelectedIndex = 0;

            // Event handlers
            lstUsersFineaBook.SelectedIndexChanged += lstUsersFineaBook_SelectedIndexChanged;
            cmbisPaid.SelectedIndexChanged += (s, e) => LoadFines();

            dgvFines.SelectionChanged += dgvFines_SelectionChanged;
            btnPayFine.Click += btnPayFine_Click;
            btnPayFine.Enabled = false;

            LoadUsersWithFines();

            ApplyCustomButtonStyle(btnDashboard);
            ApplyCustomButtonStyle(btnUserManagement);
            ApplyCustomButtonStyle(btnBookManagement);
            ApplyCustomButtonStyle(btnLoans);
            ApplyCustomButtonStyle(btnLogout);

            this.dgvFines.RowPostPaint += new DataGridViewRowPostPaintEventHandler(this.dgvFines_RowPostPaint);

        }

        private void dgvFines_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
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

        private void LoadUsersWithFines()
        {
            lstUsersFineaBook.Items.Clear();
            var fines = _fineService.GetFines();
            var loans = _loanService.GetLoans();
            var users = _userService.GetUsers();

            // Users with at least one fine with reason LateReturn, Lost, Overdue
            var userIds = fines
                .Where(f => f.Reason != null &&
                    (f.Reason.StartsWith("Late return", StringComparison.OrdinalIgnoreCase) ||
                     f.Reason.Equals("Book lost", StringComparison.OrdinalIgnoreCase) ||
                     f.Reason.Equals("Overdue", StringComparison.OrdinalIgnoreCase)))
                .Select(f => loans.FirstOrDefault(l => l.LoanID == f.LoanID)?.UserID ?? -1)
                .Where(uid => uid != -1)
                .Distinct()
                .ToList();

            var usersWithFines = users.Where(u => userIds.Contains(u.UserID)).ToList();

            // Add "ALL" option
            lstUsersFineaBook.Items.Add(new UserListBoxItem
            {
                User = null,
                Display = "ALL"
            });

            foreach (var user in usersWithFines)
            {
                lstUsersFineaBook.Items.Add(new UserListBoxItem
                {
                    User = user,
                    Display = $"{user.FullName} (UserID:{user.UserID})"
                });
            }

            if (lstUsersFineaBook.Items.Count > 0)
                lstUsersFineaBook.SelectedIndex = 0;
        }

        private void lstUsersFineaBook_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = lstUsersFineaBook.SelectedItem as UserListBoxItem;
            selectedUser = selectedItem?.User;
            DisplaySelectedUser();
            LoadFines();
        }

        private void DisplaySelectedUser()
        {
            if (selectedUser == null)
            {
                lblUserID.Text = "";
                lblFullName.Text = "";
                lblUserType.Text = "";
                lblEmail.Text = "";
                lblContactNumber.Text = "";
                // Show total unpaid fines for all users
                var fines = _fineService.GetFines();
                decimal totalUnpaid = fines
                    .Where(f => f.Reason != null &&
                        (f.Reason.StartsWith("Late return", StringComparison.OrdinalIgnoreCase) ||
                         f.Reason.Equals("Book lost", StringComparison.OrdinalIgnoreCase) ||
                         f.Reason.Equals("Overdue", StringComparison.OrdinalIgnoreCase)))
                    .Where(f => f.IsPaid == "No")
                    .Sum(f => f.FineAmount);
                lblTotalUnpaidFines.Text = $"₱{totalUnpaid:0.00}";
                dgvFines.DataSource = null;
                return;
            }
            lblUserID.Text = selectedUser.UserID.ToString();
            lblFullName.Text = selectedUser.FullName;
            lblUserType.Text = selectedUser.UserType;
            lblEmail.Text = selectedUser.Email;
            lblContactNumber.Text = selectedUser.ContactNumber;
        }

        private void LoadFines()
        {
            var fines = _fineService.GetFines();
            var loans = _loanService.GetLoans();
            var books = _bookService.GetBooks();

            if (selectedUser != null)
            {
                // Only fines for the selected user
                var userLoanIds = loans.Where(l => l.UserID == selectedUser.UserID).Select(l => l.LoanID).ToList();
                fines = fines.Where(f => userLoanIds.Contains(f.LoanID)).ToList();
            }
            else
            {
                // Only show fines for users with fines (same filter as before)
                var userIds = fines
                    .Where(f => f.Reason != null &&
                        (f.Reason.StartsWith("Late return", StringComparison.OrdinalIgnoreCase) ||
                         f.Reason.Equals("Book lost", StringComparison.OrdinalIgnoreCase) ||
                         f.Reason.Equals("Overdue", StringComparison.OrdinalIgnoreCase)))
                    .Select(f => loans.FirstOrDefault(l => l.LoanID == f.LoanID)?.UserID ?? -1)
                    .Where(uid => uid != -1)
                    .Distinct()
                    .ToList();
                var userLoanIds = loans.Where(l => userIds.Contains(l.UserID)).Select(l => l.LoanID).ToList();
                fines = fines.Where(f => userLoanIds.Contains(f.LoanID)).ToList();
            }

            // Only show fines with reason LateReturn, Lost, Overdue
            fines = fines.Where(f =>
                f.Reason != null &&
                (f.Reason.StartsWith("Late return", StringComparison.OrdinalIgnoreCase) ||
                 f.Reason.Equals("Book lost", StringComparison.OrdinalIgnoreCase) ||
                 f.Reason.Equals("Overdue", StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // Filter by isPaid
            string isPaidFilter = cmbisPaid.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(isPaidFilter) && isPaidFilter != "All")
            {
                fines = fines.Where(f => f.IsPaid == isPaidFilter).ToList();
            }

            // Calculate total unpaid fines
            decimal totalUnpaid = fines.Where(f => f.IsPaid == "No").Sum(f => f.FineAmount);
            lblTotalUnpaidFines.Text = $"₱{totalUnpaid:0.00}";

            // Project to display format
            var fineList = fines.Select(f =>
            {
                var loan = loans.FirstOrDefault(l => l.LoanID == f.LoanID);
                var book = loan != null ? books.FirstOrDefault(b => b.BookID == loan.BookID) : null;
                string reason = f.Reason;
                if (reason != null)
                {
                    if (reason.StartsWith("Late return", StringComparison.OrdinalIgnoreCase))
                        reason = "LateReturn";
                    else if (reason.Equals("Book lost", StringComparison.OrdinalIgnoreCase))
                        reason = "Lost";
                    else if (reason.Equals("Overdue", StringComparison.OrdinalIgnoreCase))
                        reason = "Overdue";
                }
                return new
                {
                    f.FineID,
                    f.LoanID,
                    BookTitle = book?.Title ?? "Unknown",
                    FineAmount = f.FineAmount.ToString("C"),
                    f.FineDate,
                    Reason = reason,
                    f.IsPaid
                };
            }).ToList();

            dgvFines.DataSource = null;
            dgvFines.Columns.Clear();
            dgvFines.DataSource = fineList;
            dgvFines.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Select first row if available and update button state
            selectedFine = null;
            btnPayFine.Enabled = false;
            if (dgvFines.Rows.Count > 0)
            {
                dgvFines.ClearSelection();
                dgvFines.Rows[0].Selected = true;
                // Manually trigger selection changed to update btnPayFine state
                dgvFines_SelectionChanged(dgvFines, EventArgs.Empty);
            }
        }

        private void dgvFines_SelectionChanged(object sender, EventArgs e)
        {
            selectedFine = null;
            btnPayFine.Enabled = false;

            if (dgvFines.SelectedRows.Count > 0)
            {
                var fineIdObj = dgvFines.SelectedRows[0].Cells["FineID"].Value;
                if (fineIdObj != null)
                {
                    int fineId = Convert.ToInt32(fineIdObj);
                    var fines = _fineService.GetFines();
                    selectedFine = fines.FirstOrDefault(f => f.FineID == fineId);
                    if (selectedFine != null && selectedFine.IsPaid == "No")
                        btnPayFine.Enabled = true;
                }
            }
        }

        private void btnPayFine_Click(object sender, EventArgs e)
        {
            if (selectedFine == null)
            {
                MessageBox.Show("Please select a fine to pay.");
                return;
            }
            using (var finePaymentForm = new FinePaymentForm(selectedFine))
            {
                if (finePaymentForm.ShowDialog() == DialogResult.OK)
                {
                    LoadFines();
                }
            }
        }

        private class UserListBoxItem
        {
            public User User { get; set; }
            public string Display { get; set; }
            public override string ToString() => Display;
        }

        // Navigation Start
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
        private void btnLoans_Click(object sender, EventArgs e)
        {
            var loansDashboardForm = new LoansDashboardForm();
            loansDashboardForm.Show();
            this.Close();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            var loginForm = new LoginForm();
            loginForm.Show();
            this.Close();
        }
        // Navigation End


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
