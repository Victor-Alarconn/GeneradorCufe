using GeneradorCufe.Model;
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

        public Emisor_Consulta()
        {
            _data = new Conexion.Data("MySqlConnectionString");
        }

        public void EjecutarAccionParaIP(string empresa, string ipBase, string usuario, string contraseña)
        {
            try
            {
                // Construir la cadena de conexión utilizando la IP, usuario y contraseña proporcionados
                string connectionString = $"Database={empresa}; Data Source={ipBase}; User Id={usuario}; Password={contraseña}; ConvertZeroDateTime=True;";

                // Crear una nueva conexión a la base de datos utilizando la cadena de conexión construida
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    // Abrir la conexión
                    connection.Open();

                    // Define tu consulta SQL
                    string query = "SELECT * FROM movimiento";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            // Procesa los resultados de la consulta aquí
                            foreach (DataRow row in dataTable.Rows)
                            {
                                
                            }
                        }
                    }

                    // Cierra la conexión
                    connection.Close();
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
