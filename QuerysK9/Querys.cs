using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAL;
using DTOK9;

namespace QuerysK9
{
    public class Querys
    {
        public Querys(string value)
        {
            PreFFix = value;
        }
        #region Dependencies
        readonly Lazy<Connection> _cnn = new Lazy<Connection>(() => new Connection());
        Connection Cnn { get { return _cnn.Value; } }

        #endregion
        #region Props

        private List<CONCEPTO> ListOfConceptos { get; set; }
        private List<Cuentas> ListOfCuentas { get; set; }
        private List<CENTRO_COSTO> ListOfCentroCosto { get; set; }

        #endregion
        #region Constants

        //public static string PreFFix;
        public  static string PreFFix { get; set; }
        #endregion

        #region Querys

        public List<CONCEPTO> GetListOfConceptoBytipo()
        {
            var read = Cnn.GetReader("SELECT	c.CONCEPTO, " +
                                                "c.DESCRIPCION, " +
                                                "c.TIPO_CONCEPTO, " +
                                                "rcc.CuentaContable, " +
                                                "rcc.ContraCuenta " +
                                     "FROM	" + PreFFix + ".CONCEPTO c " +
                                     "LEFT JOIN " + PreFFix + ".Relacion_CENTRO_CUENTA rcc ON rcc.Concepto = c.CONCEPTO " +
                                     "WHERE	TIPO_CONCEPTO IN ('B','D','A','X','Y','W') " +
                                     "order	by c.CONCEPTO;");
            ListOfConceptos = new List<CONCEPTO>();
            using (var reader = read.ExecuteReader())
            {
                while (reader.Read())
                {
                    ListOfConceptos.Add(new CONCEPTO
                    {
                        Concepto = reader["CONCEPTO"].ToString(),
                        Descripcion = reader["DESCRIPCION"].ToString(),
                        TipoConcepto = reader["TIPO_CONCEPTO"].ToString(),
                        ContraCuenta = reader["ContraCuenta"].ToString(),
                        CuentaContable = reader["CuentaContable"].ToString()
                    });
                }
            }
            return ListOfConceptos;
        }
        public List<Cuentas> GetListOfCuentasBytipo()
        {
            var read = Cnn.GetReader("SELECT CUENTA_CONTABLE,DESCRIPCION, ACEPTA_DATOS FROM " + PreFFix + ".CUENTA_CONTABLE order by CUENTA_CONTABLE;");
            ListOfCuentas = new List<Cuentas>();
            using (var reader = read.ExecuteReader())
            {
                while (reader.Read())
                {
                    ListOfCuentas.Add(new Cuentas
                    {
                        NumeroCuenta = reader["CUENTA_CONTABLE"].ToString(),
                        Descripcion = reader["DESCRIPCION"].ToString(),
                        ACEPTA_DATOS = reader["ACEPTA_DATOS"].ToString()
                    });
                }
            }
            return ListOfCuentas;
        }

        public List<CentroConcepto> UpdateCuentaContable(CONCEPTO concepto, List<CentroConcepto> estadosA, List<CentroConcepto> estadosNoA)
        {
            var numberOfRecords = 0;
            var centroConcepto = CheckIfCentroConceptoHasAStatus(concepto.Concepto);
            var result = (from estado in estadosNoA
                          join centro in centroConcepto on estado.CentroCostoCuentaContable equals
                              centro.CentroCostoCuentaContable
                          select estado).ToList();
            if (estadosA.Count <= 0) return result;


            //var resultNo = estadosA.Where(a => !centroConcepto.Select(b => b.CentroCostoCuentaContable).Contains(a.CentroCostoCuentaContable)).ToList();

            var query = "UPDATE " + PreFFix +
                                  ".CENTRO_CONCEPTO SET CUENTA_CONTABLE = @CuentaContable WHERE CONCEPTO = @Concepto AND (CENTRO_COSTO+CUENTA_CONTABLE) in ({0})";

            var centrosCuenta = (from estado in estadosA
                                 join centro in centroConcepto on estado.CentroCostoCuentaContable equals
                                     centro.CentroCostoCuentaContable
                                 select estado).ToList();
            var paramNames = centrosCuenta.Select(
                (s, index) => "@tag" + index.ToString()
                ).ToArray();

            string inClause = string.Join(",", paramNames);

            var read = Cnn.GetReader(string.Format(query, inClause));
            for (int i = 0; i < paramNames.Length; i++)
            {
                read.Parameters.AddWithValue(paramNames[i], centrosCuenta[i].CentroCostoCuentaContable);
            }
            try
            {
                read.Parameters.AddWithValue("@CuentaContable", concepto.CuentaContable);
                read.Parameters.AddWithValue("@Concepto", concepto.Concepto);
                numberOfRecords = read.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
            if (numberOfRecords != 0)
                result.Add(new CentroConcepto { CentroCosto = Constants.DefaultCentroCosto });
            //if (resultNo.Any())
                //result.AddRange(resultNo);
            return result; 
        }

        public List<CentroConcepto> UpdateContraCuenta(CONCEPTO concepto, List<CentroConcepto> estadosA, List<CentroConcepto> estadosNoA)
        {
            var numberOfRecords = 0;
            var centroConcepto = CheckIfCentroConceptoHasAStatus(concepto.Concepto);
            var result = (from estado in estadosNoA
                join centro in centroConcepto on estado.CentroCostoCuentaContable equals
                    centro.CentroCostoCuentaContable
                select estado).ToList();
            if (estadosA.Count <= 0) return result;

           // var resultNo = centroConcepto.Where(a => !estadosA.Select(b => b.CentroCostoCuentaContable).Contains(a.CentroCostoCuentaContable)).ToList();

            
            var query = "UPDATE " + PreFFix + ".CENTRO_CONCEPTO " +
                        "SET CUENTA_CONTABLE = @CuentaContable , " +
                        "CUENTA_CONTRA = @ContraCuenta " +
                        "WHERE CONCEPTO = @Concepto " +
                        "AND (CENTRO_COSTO+CUENTA_CONTABLE) in ({0})";

            var centrosCuenta = (from estado in estadosA
                join centro in centroConcepto on estado.CentroCostoCuentaContable equals
                    centro.CentroCostoCuentaContable
                select estado).ToList();
            var paramNames = centrosCuenta.Select(
                (s, index) => "@tag" + index.ToString()
                ).ToArray();

            string inClause = string.Join(",", paramNames);

            var read = Cnn.GetReader(string.Format(query, inClause));
            for (int i = 0; i < paramNames.Length; i++)
            {
                read.Parameters.AddWithValue(paramNames[i], centrosCuenta[i].CentroCostoCuentaContable);
            }
            try
            {
                read.Parameters.AddWithValue("@CuentaContable", concepto.CuentaContable);
                read.Parameters.AddWithValue("@ContraCuenta", concepto.ContraCuenta);
                read.Parameters.AddWithValue("@Concepto", concepto.Concepto);
                numberOfRecords = read.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
            if (numberOfRecords != 0)
                result.Add(new CentroConcepto { CentroCosto = Constants.DefaultCentroCosto});
            //if (resultNo.Any())
                //result.AddRange(resultNo);
            return result; 
        }

        public List<CentroConcepto> VerificarCentroConceptos(CONCEPTO concepto, List<CentroConcepto> estadosA, List<CentroConcepto> estadosNoA)
        {
            var centroConcepto = CheckIfCentroConceptoHasAStatus(concepto.Concepto);
            var result = (from estado in estadosNoA
                          join centro in centroConcepto on estado.CentroCostoCuentaContable equals
                              centro.CentroCostoCuentaContable
                          select estado).ToList();

            var resultNo = centroConcepto.Where(a => !estadosA.Select(b => b.CentroCostoCuentaContable).Contains(a.CentroCostoCuentaContable)).ToList();

            if (resultNo.Any())
            result.AddRange(resultNo);
            return result;
        }

        public List<CentroConcepto> CheckIfCentroConceptoHasAStatus(string concepto)
        {
            var arrayOfCuentas = new List<CentroConcepto>();
            var read = Cnn.GetReader("SELECT CENTRO_COSTO, CUENTA_CONTABLE FROM " + PreFFix + ".CENTRO_CONCEPTO WHERE CONCEPTO = @Concepto");
            read.Parameters.AddWithValue("@Concepto", concepto);
            using (var reader = read.ExecuteReader())
            {
                while (reader.Read())
                {
                    arrayOfCuentas.Add(new CentroConcepto
                    {
                        CentroCostoCuentaContable = reader["CENTRO_COSTO"] + reader["CUENTA_CONTABLE"].ToString(),
                        CuentaContable = reader["CUENTA_CONTABLE"].ToString(),
                        CentroCosto = reader["CENTRO_COSTO"].ToString()
                    });
                }
            }


            return arrayOfCuentas;

        }

        public void UPSERTConceptoIn_Relacion_CENTRO_CUENTA(string concepto, string cuentaContable, string contraCuenta)
        {
            var read = Cnn.GetReader("SELECT 1  FROM " + PreFFix + ".Relacion_CENTRO_CUENTA rcc WHERE rcc.Concepto = @Concepto");
            read.Parameters.AddWithValue("@Concepto", concepto);
            var query = string.Empty;
            using (var reader = read.ExecuteReader())
            {
                if (reader.Read())
                {
                    reader.Close();
                    query = "UPDATE	 " + PreFFix + ".Relacion_CENTRO_CUENTA " +
                            "SET     CuentaContable = @CuentaContable, " +
                                    "ContraCuenta = @ContraCuenta " +
                            "WHERE	 Concepto = @Concepto";
                }
                else
                {
                    reader.Close();
                    query = "INSERT INTO " + PreFFix + ".Relacion_CENTRO_CUENTA" +
                           "(" +
                           "Concepto," +
                           "CuentaContable," +
                           "ContraCuenta" +
                           ")" +
                           "VALUES" +
                           "(@Concepto," +
                           "@CuentaContable," +
                           "@ContraCuenta" +
                           ")";
                }
                read = Cnn.GetReader(query);
                read.Parameters.AddWithValue("@Concepto", concepto);
                read.Parameters.AddWithValue("@CuentaContable", cuentaContable);
                read.Parameters.AddWithValue("@ContraCuenta", contraCuenta);
                read.ExecuteReader();
            }
        }

        public List<CentroConcepto> GetCuentaContable(string excludeValue)
        {
            var value = !string.IsNullOrEmpty(excludeValue) ? "WHERE CENTRO_COSTO NOT LIKE '" + excludeValue+"%'" :"";
            var query = "SELECT CENTRO_COSTO, CUENTA_CONTABLE, ESTADO FROM " + PreFFix + ".CENTRO_CUENTA " + value;
            var cn = Cnn.GetReader(query);
            var cuentaContableList = new List<CentroConcepto>();
            using (var readerr = cn.ExecuteReader())
            {
                while (readerr.Read())
                {
                    cuentaContableList.Add(new CentroConcepto
                    {
                        CuentaContable = readerr["CUENTA_CONTABLE"].ToString(),
                        CentroCosto = readerr["CENTRO_COSTO"].ToString(),
                        CentroCostoCuentaContable = (readerr["CENTRO_COSTO"] + readerr["CUENTA_CONTABLE"].ToString()),
                        Estado = readerr["ESTADO"].ToString()
                    });
                }
            }




            return cuentaContableList;

        }

        /*Inactivación Masiva de Centros de Costo*/
        public List<CENTRO_COSTO> GetListOfCentro_Cuentas(string query)
        {
            var read = Cnn.GetReader("SELECT CENTRO_COSTO, DESCRIPCION FROM " + PreFFix + ".CENTRO_COSTO " + query + " order by CENTRO_COSTO;");
            ListOfCentroCosto = new List<CENTRO_COSTO>();
            using (var reader = read.ExecuteReader())
            {
                while (reader.Read())
                {
                    ListOfCentroCosto.Add(new CENTRO_COSTO
                    {
                        Centro_Costo = reader["CENTRO_COSTO"].ToString(),
                        Descripcion = reader["DESCRIPCION"].ToString()
                    });
                }
            }
            return ListOfCentroCosto;
        }
        public bool UpdateAsignarCentro_Cuentas(string codigo, string[] listaDeCuentas)
        {
            try
            {
                var query = @"UPDATE " + PreFFix +
                            ".CENTRO_CUENTA SET ESTADO = @Estado WHERE CENTRO_COSTO  in (@CENTRO_COSTO)";
                var sb = new StringBuilder();
                var i = 1;

                foreach (var cuenta in listaDeCuentas)
                {
                    sb.Append("@CENTRO_COSTO" + i);
                    if (listaDeCuentas.Length != i)
                    {
                        sb.Append(",");
                    }
                    i++;
                }
                var read = Cnn.GetReader(query.Replace("@CENTRO_COSTO", sb.ToString()));
                i = 1;
                foreach (var cuenta in listaDeCuentas)
                {
                    read.Parameters.AddWithValue("@CENTRO_COSTO" + i, cuenta);
                    i++;
                }
                read.Parameters.AddWithValue("@Estado", codigo);
                read.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion
    }
}
