using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DTOK9;
using QuerysK9;

namespace InactivacionMasivaDeCentrosCostoK9
{
    public partial class Form1 : Form
    {
        #region Dependencies

        readonly Lazy<Querys> _querysLazy = new Lazy<Querys>(() => new Querys(_prefix));
        Querys QuerysLazy { get { return _querysLazy.Value; } }
        private static string _prefix = "";

        #endregion

        public Form1(string value)
        {
            InitializeComponent();
            _prefix = value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BindDataGrid("","");
        }

        private void activarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AsignarCuenta("A", "ACTIVAR");
        }

        private void inactivarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AsignarCuenta("I","INACTIVAR");
        }

        private void bloquearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AsignarCuenta("B","BLOQUEAR");
        }

        private void AsignarCuenta(string codigo, string texto)
        {
            dataGridView1.EndEdit();
            var cuetas = (List<CENTRO_COSTO>)dataGridView1.DataSource;
            var centroCostosArray = cuetas.Where(x => x.Marcar).Select(x => x.Centro_Costo).ToArray();

            if (centroCostosArray.Length > 0)
            {
                if (MessageBox.Show(@"Seguro Que Desea " + texto + @" Los Centros De Costos Seleccionados", @"Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

                var result = QuerysLazy.UpdateAsignarCentro_Cuentas(codigo, centroCostosArray);
                if(result)
                    MessageBox.Show(@"Los Centros De Costos Han Sido Actualizados Satisfactoriamente", @"Informacion",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(@"Debe Seleccionar Al Menos Un Centros De Costo Para Continuar Con El Proceso Seleccionado", @"Informacion",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
        }

        private void BindDataGrid(string cuenta, string descripcion)
        {
            var queryCuenta = " AND CENTRO_COSTO LIKE '%" + cuenta + "%' ";
            var queryDescripcion = " AND DESCRIPCION LIKE '%" + descripcion + "%' ";
            var result = "WHERE 1 = 1 ";
            if (!string.IsNullOrEmpty(cuenta) || !string.IsNullOrEmpty(descripcion))
            {
                if (!string.IsNullOrEmpty(cuenta))
                {
                    result += queryCuenta;
                }
                if (!string.IsNullOrEmpty(descripcion))
                {
                    result += queryDescripcion;
                }
                if (result.Equals("WHERE 1 = 1 "))
                {
                    result = "";
                }
            }
            dataGridView1.DataSource = QuerysLazy.GetListOfCentro_Cuentas(result);
            dataGridView1.Columns[0].Width = 60;
            dataGridView1.Show();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells[1].ReadOnly = row.Cells[2].ReadOnly = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BindDataGrid(textBox1.Text, textBox2.Text);
        }
    }
}
