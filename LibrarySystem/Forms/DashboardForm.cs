using LibrarySystem.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LibrarySystem.Forms
{
    public partial class DashboardForm : Form
    {
        private readonly BookService _bookService = new BookService();
        private readonly LoanService _loanService = new LoanService();
        private readonly UserService _userService = new UserService();
        private readonly FineService _fineService = new FineService();

        public DashboardForm()
        {
            InitializeComponent();

            ApplyCustomButtonStyle(btnUserManagement);
            ApplyCustomButtonStyle(btnBookManagement);
            ApplyCustomButtonStyle(btnLoans);
            ApplyCustomButtonStyle(btnFines);
            ApplyCustomButtonStyle(btnLogout);

            LoadDashboardData();

            lblTotalUsers.Text = _userService.GetUsers().Count.ToString();

        }

        private void LoadDashboardData()
        {
            // SYSTEM OVERVIEW
            lblTotalUsers.Text = _userService.GetUsers().Count.ToString();
            lblTotalBooks.Text = _bookService.GetBooks().Count.ToString();
            lblTotalWaived.Text = _loanService.GetLoans().Count(l => l.Status == "Waived").ToString();

            // LOANS SUMMARY
            lblTotalBorrowed.Text = _loanService.GetLoans().Count(l => l.Status == "Borrowed").ToString();
            lblReturn.Text = _loanService.GetLoans().Count(l => l.Status == "Returned").ToString();
            lblTotalBorrowers.Text = _loanService.GetLoans().Select(l => l.UserID).Distinct().Count().ToString();

            // FINES SUMMARY
            var fines = _fineService.GetFines();
            var loans = _loanService.GetLoans();

            // Users with at least one fine (Late return, Lost, Overdue)
            var userIdsWithFines = fines
                .Where(f => f.Reason != null &&
                    (f.Reason.StartsWith("Late return", StringComparison.OrdinalIgnoreCase) ||
                     f.Reason.Equals("Book lost", StringComparison.OrdinalIgnoreCase) ||
                     f.Reason.Equals("Overdue", StringComparison.OrdinalIgnoreCase)))
                .Select(f => loans.FirstOrDefault(l => l.LoanID == f.LoanID)?.UserID ?? -1)
                .Where(uid => uid != -1)
                .Distinct()
                .Count();
            lblUserswithFines.Text = userIdsWithFines.ToString();

            // Late Return Fines
            lblLateReturnFines.Text = fines.Count(f => f.Reason != null && f.Reason.StartsWith("Late return", StringComparison.OrdinalIgnoreCase)).ToString();

            // Lost Book Fines
            lblLostBook.Text = fines.Count(f => f.Reason != null && f.Reason.Equals("Book lost", StringComparison.OrdinalIgnoreCase)).ToString();

            // Total Outstanding Fines (unpaid)
            decimal totalOutstanding = fines
                .Where(f => f.IsPaid == "No" && f.Reason != null &&
                    (f.Reason.StartsWith("Late return", StringComparison.OrdinalIgnoreCase) ||
                     f.Reason.Equals("Book lost", StringComparison.OrdinalIgnoreCase) ||
                     f.Reason.Equals("Overdue", StringComparison.OrdinalIgnoreCase)))
                .Sum(f => f.FineAmount);
            lblTotalOutstandingFines.Text = $"₱{totalOutstanding:0.00}";
        }


        // nav start

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

        // nav end


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


        private void pnlSidebar_Paint(object sender, PaintEventArgs e)
        {

        }

        private void DrawPanelBorder(Panel panel, PaintEventArgs e, System.Drawing.Color color, int thickness = 2)
        {
            using (var pen = new System.Drawing.Pen(color, thickness))
            {
                e.Graphics.DrawRectangle(
                    pen,
                    1,
                    1,
                    panel.Width - thickness,
                    panel.Height - thickness
                );
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            DrawPanelBorder((Panel)sender, e, System.Drawing.Color.FromArgb(221, 223, 226));
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            DrawPanelBorder((Panel)sender, e, System.Drawing.Color.FromArgb(221, 223, 226));
        }

        private void panel9_Paint(object sender, PaintEventArgs e)
        {
            DrawPanelBorder((Panel)sender, e, System.Drawing.Color.FromArgb(221, 223, 226));
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {
            DrawPanelBorder((Panel)sender, e, System.Drawing.Color.FromArgb(221, 223, 226));
        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {
            DrawPanelBorder((Panel)sender, e, System.Drawing.Color.FromArgb(221, 223, 226));
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            DrawPanelBorder((Panel)sender, e, System.Drawing.Color.FromArgb(221, 223, 226));
        }

        private void panel8_Paint(object sender, PaintEventArgs e)
        {
            DrawPanelBorder((Panel)sender, e, System.Drawing.Color.FromArgb(221, 223, 226));
        }

        private void panel7_Paint(object sender, PaintEventArgs e)
        {
            DrawPanelBorder((Panel)sender, e, System.Drawing.Color.FromArgb(221, 223, 226));
        }

        private void panel10_Paint(object sender, PaintEventArgs e)
        {
            DrawPanelBorder((Panel)sender, e, System.Drawing.Color.FromArgb(221, 223, 226));
        }

        private void panel11_Paint(object sender, PaintEventArgs e)
        {
            DrawPanelBorder((Panel)sender, e, System.Drawing.Color.FromArgb(221, 223, 226));
        }
    }
}
