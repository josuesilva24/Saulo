using System.Collections.Generic;
using System.Windows.Forms;
using DTOK9;

namespace AsociacionMasivaCuentasContableK9
{
    public partial class ErrorListForm : Form
    {
        private readonly List<ErrorConceptos> _errorConceptos;

        public ErrorListForm(List<ErrorConceptos> errorConceptos)
        {
            _errorConceptos = errorConceptos;
            InitializeComponent();
        }

        private void ErrorListForm_Load(object sender, System.EventArgs e)
        {
            dataGridView1.DataSource = _errorConceptos;
            dataGridView1.Columns[0].Width = 60;
            dataGridView1.Show();
            MessageBox.Show(@"Errores En Cuentas", @"Informacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
