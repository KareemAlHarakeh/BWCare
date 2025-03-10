using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BWCare
{
    public partial class MenuForm : Form
    {
        public MenuForm()
        {
            InitializeComponent();
        }
        private string userRole;
        public MenuForm(string role)
        {
            InitializeComponent();
            this.userRole = role;
            ConfigurePermissions();
        }
        private void ConfigurePermissions()
        {
            if (userRole != "Manager")
            {
                DashboardBtn.Enabled = false;  // Disable for non-managers
                guna2PictureBox6.Enabled = false;
                label7.Enabled = false;
            }
            else
            {
                DashboardBtn.Enabled = true;   // Enable for managers
                guna2PictureBox6.Enabled = true;
                label7.Enabled = true;
            }
        }
    

    private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void InvoiceBtn_Click(object sender, EventArgs e)
        {
            InvoiceForm IF = new InvoiceForm();
            OpenForm(IF);
           
        }

        private void MainPanel_Paint(object sender, PaintEventArgs e)
        {

        }
        private Stack<Form> formStack = new Stack<Form>();
        private void OpenForm(Form form)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            MainPanel.Controls.Clear();
            MainPanel.Controls.Add(form);
            form.Show();
            formStack.Push(form);
        }

        private void ItemsBtn_Click(object sender, EventArgs e)
        {
            DataEntryForm DE = new DataEntryForm();
            OpenForm(DE);
        }

        private void StockMgtBtn_Click(object sender, EventArgs e)
        {
            StockManagementForm SMF = new StockManagementForm();
            OpenForm(SMF);
        }

        private void SalesManBtn_Click(object sender, EventArgs e)
        {
            SalesManForm SalesF = new SalesManForm();
            OpenForm(SalesF);
        }

        private void StockInventoryBtn_Click(object sender, EventArgs e)
        {
            StockInvForm SIF = new StockInvForm();
            OpenForm(SIF);
        }

        private void DashboardBtn_Click(object sender, EventArgs e)
        {
            DashboardForm df = new DashboardForm();
            OpenForm(df);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form1 f1 = new Form1();
            f1.Show();
            this.Hide();
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

            DataEntryForm DE = new DataEntryForm();
            OpenForm(DE);
        }

        private void ItemPanel_Paint(object sender, PaintEventArgs e)
        {

           
        }

        private void label2_Click(object sender, EventArgs e)
        {

            DataEntryForm DE = new DataEntryForm();
            OpenForm(DE);
        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {
            StockInvForm SIF = new StockInvForm();
            OpenForm(SIF);
        }

        private void label3_Click(object sender, EventArgs e)
        {
            StockInvForm SIF = new StockInvForm();
            OpenForm(SIF);
        }

        private void guna2PictureBox3_Click(object sender, EventArgs e)
        {
            InvoiceForm IF = new InvoiceForm();
            OpenForm(IF);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            InvoiceForm IF = new InvoiceForm();
            OpenForm(IF);
        }

        private void guna2PictureBox5_Click(object sender, EventArgs e)
        {
            StockManagementForm SMF = new StockManagementForm();
            OpenForm(SMF);
        }

        private void label6_Click(object sender, EventArgs e)
        {
            StockManagementForm SMF = new StockManagementForm();
            OpenForm(SMF);
        }

        private void guna2PictureBox4_Click(object sender, EventArgs e)
        {

            SalesManForm SalesF = new SalesManForm();
            OpenForm(SalesF);
        }

        private void label5_Click(object sender, EventArgs e)
        {

            SalesManForm SalesF = new SalesManForm();
            OpenForm(SalesF);
        }

        private void guna2PictureBox6_Click(object sender, EventArgs e)
        {
            DashboardForm df = new DashboardForm();
            OpenForm(df);
        }

        private void label7_Click(object sender, EventArgs e)
        {
            DashboardForm df = new DashboardForm();
            OpenForm(df);
        }
    }
}
