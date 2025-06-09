using LibrarySystem.Models;
using System;
using System.Windows.Forms;

namespace LibrarySystem.Modals
{
    public partial class AddEditBookModal : Form
    {
        public Book Book { get; private set; }

        public AddEditBookModal()
        {
            InitializeComponent();
            // No need to populate years for a TextBox

            // Populate genres
            cmbGenre.Items.Clear();
            string[] genres = {
                "Generalities (000)", "Philosophy & Psychology (100)", "Religion (200)", "Social Sciences (300)",
                "Language (400)", "Natural Sciences (500)", "Technology / Programming / IT (600)", "Engineering & Electronics",
                "The Arts (700)", "Literature & Rhetoric (800)", "Geography & History (900)", "Filipiniana", "Reference",
                "Reserve", "Fiction", "Periodicals", "Graduate Study Theses", "Asiana Collection", "Audio-Visual Materials",
                "Negrosiana Collections", "Christian Books", "Japanese Learning Materials", "Vertical Files", "Practical Research",
                "Cooking & Culinary Arts", "Health & Wellness", "Business & Entrepreneurship", "Mathematics",
                "Science Projects & Investigatory", "Computer Science", "Mobile & Web Development", "Education & Teaching"
            };
            cmbGenre.Items.AddRange(genres);
            this.Text = "Add Book";
        }

        public AddEditBookModal(Book book) : this()
        {
            this.Text = "Edit Book";
            if (book != null)
            {
                txtTitle.Text = book.Title;
                txtAuthor.Text = book.Author;
                txtISBN.Text = book.ISBN;
                txtPublisher.Text = book.Publisher;
                txtPublicationYear.Text = book.PublicationYear.ToString();
                cmbGenre.SelectedItem = book.Genre;
                txtPrice.Text = book.Price.ToString("0.00");
                txtTotalCopies.Text = book.TotalCopies.ToString();
                Book = book;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(txtTitle.Text) ||
                string.IsNullOrWhiteSpace(txtAuthor.Text) ||
                string.IsNullOrWhiteSpace(txtISBN.Text) ||
                string.IsNullOrWhiteSpace(txtPublisher.Text) ||
                string.IsNullOrWhiteSpace(txtPublicationYear.Text) ||
                cmbGenre.SelectedItem == null ||
                string.IsNullOrWhiteSpace(txtPrice.Text) ||
                string.IsNullOrWhiteSpace(txtTotalCopies.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtPublicationYear.Text, out int publicationYear) ||
                publicationYear < 1200 || publicationYear > DateTime.Now.Year)
            {
                MessageBox.Show("Invalid publication year.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Invalid price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtTotalCopies.Text, out int totalCopies) || totalCopies < 0)
            {
                MessageBox.Show("Invalid total copies.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Book == null)
                Book = new Book();

            Book.Title = txtTitle.Text.Trim();
            Book.Author = txtAuthor.Text.Trim();
            Book.ISBN = txtISBN.Text.Trim();
            Book.Publisher = txtPublisher.Text.Trim();
            Book.PublicationYear = publicationYear;
            Book.Genre = cmbGenre.SelectedItem.ToString();
            Book.Price = price;
            Book.TotalCopies = totalCopies;
            Book.QuantityAvailable = totalCopies;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
