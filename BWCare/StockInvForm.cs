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
    public partial class StockInvForm : Form
    {
        public StockInvForm()
        {
            InitializeComponent();
        }
        private string connectionString = "Data Source=DESKTOP-5P6MGCV;Initial Catalog=BwCare;Integrated Security=True";

        private void PreviewBtn_Click(object sender, EventArgs e)
        {
            LoadPreviewData();
        }
        private void LoadPreviewData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Get selected date range
                    DateTime fromDate = FromDateTimePicker.Value.Date;
                    DateTime toDate = ToDateTimePicker.Value.Date.AddDays(1).AddSeconds(-1); // Include full day

                    // Get selected item name from ComboBox
                    string selectedItem = ItemsCombox.SelectedItem?.ToString();

                    // Get selected store name from StoreComboBox
                    string selectedStore = StoreCombox.SelectedItem?.ToString();

                    // Corrected SQL Query with Item Name and Store Filtering
                     string query = @"
                     SELECT 
                        t.to_store, 
                        t.transfer_date, 
                        i.item_name  -- Getting item_name from items table
                    FROM transfers t
                    JOIN items i ON t.item_id = i.id  -- Linking transfers to items
                    WHERE t.transfer_date BETWEEN @FromDate AND @ToDate
                    AND i.item_name = @ItemName
                    AND t.to_store = @StoreName;";  // Filter by selected store

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FromDate", fromDate);
                        cmd.Parameters.AddWithValue("@ToDate", toDate);
                        cmd.Parameters.AddWithValue("@ItemName", selectedItem); // Pass the selected item
                        cmd.Parameters.AddWithValue("@StoreName", selectedStore); // Pass the selected store

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        // Bind data to Guna2DataGridView
                        guna2DataGridView1.DataSource = dt;

                        // Adjust column width and header height
                        // Adjust column width and header height
                        foreach (DataGridViewColumn column in guna2DataGridView1.Columns)
                        {
                            column.Width = 100;  // Set column width to 80
                        }

                        guna2DataGridView1.ColumnHeadersHeight = 30;  // Increase header height for better visibility
                        guna2DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None; // Disable auto-sizing
                        guna2DataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        guna2DataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading preview data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void StockInvForm_Load(object sender, EventArgs e)
        {
            LoadItemNames();
            LoadStores();
        }
        private void LoadItemNames()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT DISTINCT item_name FROM items";  // Get unique item names

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    ItemsCombox.Items.Clear(); // Clear previous items
                    while (reader.Read())
                    {
                        ItemsCombox.Items.Add(reader["item_name"].ToString());
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading item names: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStores()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT DISTINCT to_store FROM transfers"; // Get unique stores

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    StoreCombox.Items.Clear(); // Clear previous items

                    // Use a HashSet to ensure unique items are added to the combo box
                    HashSet<string> uniqueStores = new HashSet<string>();

                    while (reader.Read())
                    {
                        string store = reader["to_store"].ToString().Trim(); // Remove any extra spaces
                        uniqueStores.Add(store); // HashSet will automatically handle duplicates
                    }

                    // Add unique stores to the ComboBox
                    foreach (var store in uniqueStores)
                    {
                        StoreCombox.Items.Add(store);
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading stores: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

