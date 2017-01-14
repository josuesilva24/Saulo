using System;
using System.Windows.Forms;
using QuerysK9;

namespace AsociacionMasivaCuentasContableK9
{
    public partial class SeleccionarCuentas : Form
    {
        #region Dependencies

        readonly Lazy<Querys> _querysLazy = new Lazy<Querys>(() => new Querys(PreFFix));
            Querys QuerysLazy { get { return _querysLazy.Value; } }

        #endregion

        #region Properties

            public string CuentaSelected { get; set; }
            public static string PreFFix { get; set; }

            #endregion

        public SeleccionarCuentas(string value)
        {
            PreFFix = value;
            InitializeComponent();
        }

        private void SeleccionarCuentas_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = QuerysLazy.GetListOfCuentasBytipo();
            dataGridView1.Show();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells[0].ReadOnly = row.Cells[1].ReadOnly = true;
            }
        }

        public void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == -1 || e.RowIndex == -1) return;
            if (dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString() == "S")
            {
                CuentaSelected = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                prpStrText = CuentaSelected;
                Close(); 
            }
            else
            {
                MessageBox.Show(@"La Cuenta Seleccionada No Permite Movimientos", @"Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            
        }

        public event EventHandler<TextChangeEventArgs> UpdateTextBox;

        private string strText;

        public string prpStrText
        {
            get { return strText; }
            set
            {
                if (strText != value)
                {
                    strText = value;
                    OnTextBoxTextChanged(new TextChangeEventArgs(strText));
                }
            }
        }

        private void textBox_Form2_TextChanged(object sender, EventArgs e)
        {
            //prpStrText ="test3";
        }

        protected virtual void OnTextBoxTextChanged(TextChangeEventArgs e)
        {
            EventHandler<TextChangeEventArgs> eventHandler = UpdateTextBox;
            if (eventHandler != null)
            {
                eventHandler(this, e);
            }
        }
    }
}
