using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BWCare
{
    public partial class DataEntryForm : Form
    {
        public DataEntryForm()
        {
            InitializeComponent();
        }
        private string connectionString = "Data Source=DESKTOP-5P6MGCV;Initial Catalog=BwCare;Integrated Security=True";
        private void DataEntryForm_Load(object sender, EventArgs e)
        {
            // Add the checkbox column to the DataGridView
            DataGridViewCheckBoxColumn selectColumn = new DataGridViewCheckBoxColumn();
            selectColumn.Name = "Select";
            selectColumn.HeaderText = "Select"; // Optional: change header text
            selectColumn.Width = 50; // Optional: adjust column width
            ItemsDataGridView.Columns.Insert(0, selectColumn);

            LoadItemsData();
            SearchBtn.Click += new EventHandler(SearchBtn_Click);

        }
        private void LoadItemsData(string searchTerm = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // SQL query to get data from items table, with an optional filter for item_name
                    string query = "SELECT [id], [item_name], [item_description], [item_price], [created_at] FROM [BwCare].[dbo].[items]";

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        // Add a WHERE clause to filter by item_name
                        query += " WHERE [item_name] LIKE @searchTerm";
                    }

                    // SqlDataAdapter to fill the DataGridView
                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            da.SelectCommand.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%"); // Use LIKE for search
                        }

                        DataTable dt = new DataTable();
                        da.Fill(dt); // Fill the DataTable with data from the database

                        // Bind the DataTable to the DataGridView
                        ItemsDataGridView.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Find the last row that is not the new row
                    DataGridViewRow lastRow = null;
                    foreach (DataGridViewRow row in ItemsDataGridView.Rows)
                    {
                        if (row.IsNewRow) continue; // Skip the placeholder new row
                        lastRow = row; // Keep updating to the last non-new row
                    }

                    if (lastRow == null)
                    {
                        MessageBox.Show("No rows to insert.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Get values from the last row
                    string itemName = lastRow.Cells["item_name"].Value?.ToString() ?? "";
                    string itemDescription = lastRow.Cells["item_description"].Value?.ToString() ?? "";
                    string itemPriceStr = lastRow.Cells["item_price"].Value?.ToString() ?? "";

                    // Validate data
                    if (string.IsNullOrEmpty(itemName) || !decimal.TryParse(itemPriceStr, out decimal itemPrice))
                    {
                        lastRow.DefaultCellStyle.BackColor = Color.Red; // Highlight invalid row
                        MessageBox.Show("Invalid data in the last row. Please check and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DateTime createdAt = DateTime.Now;

                    // Insert into the database
                    string query = "INSERT INTO items (item_name, item_description, item_price, created_at) VALUES (@name, @description, @price, @createdAt)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", itemName);
                        cmd.Parameters.AddWithValue("@description", itemDescription);
                        cmd.Parameters.AddWithValue("@price", itemPrice);
                        cmd.Parameters.AddWithValue("@createdAt", createdAt);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Item added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadItemsData(); // Refresh the DataGridView
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Loop through rows to find the selected (checked) rows
                    foreach (DataGridViewRow row in ItemsDataGridView.Rows)
                    {
                        // Check if the row is selected (checked)
                        if (row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value)
                        {
                            // Get the values of the selected row
                            int itemId = Convert.ToInt32(row.Cells["id"].Value);
                            string itemName = row.Cells["item_name"].Value?.ToString();
                            string itemDescription = row.Cells["item_description"].Value?.ToString();
                            string itemPriceStr = row.Cells["item_price"].Value?.ToString();

                            // Validate data
                            if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(itemDescription) ||
                                !decimal.TryParse(itemPriceStr, out decimal itemPrice))
                            {
                                MessageBox.Show("Invalid data. Please ensure all fields are filled out correctly.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // Update the item in the database
                            string query = "UPDATE items SET item_name = @name, item_description = @description, item_price = @price WHERE id = @id";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@name", itemName);
                                cmd.Parameters.AddWithValue("@description", itemDescription);
                                cmd.Parameters.AddWithValue("@price", itemPrice);
                                cmd.Parameters.AddWithValue("@id", itemId);

                                cmd.ExecuteNonQuery(); // Execute the update query
                            }

                            MessageBox.Show("Item updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Reload data after updating
                            LoadItemsData();
                            return; // Exit once the item is updated
                        }
                    }

                    // If no row was selected
                    MessageBox.Show("Please select an item to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Loop through rows to find the selected (checked) rows
                    foreach (DataGridViewRow row in ItemsDataGridView.Rows)
                    {
                        // Check if the row is selected (checked)
                        if (row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value)
                        {
                            int itemId = Convert.ToInt32(row.Cells["id"].Value);

                            // Delete the item from the database
                            string query = "DELETE FROM items WHERE id = @id";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", itemId);
                                cmd.ExecuteNonQuery(); // Execute the delete query
                            }

                            // Optionally, remove the row from DataGridView (optional)
                            ItemsDataGridView.Rows.Remove(row);
                        }
                    }

                    // Reload data after deleting
                    LoadItemsData();

                    MessageBox.Show("Selected items deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadItemsData();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            LoadItemsData();
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            string searchTerm = SearchTxt.Text.Trim();  // Get the text from the search box

            if (string.IsNullOrEmpty(searchTerm))
            {
                // If the search term is empty, load all items
                LoadItemsData();
            }
            else
            {
                // If there is a search term, load filtered items
                LoadItemsData(searchTerm);
            }
        }
    }
}
