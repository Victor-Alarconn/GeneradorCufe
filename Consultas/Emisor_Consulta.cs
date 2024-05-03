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
                // Construir la cadena de conexión utilizando la IP, usuario y contraseña proporcionados
                string connectionString = $"Database=empresas; Data Source={factura.Ip_base}; User Id={usuario}; Password={contraseña}; ConvertZeroDateTime=True;";
       
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    // Abrir la conexión
                    connection.Open();

                    string query = $"SELECT emprnombr, emprnit, emprtipo, emprdirec, emprciuda, empregim_x, emprperson, empremail, emprtelef, empretiene, empr_urlx, emprcity FROM empresas WHERE emprobra = @empresa";

                    // Crear un nuevo comando SQL con la consulta y la conexión
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Agregar el parámetro 'empresa' a la consulta SQL para evitar la inyección de SQL
                        command.Parameters.AddWithValue("@empresa", factura.Empresa);

                        // Crear un adaptador de datos y un DataTable para almacenar los resultados de la consulta
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            foreach (DataRow row in dataTable.Rows)
                            {
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
                                    Ciudad_emisor = row["emprcity"].ToString() ?? ""
                                };

                                // Llamada al método desde dentro del bucle
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
