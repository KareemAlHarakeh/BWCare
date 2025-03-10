using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BWCare
{
    public partial class InvoiceForm : Form
    {
        public InvoiceForm()
        {
            InitializeComponent();
        }
        private string connectionString = "Data Source=DESKTOP-5P6MGCV;Initial Catalog=BwCare;Integrated Security=True";

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void InvoiceForm_Load(object sender, EventArgs e)
        {
            LoadInvoices();
            AddCheckboxColumn(); 
            LoadStatusComboBox();

        }

        private void LoadStatusComboBox()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Query to get distinct status values
                    string query = "SELECT DISTINCT status FROM invoices";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            StatusCombox.Items.Clear(); // Clear existing items

                            while (reader.Read())
                            {
                                string statusValue = reader["status"].ToString();
                                if (!string.IsNullOrEmpty(statusValue))
                                {
                                    StatusCombox.Items.Add(statusValue);
                                }
                            }
                        }
                    }
                }

                // Optionally, add a default value like "Select Status"
                StatusCombox.Items.Insert(0, "Select Status");
                StatusCombox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading status values: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadInvoices()
        {
            try
            {
                string query = "SELECT id, invoice_date, invoice_name , invoice_price, salesman_id, status,created_at   FROM invoices";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    DataTable invoicesTable = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(invoicesTable);

                    guna2DataGridView1.DataSource = invoicesTable;
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
                guna2DataGridView1.AllowUserToAddRows = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error enabling row addition: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddInvoiceButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                    {
                        // Skip the new row placeholder
                        if (row.IsNewRow) continue;

                        // Check if this row is new (by some logic or flag)
                        if (row.Cells["invoice_name"].Value == null || row.Cells["invoice_price"].Value == null || row.Cells["salesman_id"].Value == null)
                        {
                            MessageBox.Show("Please fill all required fields before adding.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }

                        // Fetch and validate values
                        string invoiceName = row.Cells["invoice_name"].Value.ToString();
                        string invoicePriceStr = row.Cells["invoice_price"].Value.ToString();
                        string salesmanIdStr = row.Cells["salesman_id"].Value.ToString();

                        if (!decimal.TryParse(invoicePriceStr, out decimal invoicePrice) || !int.TryParse(salesmanIdStr, out int salesmanId))
                        {
                            MessageBox.Show("Invalid data format. Check price and salesman ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }

                        // Automatically use the current date
                        DateTime invoiceDate = DateTime.Now;

                        // Insert into database
                        string query = "INSERT INTO invoices (invoice_name, invoice_price, salesman_id) VALUES (@name, @price, @salesmanId)"; 
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@date", invoiceDate);
                            cmd.Parameters.AddWithValue("@name", invoiceName);
                            cmd.Parameters.AddWithValue("@price", invoicePrice);
                            cmd.Parameters.AddWithValue("@salesmanId", salesmanId);

                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Invoices added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadInvoices();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding invoices: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (!guna2DataGridView1.Columns.Contains("Select"))
            {
                guna2DataGridView1.Columns.Insert(0, checkColumn); // Adds it as the first column
            }
        }

        private void InvoiceInsertBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Find the last row that is not the new row
                    DataGridViewRow lastRow = null;
                    foreach (DataGridViewRow row in guna2DataGridView1.Rows)
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
                    string invoiceName = lastRow.Cells["invoice_name"].Value?.ToString() ?? "";
                    string invoicePriceStr = lastRow.Cells["invoice_price"].Value?.ToString() ?? "";
                    string salesmanIdStr = lastRow.Cells["salesman_id"].Value?.ToString() ?? "";

                    // Validate data
                    if (string.IsNullOrEmpty(invoiceName) ||
                        !decimal.TryParse(invoicePriceStr, out decimal invoicePrice) ||
                        !int.TryParse(salesmanIdStr, out int salesmanId))
                    {
                        lastRow.DefaultCellStyle.BackColor = Color.Red;
                        MessageBox.Show("Invalid data in the last row. Please check and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DateTime invoiceDate = DateTime.Now;

                    // Insert the last row data into the database
                    string query = "INSERT INTO invoices (invoice_date, invoice_name, invoice_price, salesman_id) VALUES (@date, @name, @price, @salesmanId)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@date", invoiceDate);
                        cmd.Parameters.AddWithValue("@name", invoiceName);
                        cmd.Parameters.AddWithValue("@price", invoicePrice);
                        cmd.Parameters.AddWithValue("@salesmanId", salesmanId);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Last invoice added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadInvoices();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding invoice: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Find the last row that is not the new row
                    DataGridViewRow lastRow = null;
                    foreach (DataGridViewRow row in guna2DataGridView1.Rows)
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
                    string invoiceName = lastRow.Cells["invoice_name"].Value?.ToString() ?? "";
                    string invoicePriceStr = lastRow.Cells["invoice_price"].Value?.ToString() ?? "";
                    string salesmanIdStr = lastRow.Cells["salesman_id"].Value?.ToString() ?? "";
                    string status = lastRow.Cells["status"].Value?.ToString() ?? "";

                    // Validate data
                    if (string.IsNullOrEmpty(invoiceName) ||
                        !decimal.TryParse(invoicePriceStr, out decimal invoicePrice) ||
                        !int.TryParse(salesmanIdStr, out int salesmanId) ||
                        string.IsNullOrEmpty(status))
                    {
                        lastRow.DefaultCellStyle.BackColor = Color.Red;
                        MessageBox.Show("Invalid data in the last row. Please check and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DateTime invoiceDate = DateTime.Now;

                    // Insert the last row data into the database
                    string query = "INSERT INTO invoices (invoice_date, invoice_name, invoice_price, salesman_id, status) VALUES (@date, @name, @price, @salesmanId, @status)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@date", invoiceDate);
                        cmd.Parameters.AddWithValue("@name", invoiceName);
                        cmd.Parameters.AddWithValue("@price", invoicePrice);
                        cmd.Parameters.AddWithValue("@salesmanId", salesmanId);
                        cmd.Parameters.AddWithValue("@status", status);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Last invoice added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadInvoices();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding invoice: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    bool anyRowSelected = false;

                    foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue;

                        bool isChecked = Convert.ToBoolean(row.Cells["Select"].Value);
                        if (isChecked)
                        {
                            anyRowSelected = true;

                            // Fetching values from the selected row
                            string invoiceId = row.Cells["id"].Value?.ToString(); // This should be 'id' in your DataGridView, not 'invoice_id'
                            string invoiceName = row.Cells["invoice_name"].Value?.ToString();
                            string invoicePriceStr = row.Cells["invoice_price"].Value?.ToString();
                            string salesmanIdStr = row.Cells["salesman_id"].Value?.ToString();

                            // Validate the fields
                            if (string.IsNullOrEmpty(invoiceId) ||
                                string.IsNullOrEmpty(invoiceName) ||
                                !decimal.TryParse(invoicePriceStr, out decimal invoicePrice) ||
                                !int.TryParse(salesmanIdStr, out int salesmanId))
                            {
                                MessageBox.Show("Please fill in all fields correctly for selected invoices.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // Adjusting column names if needed
                            string query = "UPDATE invoices SET invoice_name = @name, invoice_price = @price, salesman_id = @salesmanId WHERE id = @Id"; // Use 'id' for the invoice ID column name

                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                // Parameters
                                cmd.Parameters.AddWithValue("@Id", invoiceId);  // Use 'id' column from the DataGridView
                                cmd.Parameters.AddWithValue("@name", invoiceName);
                                cmd.Parameters.AddWithValue("@price", invoicePrice);
                                cmd.Parameters.AddWithValue("@salesmanId", salesmanId);

                                // Execute query
                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Invoice updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadInvoices(); // Reload data after successful update
                                }
                                else
                                {
                                    MessageBox.Show("No matching invoice found to update.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }

                    // If no row is selected
                    if (!anyRowSelected)
                    {
                        MessageBox.Show("Please select at least one row to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating invoice: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void EditCheckBtn_Click(object sender, EventArgs e)
        {
         
        }
        private void guna2DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Handle cell value changes (optional, depending on your requirements)
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewRow row = guna2DataGridView1.Rows[e.RowIndex];
                // You could validate the cell data here if needed
            }
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    bool anyRowSelected = false;
                    List<int> selectedInvoiceIds = new List<int>();

                    // Verify the column name for 'id' is correct in the DataGridView
                    if (!guna2DataGridView1.Columns.Contains("id"))
                    {
                        MessageBox.Show("Column 'id' does not exist in the DataGridView.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Loop through each row and check for selected checkboxes
                    foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue;

                        bool isChecked = Convert.ToBoolean(row.Cells["Select"].Value);
                        if (isChecked)
                        {
                            anyRowSelected = true;

                            // Get the invoice_id of the selected row (changed to 'id')
                            if (row.Cells["id"].Value != null)
                            {
                                string invoiceIdStr = row.Cells["id"].Value.ToString();
                                if (int.TryParse(invoiceIdStr, out int invoiceId))
                                {
                                    selectedInvoiceIds.Add(invoiceId);
                                }
                            }
                        }
                    }

                    if (!anyRowSelected)
                    {
                        MessageBox.Show("Please select at least one row to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // If there are selected invoices, proceed with deletion
                    foreach (int invoiceId in selectedInvoiceIds)
                    {
                        // SQL query to delete the invoice by id
                        string query = "DELETE FROM invoices WHERE id = @invoiceId";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@invoiceId", invoiceId);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Invoice deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show($"No invoice found with ID {invoiceId} to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }

                    LoadInvoices(); // Reload the data to reflect the changes
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting invoice: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // Get selected dates from DateTimePickers
                DateTime fromDate = FromDateTimePicker.Value.Date;
                DateTime toDate = ToDateTimePicker.Value.Date;

                // Get selected status from the combo box
                string selectedStatus = StatusCombox.SelectedItem?.ToString();

                // Ensure valid range
                if (fromDate > toDate)
                {
                    MessageBox.Show("The 'From' date cannot be later than the 'To' date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Ensure a status is selected
                if (string.IsNullOrEmpty(selectedStatus))
                {
                    MessageBox.Show("Please select a status from the dropdown.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Query to fetch data based on the selected date range and status
                    string query = @"SELECT * FROM invoices 
                             WHERE invoice_date BETWEEN @fromDate AND @toDate
                             AND status = @status";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameters to prevent SQL injection
                        cmd.Parameters.AddWithValue("@fromDate", fromDate);
                        cmd.Parameters.AddWithValue("@toDate", toDate);
                        cmd.Parameters.AddWithValue("@status", selectedStatus);

                        // Execute query and load data into a DataTable
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            // Bind the DataTable to the DataGridView
                            guna2DataGridView1.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadInvoices();
        }

        private void button4_Click(object sender, EventArgs e)
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
                            for (int col = 0; col < guna2DataGridView1.Columns.Count; col++)
                            {
                                worksheet.Cell(1, col + 1).Value = guna2DataGridView1.Columns[col].HeaderText;
                            }

                            // Add rows from DataGridView to Excel
                            for (int row = 0; row < guna2DataGridView1.Rows.Count; row++)
                            {
                                for (int col = 0; col < guna2DataGridView1.Columns.Count; col++)
                                {
                                    worksheet.Cell(row + 2, col + 1).Value = guna2DataGridView1.Rows[row].Cells[col].Value?.ToString() ?? "";
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
    }
    
}


