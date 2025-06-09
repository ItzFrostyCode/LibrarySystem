using LibrarySystem.Forms;
using LibrarySystem.Services;
using System;
using System.Windows.Forms;

namespace LibrarySystem
{
    public partial class LoginForm : Form
    {
        private readonly AdminService _adminService = new AdminService();

        public LoginForm()
        {
            InitializeComponent();
            txtPassword.UseSystemPasswordChar = true;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var admin = _adminService.Login(username, password);
                if (admin != null)
                {
                    DashboardForm dashboard = new DashboardForm();
                    dashboard.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void pnlUsername_Paint(object sender, PaintEventArgs e)
        {
            DrawPanelBorder((Panel)sender, e, System.Drawing.Color.FromArgb(221, 223, 226));

        }

        private void pnlPassword_Paint(object sender, PaintEventArgs e)
        {
            DrawPanelBorder((Panel)sender, e, System.Drawing.Color.FromArgb(221, 223, 226));

        }
    }
}
