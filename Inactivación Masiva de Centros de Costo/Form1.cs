using System;
using System.Windows.Forms;
using QuerysK9;

namespace Inactivación_Masiva_de_Centros_de_Costo
{
    public partial class Form1 : Form
    {
        #region Dependencies

        readonly Lazy<Querys> _querysLazy = new Lazy<Querys>(() => new Querys());
        Querys QuerysLazy { get { return _querysLazy.Value; } }

        #endregion
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = QuerysLazy.GetListOfConceptoBytipo();
            dataGridView1.Columns[4].Visible = true;
            dataGridView1.Show();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells[0].ReadOnly = row.Cells[1].ReadOnly = row.Cells[2].ReadOnly = row.Cells[3].ReadOnly = row.Cells[3].ReadOnly = true;
            }
        }
    }
}
