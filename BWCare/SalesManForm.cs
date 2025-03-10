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
using Guna.UI2.WinForms;

namespace BWCare
{
    public partial class SalesManForm : Form
    {
        public SalesManForm()
        {
            InitializeComponent();
        }
        private string connectionString = "Data Source=DESKTOP-5P6MGCV;Initial Catalog=BwCare;Integrated Security=True";
        private void SalesManForm_Load(object sender, EventArgs e)
        {   

            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Fetch all salesmen
                    string salesmanQuery = "SELECT id, name FROM salesman";
                    SqlCommand salesmanCmd = new SqlCommand(salesmanQuery, conn);
                    SqlDataReader reader = salesmanCmd.ExecuteReader();

                    while (reader.Read())
                    {
                        // Create a TabPage for each salesman
                        int salesmanId = reader.GetInt32(0); // 'id' from salesman table
                        string salesmanName = reader.GetString(1); // 'name' from salesman table

                        TabPage tabPage = new TabPage(salesmanName);
                        guna2TabControl1.TabPages.Add(tabPage);

                        // Add a Guna2DataGridView to the TabPage
                        Guna2DataGridView gunaDataGridView = new Guna2DataGridView
                        {
                            Dock = DockStyle.Fill,
                            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                            ThemeStyle =
                    {
                        AlternatingRowsStyle = { BackColor = Color.LightGray }, // Optional styling
                        RowsStyle = { BackColor = Color.White, ForeColor = Color.Black } // Optional styling
                    }
                        };

                        // Add a checkbox column for selection
                        AddCheckboxColumn(gunaDataGridView);

                        tabPage.Controls.Add(gunaDataGridView);

                        // Fetch and display invoices for the salesman
                        LoadInvoicesForSalesman(gunaDataGridView, salesmanId);

                        // Add the Paid button and its click event
                        Guna2Button paidButton = new Guna2Button
                        {
                            Text = "Mark as Paid",
                            Size = new Size(200, 40),
                            Dock = DockStyle.Bottom,
                            BorderRadius = 20,
                            FillColor = Color.FromArgb(255, 123, 0), // Blue color for button
                            ForeColor = Color.White,
                            Font = new Font("Segoe UI", 10, FontStyle.Bold),
                            ShadowDecoration = { Enabled = true, Color = Color.FromArgb(0, 0, 0), BorderRadius = 20 },
                            HoverState =
                    {
                        FillColor = Color.FromArgb(0, 102, 204), // Darker blue on hover
                        BorderColor = Color.FromArgb(0, 102, 204),
                        ForeColor = Color.White
                    }
                        };

                        paidButton.Click += (s, args) => PaidBtn_Click(gunaDataGridView, salesmanId);

                        tabPage.Controls.Add(paidButton);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading salesmen: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void LoadInvoicesForSalesman(DataGridView dataGridView, int salesmanId)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string invoiceQuery = @"
            SELECT [id], [invoice_date], [invoice_name], [invoice_price], [salesman_id], [created_at], [status] 
            FROM invoices 
            WHERE salesman_id = @salesman_id";

                    SqlDataAdapter adapter = new SqlDataAdapter(invoiceQuery, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@salesman_id", salesmanId);

                    DataTable invoiceTable = new DataTable();
                    adapter.Fill(invoiceTable);

                    dataGridView.DataSource = invoiceTable;

                    // Attach event to colorize "Paid" rows after data loads
                    dataGridView.DataBindingComplete -= DataGridView_DataBindingComplete;
                    dataGridView.DataBindingComplete += DataGridView_DataBindingComplete;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading invoices: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            Guna2DataGridView dataGridView = sender as Guna2DataGridView;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells["status"].Value?.ToString() == "Paid")
                {
                    row.Cells["status"].Style.BackColor = Color.Red; // Red background
                    row.Cells["status"].Style.ForeColor = Color.White; // White text
                    row.Cells["status"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold); // Bold text
                }
            }
        }
        private void ColorizePaidStatus(DataGridViewRow row)
        {
            if (row.Cells["status"].Value?.ToString() == "Paid")
            {
                row.Cells["status"].Style.BackColor = Color.Red; // Background color red
                row.Cells["status"].Style.ForeColor = Color.White; // Text color white
                row.Cells["status"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold); // Bold text
            }
        }
        private void AddCheckboxColumn(Guna2DataGridView dataGridView)
        {

            // Create and configure the checkbox column
            DataGridViewCheckBoxColumn selectColumn = new DataGridViewCheckBoxColumn
            {
                HeaderText = "Select",
                Name = "SelectColumn",
                Width = 50,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                TrueValue = true,
                FalseValue = false
            };

            // Add the checkbox column to the DataGridView
            dataGridView.Columns.Add(selectColumn);
        }
        private void PaidBtn_Click(Guna2DataGridView dataGridView, int salesmanId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        if (row.Cells["SelectColumn"] is DataGridViewCheckBoxCell checkboxCell &&
                            checkboxCell.Value is bool isChecked &&
                            isChecked)
                        {
                            int invoiceId = Convert.ToInt32(row.Cells["id"].Value);
                            DateTime currentDate = DateTime.Now;

                            string updateQuery = @"
                        UPDATE invoices 
                        SET status = 'Paid', created_at = @paidDate 
                        WHERE id = @id";

                            SqlCommand cmd = new SqlCommand(updateQuery, conn);
                            cmd.Parameters.AddWithValue("@id", invoiceId);
                            cmd.Parameters.AddWithValue("@paidDate", currentDate);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Selected invoices marked as Paid.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh DataGridView and apply color
                    LoadInvoicesForSalesman(dataGridView, salesmanId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating invoices: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void PaidBtn_Click(object sender, EventArgs e)
        {
              
        }
    }
}
    