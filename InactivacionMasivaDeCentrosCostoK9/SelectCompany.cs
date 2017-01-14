using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DTOK9;
using System.Collections.Generic;

namespace InactivacionMasivaDeCentrosCostoK9
{
    public partial class SelectCompany : Form
    {
        public SelectCompany()
        {
            InitializeComponent();
        }

        private void SelectCompany_Load(object sender, EventArgs e)
        {
            var listOfCompany = File.ReadAllLines(@"Conexion.txt")[4].Split('=')[1].Split(',');
            var dataSource = listOfCompany.Select(company => new Company
            {
                Check = false, Nombre = company
            }).ToList();
            dataGridView1.DataSource = dataSource;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells[0].ReadOnly = true;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == -1) return;
            var index = 0;

            dataGridView1.EndEdit();
            dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (index == e.RowIndex)
                    row.Cells[1].Value = true;
                else
                    row.Cells[1].Value = false;
                index++;
            }
            dataGridView1.EndEdit();
            dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var list = (List<Company>)dataGridView1.DataSource;
            var data = list.Where(x => x.Check).ToList();
            if (data.Count > 0)
            {
                var form = new Form1(data[0].Nombre);
                form.ShowDialog(this);
                Dispose();
            }
            else
            {
                MessageBox.Show(@"Por Selecciones Una Compañia",
                       @"Error", MessageBoxButtons.OK,
                       MessageBoxIcon.Error);
                return;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
