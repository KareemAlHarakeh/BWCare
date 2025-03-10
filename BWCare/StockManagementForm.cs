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
    public partial class StockManagementForm : Form
    {
        public StockManagementForm()
        {
            InitializeComponent();
        }
        private string connectionString = "Data Source=DESKTOP-5P6MGCV;Initial Catalog=BwCare;Integrated Security=True";
        private void StockManagementForm_Load(object sender, EventArgs e)
        {
            LoadTransfer();
            AddCheckboxColumn();
        }
        private void LoadTransfer()
        {
            try
            {
                string query = "SELECT *   FROM transfers";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    DataTable invoicesTable = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(invoicesTable);

                    StockMgtDatagridView.DataSource = invoicesTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void AddCheckboxColumn()
        {
            // Create a new checkbox column
            DataGridViewCheckBoxColumn checkColumn = new DataGridViewCheckBoxColumn
            {
                Name = "Select", // You can give the column any name
                HeaderText = "Select", // Column header text
                Width = 50, // Set the width of the checkbox column
                FalseValue = false,
                TrueValue = true
            };

            // Add the checkbox column to the DataGridView
            if (!StockMgtDatagridView.Columns.Contains("Select"))
            {
                StockMgtDatagridView.Columns.Insert(0, checkColumn); // Adds it as the first column
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
                    foreach (DataGridViewRow row in StockMgtDatagridView.Rows)
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
                    string fromStore = lastRow.Cells["from_store"].Value?.ToString() ?? "";
                    string toStore = lastRow.Cells["to_store"].Value?.ToString() ?? "";
                    string itemIdStr = lastRow.Cells["item_id"].Value?.ToString() ?? "";

                    // Validate data
                    if (string.IsNullOrEmpty(fromStore) ||
                        string.IsNullOrEmpty(toStore) ||
                        !int.TryParse(itemIdStr, out int itemId))
                    {
                        lastRow.DefaultCellStyle.BackColor = Color.Red;
                        MessageBox.Show("Invalid data in the last row. Please check and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DateTime transferDate = DateTime.Now;
                    DateTime createdAt = DateTime.Now;

                    // Insert the last row data into the database
                    string query = "INSERT INTO transfers (transfer_date, from_store, to_store, item_id, created_at) VALUES (@transferDate, @fromStore, @toStore, @itemId, @createdAt)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@transferDate", transferDate);
                        cmd.Parameters.AddWithValue("@fromStore", fromStore);
                        cmd.Parameters.AddWithValue("@toStore", toStore);
                        cmd.Parameters.AddWithValue("@itemId", itemId);
                        cmd.Parameters.AddWithValue("@createdAt", createdAt);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Transfer added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTransfer(); // Reload data to show the newly inserted row
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding transfer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    foreach (DataGridViewRow row in StockMgtDatagridView.Rows)
                    {
                        // Check if the row is selected (checked)
                        if (row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value)
                        {
                            // Get the values of the selected row
                            int transferId = Convert.ToInt32(row.Cells["id"].Value);
                            DateTime transferDate = Convert.ToDateTime(row.Cells["transfer_date"].Value);
                            string fromStore = row.Cells["from_store"].Value?.ToString();
                            string toStore = row.Cells["to_store"].Value?.ToString();
                            int itemId = Convert.ToInt32(row.Cells["item_id"].Value);

                            // Validate data
                            if (string.IsNullOrEmpty(fromStore) || string.IsNullOrEmpty(toStore))
                            {
                                MessageBox.Show("Invalid data. Please ensure all fields are filled out correctly.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // Update the transfer in the database
                            string query = @"UPDATE transfers 
                                     SET transfer_date = @transfer_date, 
                                         from_store = @from_store, 
                                         to_store = @to_store, 
                                         item_id = @item_id 
                                     WHERE id = @id";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@transfer_date", transferDate);
                                cmd.Parameters.AddWithValue("@from_store", fromStore);
                                cmd.Parameters.AddWithValue("@to_store", toStore);
                                cmd.Parameters.AddWithValue("@item_id", itemId);
                                cmd.Parameters.AddWithValue("@id", transferId);

                                cmd.ExecuteNonQuery(); // Execute the update query
                            }

                            MessageBox.Show("Transfer updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Reload data after updating
                            LoadTransfer();
                            return; // Exit once the item is updated
                        }
                    }

                    // If no row was selected
                    MessageBox.Show("Please select a transfer to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating transfer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    foreach (DataGridViewRow row in StockMgtDatagridView.Rows)
                    {
                        // Check if the row is selected (checked)
                        if (row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value)
                        {
                            int transferId = Convert.ToInt32(row.Cells["id"].Value);

                            // Delete the item from the database
                            string query = "DELETE FROM transfers WHERE id = @id";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", transferId);
                                cmd.ExecuteNonQuery(); // Execute the delete query
                            }

                            // Optionally, remove the row from DataGridView (optional)
                            StockMgtDatagridView.Rows.Remove(row);
                        }
                    }

                    // Reload data after deleting
                    LoadTransfer();

                    MessageBox.Show("Selected items deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExcelBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel Files|*.xlsx";
                    saveFileDialog.Title = "Save as Excel File";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Create a new Excel workbook
                        using (var workbook = new ClosedXML.Excel.XLWorkbook())
                        {
                            // Add a worksheet to the workbook
                            var worksheet = workbook.Worksheets.Add("Data");

                            // Add headers from DataGridView to Excel
                            for (int col = 0; col < StockMgtDatagridView.Columns.Count; col++)
                            {
                                worksheet.Cell(1, col + 1).Value = StockMgtDatagridView.Columns[col].HeaderText;
                            }

                            // Add rows from DataGridView to Excel
                            for (int row = 0; row < StockMgtDatagridView.Rows.Count; row++)
                            {
                                for (int col = 0; col < StockMgtDatagridView.Columns.Count; col++)
                                {
                                    worksheet.Cell(row + 2, col + 1).Value = StockMgtDatagridView.Rows[row].Cells[col].Value?.ToString() ?? "";
                                }
                            }

                            // Save the Excel file
                            workbook.SaveAs(saveFileDialog.FileName);
                        }

                        MessageBox.Show("Data successfully exported to Excel!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            string searchTerm = SearchTxt.Text.Trim();  // Get the text from the search box

            if (string.IsNullOrEmpty(searchTerm))
            {
                // If the search term is empty, load all items
                LoadTransfer();
            }
            else
            {
                // If there is a search term, load filtered items
                LoadItemsTransfer(searchTerm);
            }
        }
        private void LoadItemsTransfer(string searchTerm = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // SQL query to get data from items table, with an optional filter for item_name
                    string query = "SELECT [id],[transfer_date], [from_store], [to_store], [item_id],[created_at] FROM [BwCare].[dbo].[transfers]";

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        // Add a WHERE clause to filter by item_name
                        query += " WHERE [from_store] LIKE @searchTerm";
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
                        StockMgtDatagridView.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            LoadTransfer();
        }
    }
}
