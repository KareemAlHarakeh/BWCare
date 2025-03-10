using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data.SqlClient;

namespace BWCare
{
    public partial class DashboardForm : Form
    {
        public DashboardForm()
        {
            InitializeComponent();
            
            
        }
        private string connectionString = "Data Source=DESKTOP-5P6MGCV;Initial Catalog=BwCare;Integrated Security=True";
        private void DashboardForm_Load(object sender, EventArgs e)
        {
            LoadTransferTrends();
            LoadTransfersByStore();

            LoadStatusChart();
            LoadChartsForAllSalesmen();
        }
        private void LoadTransferTrends()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                    SELECT transfer_date, COUNT(*) AS transfer_count 
                    FROM transfers 
                    GROUP BY transfer_date 
                    ORDER BY transfer_date";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    // Clear previous data
                    chart1.Series.Clear();
                    chart1.ChartAreas.Clear();
                    chart1.ChartAreas.Add("ChartArea1");

                    // Create a new series for the chart
                    Series series = new Series("Transfers Over Time")
                    {
                        ChartType = SeriesChartType.Line, // Line chart
                        BorderWidth = 3,
                        Color = Color.Blue,
                        IsValueShownAsLabel = true // Show values on points
                    };

                    bool hasData = false; // Check if data exists

                    while (reader.Read())
                    {
                        hasData = true;
                        DateTime date = reader.GetDateTime(0);
                        int count = reader.GetInt32(1);
                        series.Points.AddXY(date.ToShortDateString(), count);
                    }

                    reader.Close();

                    if (hasData)
                    {
                        // Add the series to the chart
                        chart1.Series.Add(series);

                        // Set chart title
                        chart1.Titles.Clear();
                        chart1.Titles.Add("Transfers Over Time");
                        chart1.Titles[0].Font = new Font("Arial", 14, FontStyle.Bold);

                        // Customize X-axis
                        chart1.ChartAreas[0].AxisX.Title = "Date";
                        chart1.ChartAreas[0].AxisX.Interval = 1;
                        chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;

                        // Customize Y-axis
                        chart1.ChartAreas[0].AxisY.Title = "Number of Transfers";
                        chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                    }
                    else
                    {
                        MessageBox.Show("No data found for transfer trends.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading transfer trends: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadTransfersByStore()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
            SELECT to_store, COUNT(*) AS transfer_count 
            FROM transfers 
            GROUP BY to_store 
            ORDER BY transfer_count DESC"; // Order by most transfers

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    // Clear previous data
                    chart2.Series.Clear();
                    chart2.ChartAreas.Clear();
                    chart2.ChartAreas.Add("ChartArea1");

                    // Create new bar chart series
                    Series series = new Series("Transfers by Store")
                    {
                        ChartType = SeriesChartType.Column, // Bar chart
                        Color = Color.Green, // Change color
                        IsValueShownAsLabel = true // Show values
                    };

                    while (reader.Read())
                    {
                        string store = reader.GetString(0);
                        int count = reader.GetInt32(1);
                        series.Points.AddXY(store, count);
                    }

                    reader.Close();

                    // Add the series to the chart
                    chart2.Series.Add(series);

                    // Set chart title
                    chart2.Titles.Clear();
                    chart2.Titles.Add("Transfers by Store");
                    chart2.Titles[0].Font = new Font("Arial", 14, FontStyle.Bold);

                    // Customize X-axis (Stores)
                    chart2.ChartAreas[0].AxisX.Title = "Store";
                    chart2.ChartAreas[0].AxisX.Interval = 1;
                    chart2.ChartAreas[0].AxisX.LabelStyle.Angle = -45; // Rotate labels for readability
                    chart2.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;

                    // Customize Y-axis (Transfers Count)
                    chart2.ChartAreas[0].AxisY.Title = "Number of Transfers";
                    chart2.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading store transfer data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadChartsForAllSalesmen()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
            SELECT s.name, SUM(i.invoice_price) AS total_sales 
            FROM invoices i
            JOIN salesman s ON i.salesman_id = s.id
            GROUP BY s.name
            ORDER BY total_sales DESC"; // Sort by highest sales

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    // Clear previous data
                    chart3.Series.Clear();
                    chart3.ChartAreas.Clear();
                    chart3.ChartAreas.Add("ChartArea1");

                    // Create new bar chart series
                    Series series = new Series("Sales \n by Salesman")
                    {
                        ChartType = SeriesChartType.Column, // Bar chart
                        Color = Color.Purple,
                        IsValueShownAsLabel = true
                    };

                    while (reader.Read())
                    {
                        string salesman = reader.GetString(0);
                        decimal totalSales = reader.GetDecimal(1);
                        series.Points.AddXY(salesman, totalSales);
                    }

                    reader.Close();

                    // Add the series to the chart
                    chart3.Series.Add(series);

                    // Set chart title
                    chart3.Titles.Clear();
                    chart3.Titles.Add("Total Sales by Salesman");
                    chart3.Titles[0].Font = new Font("Arial", 14, FontStyle.Bold);
                   

                    


                    // Customize X-axis (Salesmen)
                    chart3.ChartAreas[0].AxisX.Title = "Salesman";
                    chart3.ChartAreas[0].AxisX.Interval = 1;
                    chart3.ChartAreas[0].AxisX.LabelStyle.Angle = -45; // Rotate labels for readability
                    chart3.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;

                    // Customize Y-axis (Sales)
                    chart3.ChartAreas[0].AxisY.Title = "Total Sales ($)";
                    chart3.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading sales data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void LoadStatusChart()
        {
            string query = "SELECT status, COUNT(*) as count FROM invoices GROUP BY status";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                chart4.Series.Clear();
                Series series = new Series("Item Status");
                series.ChartType = SeriesChartType.Pie;
                series.IsValueShownAsLabel = true;
                series.LabelForeColor = System.Drawing.Color.Black;

                while (reader.Read())
                {
                    string status = reader["status"].ToString();
                    int count = Convert.ToInt32(reader["count"]);
                    int pointIndex = series.Points.AddXY(status, count);
                    series.Points[pointIndex].LegendText = $"{status}"; // ✅ Fix: Remove ":1"
                    series.Points[pointIndex].Label = $"{status}: {count}";
                }

                chart4.Series.Add(series);
                chart4.Titles.Clear();
                chart4.Titles.Add("Items by Status");

                // ✅ Enable different colors
                series.Palette = ChartColorPalette.BrightPastel;

                reader.Close();
            }
        }

    }
}
