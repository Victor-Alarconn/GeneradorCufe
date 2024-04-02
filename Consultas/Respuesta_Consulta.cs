using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Consultas
{
    public class Respuesta_Consulta
    {
        private readonly Conexion.Data _data;

        public Respuesta_Consulta(Conexion.Data data)
        {
            _data = data;
        }

        public void GuardarRespuestaEnBD(string cadenaConexion, string documentoBase64)
        {
            try
            {
                // Conectarse a la base de datos MySQL
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    // Abrir la conexión
                    connection.Open();

                    // Definir la consulta SQL para insertar la respuesta en la base de datos
                    string query = "INSERT INTO RespuestasConsulta (DocumentoBase64) VALUES (@DocumentoBase64)";

                    // Crear el comando SQL con la consulta y la conexión
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Asignar el valor del parámetro DocumentoBase64
                        command.Parameters.AddWithValue("@DocumentoBase64", documentoBase64);

                        // Ejecutar el comando SQL
                        command.ExecuteNonQuery();
                    }
                }

                Console.WriteLine("Respuesta de consulta guardada en la base de datos correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar la respuesta de consulta en la base de datos: {ex.Message}");
            }
        }
    }
}
