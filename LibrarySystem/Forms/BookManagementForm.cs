using LibrarySystem.Modals;
using LibrarySystem.Models;
using LibrarySystem.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LibrarySystem.Forms
{
    public partial class BookManagementForm : Form
    {
        private readonly BookService _bookService = new BookService();
        private List<Book> _allBooks = new List<Book>();

        public BookManagementForm()
        {
            InitializeComponent();

            // Populate genre ComboBox (DropDownList, select only)
            cmbGenre.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGenre.Items.Clear();
            cmbGenre.Items.Add("All");
            cmbGenre.Items.AddRange(new object[] {
                "Generalities (000)", "Philosophy & Psychology (100)", "Religion (200)", "Social Sciences (300)",
                "Language (400)", "Natural Sciences (500)", "Technology / Programming / IT (600)", "Engineering & Electronics",
                "The Arts (700)", "Literature & Rhetoric (800)", "Geography & History (900)", "Filipiniana", "Reference",
                "Reserve", "Fiction", "Periodicals", "Graduate Study Theses", "Asiana Collection", "Audio-Visual Materials",
                "Negrosiana Collections", "Christian Books", "Japanese Learning Materials", "Vertical Files", "Practical Research",
                "Cooking & Culinary Arts", "Health & Wellness", "Business & Entrepreneurship", "Mathematics",
                "Science Projects & Investigatory", "Computer Science", "Mobile & Web Development", "Education & Teaching"
            });
            cmbGenre.SelectedItem = "All";

            // Wire up filter events
            txtTitle.TextChanged += (s, e) => ApplyBookFilters();
            txtAuthor.TextChanged += (s, e) => ApplyBookFilters();
            txtISBN.TextChanged += (s, e) => ApplyBookFilters();
            cmbGenre.SelectedIndexChanged += (s, e) => ApplyBookFilters();
            txtPublisher.TextChanged += (s, e) => ApplyBookFilters();
            txtPublicationYear.TextChanged += (s, e) => ApplyBookFilters();
            btnClearFilters.Click += btnClearFilters_Click;
            txtPublicationYear.KeyPress += txtPublicationYear_KeyPress;
            txtISBN.KeyPress += txtISBN_KeyPress;

            LoadBooks();
            ApplyBookFilters();

            ApplyCustomButtonStyle(btnDashboard);
            ApplyCustomButtonStyle(btnUserManagement);
            ApplyCustomButtonStyle(btnLoans);
            ApplyCustomButtonStyle(btnFines);
            ApplyCustomButtonStyle(btnLogout);



        }

        private void LoadBooks()
        {
            _allBooks = _bookService.GetBooks();
            ApplyBookFilters();
        }

        private void ApplyBookFilters()
        {
            string title = txtTitle.Text.Trim().ToLower();
            string author = txtAuthor.Text.Trim().ToLower();
            string isbn = txtISBN.Text.Trim();
            string genre = cmbGenre.SelectedItem != null ? cmbGenre.SelectedItem.ToString() : string.Empty;
            string publisher = txtPublisher.Text.Trim().ToLower();
            string publicationYear = txtPublicationYear.Text.Trim();

            var filtered = _allBooks;

            if (!string.IsNullOrEmpty(title))
                filtered = filtered.FindAll(b => b.Title != null && b.Title.ToLower().Contains(title));

            if (!string.IsNullOrEmpty(author))
                filtered = filtered.FindAll(b => b.Author != null && b.Author.ToLower().Contains(author));

            if (!string.IsNullOrEmpty(isbn))
                filtered = filtered.FindAll(b => b.ISBN != null && b.ISBN.Contains(isbn));

            if (!string.IsNullOrEmpty(genre) && genre != "All")
                filtered = filtered.FindAll(b => b.Genre != null && b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(publisher))
                filtered = filtered.FindAll(b => b.Publisher != null && b.Publisher.ToLower().Contains(publisher));

            if (!string.IsNullOrEmpty(publicationYear))
                filtered = filtered.FindAll(b => b.PublicationYear.ToString().Contains(publicationYear));

            dgvBooks.DataSource = null;
            dgvBooks.Columns.Clear();
            dgvBooks.DataSource = filtered;
            dgvBooks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            if (dgvBooks.Columns.Contains("Title")) dgvBooks.Columns["Title"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            if (dgvBooks.Columns.Contains("Author")) dgvBooks.Columns["Author"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtTitle.Text = "";
            txtAuthor.Text = "";
            txtISBN.Text = "";
            cmbGenre.SelectedItem = "All";
            txtPublisher.Text = "";
            txtPublicationYear.Text = "";
            ApplyBookFilters();
        }

        // Only allow numbers and max 4 digits for publication year
        private void txtPublicationYear_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
            if (txtPublicationYear.Text.Length >= 4 && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        // Only allow numbers for ISBN (optional, remove if ISBN can have dashes/letters)
        private void txtISBN_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
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
        // navigation end

        private void btnAddBook_Click(object sender, EventArgs e)
        {
            using (var modal = new AddEditBookModal())
            {
                if (modal.ShowDialog() == DialogResult.OK && modal.Book != null)
                {
                    try
                    {
                        _bookService.AddBook(modal.Book);
                        LoadBooks();
                        MessageBox.Show("Book added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to add book: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnEditBook_Click(object sender, EventArgs e)
        {
            if (dgvBooks.CurrentRow == null || dgvBooks.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Please select a book to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedBook = dgvBooks.CurrentRow.DataBoundItem as Book;
            if (selectedBook == null)
            {
                MessageBox.Show("Invalid book selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var modal = new AddEditBookModal(selectedBook))
            {
                if (modal.ShowDialog() == DialogResult.OK && modal.Book != null)
                {
                    try
                    {
                        _bookService.UpdateBook(modal.Book);
                        LoadBooks();
                        MessageBox.Show("Book updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to update book: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDeleteBook_Click(object sender, EventArgs e)
        {
            if (dgvBooks.CurrentRow == null || dgvBooks.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Please select a book to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedBook = dgvBooks.CurrentRow.DataBoundItem as Book;
            if (selectedBook == null)
            {
                MessageBox.Show("Invalid book selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to delete '{selectedBook.Title}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    _bookService.DeleteBook(selectedBook.BookID);
                    LoadBooks();
                    MessageBox.Show("Book deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete book: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
