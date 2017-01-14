using System;
using System.Collections.Generic;
using System.Windows.Forms;
using QuerysK9;
using System.Linq;
using System.Threading;
using DTOK9;

namespace AsociacionMasivaCuentasContableK9
{
    public partial class Form1 : Form
    {
        #region Dependencies

        readonly Lazy<Querys> _querysLazy = new Lazy<Querys>(() => new Querys(_prefix));
        Querys QuerysLazy { get { return _querysLazy.Value; } }

        #endregion
        #region Props

        public int RowIndex  { get; set; }
        public string NameOfColumn { get; set; }
        public string[] TiposConceptosCuentaContable
        {
            get
            {
                return new[]{"B","D","X","Y"};
            }
        }
        public string[] TiposConceptosContraCuenta
        {
            get
            {
                return new[] { "A", "W"};
            }
        }
        private List<ErrorConceptos> ErrorConceptos { get; set; }
        private static List<CentroConcepto> GetCuentaContable { get; set; }
        private static readonly Thread MyThread = new Thread(NewThread);
        private static bool _isLoaded;
        private static int percentage = 0;
        private static int totalNumber = 0;
        private static string _prefix = "";
        private static string _excludeValue = "";
        #endregion
        #region Constants

        private const string ConstantCuentaContable = "CuentaContable";
        private const string ConstantContraCuenta = "ContraCuenta";
        private const string ConstantDeseaContinuar = "Seguro Que Desea Continuar?";
        private const string ConstantConfirmarParaAsociarCuentas = "Confirmar Para Asociar Cuentas";

        #endregion

        public Form1(string value, string excludeValue)
        {
            InitializeComponent();
            _prefix = value;
            _excludeValue = excludeValue;
            MyThread.Start();
        }

        static void NewThread()
        {
            GetCuentaContable = new Querys(_prefix).GetCuentaContable(_excludeValue);
            _isLoaded = true;
            MyThread.Abort();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = QuerysLazy.GetListOfConceptoBytipo();
            dataGridView1.Columns[4].Visible = true;
            dataGridView1.Columns[0].Width = 60;
            dataGridView1.Columns[0].HeaderText = "";
            dataGridView1.Show();
            progressBar1.Value = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells[1].ReadOnly = row.Cells[2].ReadOnly = row.Cells[3].ReadOnly = row.Cells[3].ReadOnly = row.Cells[4].ReadOnly = true;
            }

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                var dataGridViewColumn = dataGridView1.Columns[column.Name];
                if (dataGridViewColumn != null)
                    dataGridViewColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == -1 || e.RowIndex == -1) return;
            NameOfColumn = dataGridView1.Columns[e.ColumnIndex].Name;
            if (NameOfColumn != ConstantCuentaContable && NameOfColumn != ConstantContraCuenta) return;
            var tipoCuenta = dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString();
            if (
                (TiposConceptosCuentaContable.Any(x => x == tipoCuenta) && NameOfColumn == ConstantCuentaContable) ||
                (TiposConceptosContraCuenta.Any(x => x == tipoCuenta))
                )
            {
                RowIndex = e.RowIndex;
                var form2 = new SeleccionarCuentas(_prefix);
                form2.UpdateTextBox += txtBox_TextChanged;
                form2.ShowDialog(this);
            }
            else
            {
                MessageBox.Show(@"La cuenta seleccionada no permite movimientos.", @"Informacion",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }

        private void txtBox_TextChanged(object sender, TextChangeEventArgs e)
        {
            dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridView1.Rows[RowIndex].Cells[NameOfColumn].Value = e.PrpStrDataToPass;
            dataGridView1.EndEdit();
            dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;

        }

        private void asociarCuentasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_isLoaded)
            {
                MessageBox.Show(@"Los Datos No Han Sido Cargado, Espere 4 Segundos Aproximandamente",
                        @"Los Datos No Han Sido Cargado, Espere 4 Segundos Aproximandamente", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                return;
            }
            var cuentaContable = GetCuentaContable;
            var estadosA = cuentaContable.Where(x => x.Estado == "A").ToList();
            var estadosNoA = cuentaContable.Where(x => x.Estado != "A").ToList();
            if (MessageBox.Show(ConstantDeseaContinuar, ConstantConfirmarParaAsociarCuentas, MessageBoxButtons.YesNo) !=
                DialogResult.Yes) return;

            dataGridView1.EndEdit();
            ErrorConceptos = new List<ErrorConceptos>();
            var conceptoList = (List<CONCEPTO>)dataGridView1.DataSource;
            var length = 0;
            progressBar1.Value = 10;
            foreach (var concepto in conceptoList.Where(concepto => concepto.IsChecked))
            {
                length++;
                if (TiposConceptosCuentaContable.Any(x => x == concepto.TipoConcepto)) UpdateConceptoCuentaContable(concepto, estadosA, estadosNoA);
                else UpdateConceptoContraCuenta(concepto, estadosA, estadosNoA);
                if (progressBar1.Value > 100)
                {
                    progressBar1.Value += 5;
                }
            }
            progressBar1.Value = 90;
            if (length > 0)
            {
                if (ErrorConceptos.Count > 0)
                {
                    var errorFrom = new ErrorListForm(ErrorConceptos);
                    ErrorConceptos = new List<ErrorConceptos>();
                    errorFrom.ShowDialog(this);
                }
                else
                {
                    MessageBox.Show(@"Los Datos Han Sido Actualizados Satisfactoriamente",
                        @"Los Datos Han Sido Actualizados Satisfactoriamente", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                progressBar1.Value = 100;
            }
            else MessageBox.Show(@"Por Favor Modifique Al Menos Un Concepto", @"Mensaje",MessageBoxButtons.OK,
                       MessageBoxIcon.Information);

            progressBar1.Value = 0;
            
        }

        private void UpdateConceptoCuentaContable(CONCEPTO concepto, List<CentroConcepto> estadosA, List<CentroConcepto> estadosNoA)
        {
            if (string.IsNullOrEmpty(concepto.CuentaContable)) return;
            if (!string.IsNullOrEmpty(concepto.ContraCuenta))
            {
                ErrorConceptos.Add(new ErrorConceptos
                {
                    Concepto = concepto.Concepto,
                    Descripcion = @"Contra Cuenta No debe ser asignada"
                });
            }   
            else
            {
                var result = QuerysLazy.UpdateCuentaContable(concepto, estadosA, estadosNoA);
                if (result.Any(x => x.CentroCosto == Constants.DefaultCentroCosto))
                {
                    QuerysLazy.UPSERTConceptoIn_Relacion_CENTRO_CUENTA(concepto.Concepto, concepto.CuentaContable, concepto.ContraCuenta); 
                }

                foreach (var results in result.Where(x => x.CentroCosto != Constants.DefaultCentroCosto))
                {
                    ErrorConceptos.Add(new ErrorConceptos
                    {
                        Concepto = concepto.Concepto,
                        Descripcion = @"Para el concepto " + concepto.Concepto + " y el centro de costo " + results.CentroCosto + ", no se pudo actualizar la cuenta debido a que el centro de costo no está asociado a la cuenta contable o bien su relación se encuentra inactiva en la contabilidad."
                    });
                }
            }
        }

        private void UpdateConceptoContraCuenta(CONCEPTO concepto, List<CentroConcepto> estadosA, List<CentroConcepto> estadosNoA)
        {
            if (!string.IsNullOrEmpty(concepto.CuentaContable))
            {
                if (!string.IsNullOrEmpty(concepto.ContraCuenta))
                {
                    var result = QuerysLazy.UpdateContraCuenta(concepto, estadosA, estadosNoA);
                    if (result.Any(x=> x.CentroCosto == Constants.DefaultCentroCosto))
                    {
                        QuerysLazy.UPSERTConceptoIn_Relacion_CENTRO_CUENTA(concepto.Concepto, concepto.CuentaContable, concepto.ContraCuenta);
                    }

                        foreach (var results in result.Where(x => x.CentroCosto != Constants.DefaultCentroCosto))
                        {
                            ErrorConceptos.Add(new ErrorConceptos
                            {
                                Concepto = concepto.Concepto,
                                Descripcion = @"Para el concepto " + concepto.Concepto + " y el centro de costo "+results.CentroCosto+", no se pudo actualizar la cuenta debido a que el centro de costo no está asociado a la cuenta contable o bien su relación se encuentra inactiva en la contabilidad."
                            });
                        }
                }
                else
                {
                    ErrorConceptos.Add(new ErrorConceptos
                    {
                        Concepto = concepto.Concepto,
                        Descripcion = @"Contra Cuenta Debe Ser Asignada"
                    });
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(concepto.ContraCuenta))
                {
                    ErrorConceptos.Add(new ErrorConceptos
                    {
                        Concepto = concepto.Concepto,
                        Descripcion = @"Cuenta Contable Debe Ser Asignada"
                    });
                }
                else
                {
                    ErrorConceptos.Add(new ErrorConceptos
                    {
                        Concepto = concepto.Concepto,
                        Descripcion = @"Cuenta Contable Y Contra Cuenta Deben Ser Asignada"
                    });
                }
            }
        }

        private void marcarTodoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool ischecked;
            if (marcarTodoToolStripMenuItem.Text == @"Marcar Todo")
            {
                ischecked = true;
                marcarTodoToolStripMenuItem.Text = @"Desmarcar";
            }
            else
            {
                ischecked = false;
                marcarTodoToolStripMenuItem.Text = @"Marcar Todo";
            }
            var conceptoList = (List<CONCEPTO>) dataGridView1.DataSource;
            for (var i = 0; i < conceptoList.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = ischecked;
            }
        }

        private void verificacionCentroCuentaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_isLoaded)
            {
                MessageBox.Show(@"Los Datos No Han Sido Cargado, Espere 4 Segundos Aproximandamente",
                    @"Los Datos No Han Sido Cargado, Espere 4 Segundos Aproximandamente", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            progressBar1.Value = 10;
            var cuentaContable = GetCuentaContable;
            var estadosA = cuentaContable.Where(x => x.Estado == "A").ToList();
            var estadosNoA = cuentaContable.Where(x => x.Estado != "A").ToList();
            dataGridView1.EndEdit();
            ErrorConceptos = new List<ErrorConceptos>();
            var conceptoList = (List<CONCEPTO>)dataGridView1.DataSource;
            var list = conceptoList.Where(concepto => concepto.IsChecked).ToList();
            totalNumber = list.Count;
            foreach (var concepto in list)
            {
                percentage += 1;
                progressBar1.Value = (totalNumber / percentage ) * 10;
                var result = QuerysLazy.VerificarCentroConceptos(concepto, estadosA, estadosNoA);
                foreach (var results in result.Where(x => x.CentroCosto != Constants.DefaultCentroCosto))
                {
                    
                    ErrorConceptos.Add(new ErrorConceptos
                    {
                        Concepto = concepto.Concepto,
                        Descripcion = @"Para el concepto " + concepto.Concepto + " y el centro de costo " + results.CentroCosto + ", no se pudo actualizar la cuenta debido a que el centro de costo no está asociado a la cuenta contable o bien su relación se encuentra inactiva en la contabilidad."
                    });
                }
            }
            progressBar1.Value = 100;
                if (ErrorConceptos.Count > 0)
                {
                    var errorFrom = new ErrorListForm(ErrorConceptos);
                    ErrorConceptos = new List<ErrorConceptos>();
                    errorFrom.Show();
                    percentage = 0;
                }
                else
                {
                    MessageBox.Show(@"Todas Las Relaciones Centro - Cuenta Son Existosas",
                        @"Todas Las Relaciones Centro - Cuenta Son Existosas", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            
        }

        private void process1_Exited(object sender, EventArgs e)
        {
            progressBar1.Value =  (percentage / 100) * totalNumber;
        }


    }

    public class TextChangeEventArgs : EventArgs
    {
        private readonly string _strDataToPass;

        public TextChangeEventArgs(string text)
        {
            _strDataToPass = text;
        }

        public string PrpStrDataToPass
        {
            get { return _strDataToPass; }
        }
    }
}
