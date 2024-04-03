using GeneradorCufe.Model;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
                // Decodificar el documento base64 a formato XML
                byte[] documentBytes = Convert.FromBase64String(documentoBase64);
                string xmlContent = Encoding.UTF8.GetString(documentBytes);

                // Convertir el XML a formato JSON
                string jsonContent = JsonConvert.SerializeXNode(XDocument.Parse(xmlContent), Formatting.Indented);

                // Encontrar la posición del inicio del JSON deseado
                int startIndex = jsonContent.IndexOf("\"cbc:UBLVersionID\": \"UBL 2.1\"");

                // Verificar si se encontró el inicio del JSON deseado
                if (startIndex >= 0)
                {
                    // Recortar el JSON desde el inicio deseado
                    jsonContent = jsonContent.Substring(startIndex);

                    using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                    {
                        connection.Open();

                        // Definir la consulta SQL para actualizar la tabla xxxxccfc solo cuando el valor de factura coincida
                        string updateQuery = "UPDATE xxxxccfc SET estado_fe = 3, dato_qr = @DocumentoJson WHERE factura = @Factura";

                        using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                        {
                            // Asignar los valores de los parámetros DocumentoJson y Factura
                            updateCommand.Parameters.AddWithValue("@DocumentoJson", jsonContent);
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
                else
                {
                    Console.WriteLine("No se pudo encontrar el inicio del JSON deseado en el contenido.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar la respuesta de consulta en la base de datos: {ex.Message}");
            }
        }

        public void BorrarEnBD(string cadenaConexion, string factura)
        {
            try
            {
                // Crear un constructor de cadena de conexión MySQL para analizar la cadena de conexión existente
                MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder(cadenaConexion);

                // Cambiar el nombre de la base de datos en la cadena de conexión
                connectionStringBuilder.Database = "facturas";

                // Obtener la cadena de conexión modificada
                string nuevaCadenaConexion = connectionStringBuilder.ConnectionString;

                // Abrir una nueva conexión utilizando la cadena de conexión modificada
                using (MySqlConnection connection = new MySqlConnection(nuevaCadenaConexion))
                {
                    connection.Open();

                    // Definir la consulta SQL para borrar el archivo en la tabla fac donde factura sea igual al valor proporcionado
                    string deleteQuery = "DELETE FROM fac WHERE factura = @Factura";

                    using (MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection))
                    {
                        // Asignar el valor del parámetro Factura
                        deleteCommand.Parameters.AddWithValue("@Factura", factura);

                        // Ejecutar la consulta para borrar el archivo
                        int rowsAffected = deleteCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("El archivo se borró correctamente de la base de datos.");
                        }
                        else
                        {
                            Console.WriteLine("No se encontró ningún archivo con la factura especificada en la base de datos.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al borrar el archivo de la base de datos: {ex.Message}");
            }
        }

        public void GuardarErrorEnBD(string cadenaConexion, HttpStatusCode status, string mensaje, Factura factura)
        {
            try
            {
                // Concatenar el estado y el mensaje
                string estadoMensaje = $"{status}: {mensaje}";

                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    connection.Open();

                    // Definir la consulta SQL para actualizar la tabla xxxxccfc donde Facturas sea igual a factura.Facturas
                    string updateQuery = "UPDATE xxxxccfc SET dato_qr = @EstadoMensaje WHERE factura = @factura";

                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        // Asignar los valores de los parámetros EstadoMensaje y Factura
                        updateCommand.Parameters.AddWithValue("@EstadoMensaje", estadoMensaje);
                        updateCommand.Parameters.AddWithValue("@factura", factura.Facturas);

                        // Ejecutar la actualización
                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("Se actualizó correctamente el mensaje de error en la base de datos.");
                        }
                        else
                        {
                            Console.WriteLine("No se pudo actualizar el mensaje de error en la base de datos. No se encontró la factura especificada.");
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
