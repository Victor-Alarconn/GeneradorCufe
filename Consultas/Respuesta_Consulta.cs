using GeneradorCufe.Model;
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

        public void GuardarRespuestaEnBD(string cadenaConexion, string documentoBase64, string factura)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    connection.Open();

                    // Definir la consulta SQL para actualizar la tabla xxxxccfc solo cuando el valor de factura coincida
                    string updateQuery = "UPDATE xxxxccfc SET estado_fe = 3, dato_qr = @DocumentoBase64 WHERE factura = @Factura";

                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        // Asignar los valores de los parámetros DocumentoBase64 y Factura
                        updateCommand.Parameters.AddWithValue("@DocumentoBase64", documentoBase64);
                        updateCommand.Parameters.AddWithValue("@Factura", factura);

                        // Ejecutar la actualización
                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("La respuesta de consulta se actualizó correctamente en la base de datos.");
                        }
                        else
                        {
                            Console.WriteLine("No se pudo actualizar la respuesta de consulta en la base de datos. No se encontró la factura especificada.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar la respuesta de consulta en la base de datos: {ex.Message}");
            }
        }

    }
}
