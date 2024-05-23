using GeneradorCufe.Model;
using GeneradorCufe.ViewModel;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Consultas
{
    public class Emisor_Consulta
    {
        private readonly Conexion.Data _data;
        private readonly InvoiceViewModel _invoiceViewModel;

        public Emisor_Consulta()
        {
            _data = new Conexion.Data("MySqlConnectionString");
          
            InvoiceViewModel invoiceViewModel = new InvoiceViewModel();
        }
        public void EjecutarAccionParaIP(Factura factura, string usuario, string contraseña)
        {
            try
            {
                string connectionString = $"Database=empresas; Data Source={factura.Ip_base}; User Id={usuario}; Password={contraseña}; ConvertZeroDateTime=True;";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"SELECT emprnombr, emprnit, emprtipo, emprdirec, emprciuda, empregim_x, emprperson, empremail, emprtelef, empretiene, empr_urlx, emprcity, logo, emprperson FROM empresas WHERE emprobra = @empresa";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@empresa", factura.Empresa);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            foreach (DataRow row in dataTable.Rows)
                            {
                                byte[] logoBytes = (byte[])row["logo"];
                                string Logo_emisor = Convert.ToBase64String(logoBytes);

                                Emisor emisor = new Emisor
                                {
                                    Nombre_emisor = row["emprnombr"].ToString() ?? "",
                                    Nit_emisor = row["emprnit"].ToString() ?? "",
                                    Codigo_departamento_emisor = row["emprtipo"].ToString() ?? "",
                                    Direccion_emisor = row["emprdirec"].ToString() ?? "",
                                    Nombre_municipio_emisor = row["emprciuda"].ToString() ?? "",
                                    Responsable_emisor = row["empregim_x"].ToString() ?? "",
                                    Tipo_emisor = Convert.ToDecimal(row["emprperson"] ?? 0),
                                    Correo_emisor = row["empremail"].ToString() ?? "",
                                    Telefono_emisor = row["emprtelef"].ToString() ?? "",
                                    Retiene_emisor = Convert.ToDecimal(row["empretiene"] ?? 0),
                                    Url_emisor = row["empr_urlx"].ToString() ?? "",
                                    Ciudad_emisor = row["emprcity"].ToString() ?? "",
                                    Logo_emisor = Logo_emisor,
                                    Regimen_emisor = Convert.ToDecimal(row["emprperson"] ?? 0) == 1 ? "Natural" :
                                    Convert.ToDecimal(row["emprperson"] ?? 0) == 2 ? "Jurídica" : ""
                                };

                                InvoiceViewModel.EjecutarGeneracionXML(emisor, factura);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConError(factura, ex);
            }
        }


    }
}
