using LibrarySystem.Modals;
using LibrarySystem.Models;
using LibrarySystem.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LibrarySystem.Forms
{
    public partial class UserManagementForm : Form
    {
        private readonly UserService _userService = new UserService();
        private List<User> _allUsers = new List<User>();

        public UserManagementForm()
        {
            InitializeComponent();

            // Initialize cmbUserType with options
            cmbUserType.Items.Clear();
            cmbUserType.Items.Add("All");
            cmbUserType.Items.Add("Student");
            cmbUserType.Items.Add("Faculty");
            cmbUserType.Items.Add("Others");
            cmbUserType.SelectedIndex = 0; // Default to "All"

            LoadUsers();

            txtFullName.TextChanged += (s, e) => ApplyUserFilters();
            txtEmail.TextChanged += (s, e) => ApplyUserFilters();
            txtContactNumber.TextChanged += (s, e) => ApplyUserFilters();
            cmbUserType.SelectedIndexChanged += (s, e) => ApplyUserFilters();
            dtpFrom.ValueChanged += (s, e) => ApplyUserFilters();
            dtpTo.ValueChanged += (s, e) => ApplyUserFilters();

            ApplyUserFilters(); // Always call the filter after loading users

            ApplyCustomButtonStyle(btnDashboard);
            ApplyCustomButtonStyle(btnBookManagement);
            ApplyCustomButtonStyle(btnLoans);
            ApplyCustomButtonStyle(btnFines);
            ApplyCustomButtonStyle(btnLogout);

            // Set DataGridView header style for dark background and white text

        }
        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtFullName.Text = "";
            txtEmail.Text = "";
            txtContactNumber.Text = "";
            cmbUserType.SelectedItem = "All";
            if (_allUsers.Count > 0)
            {
                dtpFrom.Value = _allUsers.Min(u => u.RegisteredDate);
                dtpTo.Value = _allUsers.Max(u => u.RegisteredDate);
            }
            else
            {
                dtpFrom.Value = DateTime.Now.AddYears(-1);
                dtpTo.Value = DateTime.Now;
            }
            ApplyUserFilters();
        }

        private void LoadUsers()
        {
            _allUsers = _userService.GetUsers();

            // Set sensible defaults for date pickers
            if (_allUsers.Count > 0)
            {
                dtpFrom.Value = _allUsers.Min(u => u.RegisteredDate);
                dtpTo.Value = _allUsers.Max(u => u.RegisteredDate);
            }
            else
            {
                dtpFrom.Value = DateTime.Now.AddYears(-1);
                dtpTo.Value = DateTime.Now;
            }

            ApplyUserFilters();
        }

        private void ApplyUserFilters()
        {
            string fullName = txtFullName.Text.Trim().ToLower();
            string email = txtEmail.Text.Trim().ToLower();
            string contactNumber = txtContactNumber.Text.Trim().ToLower();
            string userType = cmbUserType.SelectedItem != null ? cmbUserType.SelectedItem.ToString() : string.Empty;
            DateTime fromDate = dtpFrom.Value.Date;
            DateTime toDate = dtpTo.Value.Date;

            var filtered = _allUsers;

            if (!string.IsNullOrEmpty(fullName))
                filtered = filtered.FindAll(u => u.FullName != null && u.FullName.ToLower().Contains(fullName));

            if (!string.IsNullOrEmpty(email))
                filtered = filtered.FindAll(u => u.Email != null && u.Email.ToLower().Contains(email));

            if (!string.IsNullOrEmpty(contactNumber))
                filtered = filtered.FindAll(u => u.ContactNumber != null && u.ContactNumber.ToLower().Contains(contactNumber));

            if (!string.IsNullOrEmpty(userType) && userType != "All")
                filtered = filtered.FindAll(u => u.UserType != null && u.UserType.Equals(userType, StringComparison.OrdinalIgnoreCase));

            filtered = filtered.FindAll(u => u.RegisteredDate.Date >= fromDate && u.RegisteredDate.Date <= toDate);

            dgvUsers.DataSource = null;
            dgvUsers.Columns.Clear();
            dgvUsers.DataSource = filtered;
            dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUsers.Columns["FullName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvUsers.Columns["Email"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvUsers.Columns["UserID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        // Sidebar navigation handlers...
        private void btnDashboard_Click(object sender, EventArgs e)
        {
            var dashboardForm = new DashboardForm();
            dashboardForm.Show();
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

        // Center actions

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            using (var addEditUserModal = new AddEditUserModal())
            {
                if (addEditUserModal.ShowDialog() == DialogResult.OK && addEditUserModal.User != null)
                {
                    try
                    {
                        _userService.AddUser(addEditUserModal.User);
                        LoadUsers();
                        MessageBox.Show("User added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to add user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnEditUser_Click(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow == null || dgvUsers.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Please select a user to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedUser = dgvUsers.CurrentRow.DataBoundItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Invalid user selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var modal = new AddEditUserModal(selectedUser))
            {
                if (modal.ShowDialog() == DialogResult.OK && modal.User != null)
                {
                    try
                    {
                        _userService.UpdateUser(modal.User);
                        LoadUsers();
                        MessageBox.Show("User updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to update user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDeleteUser_Click(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow == null || dgvUsers.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Please select a user to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedUser = dgvUsers.CurrentRow.DataBoundItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Invalid user selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to delete user '{selectedUser.FullName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    _userService.DeleteUser(selectedUser.UserID);
                    LoadUsers();
                    MessageBox.Show("User deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvUsers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvUsers.Rows[e.RowIndex].DataBoundItem != null)
            {
                btnEditUser_Click(sender, e);
            }
        }

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
