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

                // Crear una nueva conexión a la base de datos utilizando la cadena de conexión construida
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    // Abrir la conexión
                    connection.Open();

                    // Define tu consulta SQL para seleccionar los campos deseados de la tabla empresas
                    string query = $"SELECT emprnombr, emprnit, emprtipo, emprdirec, emprciuda, empregim_x, emprperson, empremail, emprtelef FROM empresas WHERE emprobra = @empresa";

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
                                    Telefono_emisor = row["emprtelef"].ToString() ?? ""
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
                // Manejar cualquier excepción que pueda ocurrir durante la consulta
                Console.WriteLine("Error al ejecutar la acción para la IP: " + ex.Message);
            }
        }






        public Emisor ConsultarEmisor() // Consulta para obtener los datos del emisor
        {
            Emisor emisor = new Emisor();

            string query = "SELECT emprnombre, emprnit, emprdirec, emprciuda FROM empresas LIMIT 1"; // Limitamos a un solo registro

            using (MySqlConnection connection = _data.CreateConnection())
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            emisor.Nombre_emisor = reader["emprnombre"].ToString();
                            emisor.Nit_emisor = reader["emprnit"].ToString();
                            emisor.Direccion_emisor = reader["emprdirec"].ToString();
                            emisor.Nombre_municipio_emisor = reader["emprciuda"].ToString();
                            
                        }
                    }
                }
            }

            return emisor;
        }
    }
}
