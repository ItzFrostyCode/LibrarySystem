using LibrarySystem.Models;
using System;
using System.Windows.Forms;

namespace LibrarySystem.Modals
{
    public partial class AddEditUserModal : Form
    {
        // Expose the user to be added/edited
        public User User { get; private set; }

        // For Add
        public AddEditUserModal()
        {
            InitializeComponent();
            cmbUserType.Items.AddRange(new object[] { "Student", "Faculty", "Others" });
            this.Text = "Add User";
        }

        // For Edit
        public AddEditUserModal(User user) : this()
        {
            this.Text = "Edit User";
            if (user != null)
            {
                txtFullName.Text = user.FullName;
                txtEmail.Text = user.Email;
                txtContactNumber.Text = user.ContactNumber;
                cmbUserType.SelectedItem = user.UserType;
                User = user;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                cmbUserType.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all required fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (User == null)
                User = new User();

            User.FullName = txtFullName.Text.Trim();
            User.Email = txtEmail.Text.Trim();
            User.ContactNumber = txtContactNumber.Text.Trim();
            User.UserType = cmbUserType.SelectedItem.ToString();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }


    }
}
