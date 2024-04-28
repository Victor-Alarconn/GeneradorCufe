using GeneradorCufe.Model;
using MySqlConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public bool GuardarRespuestaEnBD(string cadenaConexion, string documentoBase64, string factura, string cufe, string recibo)
        {
            try
            {
                string detalle = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string jsonRespuesta = $"[{{\"factura\":\"{recibo}\",\"cufe/cude\":\"{cufe}\",\"estado\":{{\"codigo\":\"Enviado Adquiriente\"}},\"detalle\":\"{detalle}\"}}]";

                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    connection.Open();

                    string updateQuery = string.Empty;

                    // Verificar si la condición se cumple
                    if (!string.IsNullOrEmpty(recibo) && recibo != "0")
                    {
                        // Si la condición se cumple, actualizar la tabla "xxxxcmbt"
                        updateQuery = "UPDATE xxxxcmbt SET estado_fe = 3, dato_qr = @DocumentoJson WHERE recibo = @Factura";
                    }
                    else
                    {
                        // Si la condición no se cumple, actualizar la tabla "xxxxccfc"
                        updateQuery = "UPDATE xxxxccfc SET estado_fe = 3, dato_qr = @DocumentoJson, dato_cufe = @Cufe WHERE factura = @Factura";
                    }

                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        // Asignar los valores de los parámetros DocumentoJson, Factura y CUFE
                        updateCommand.Parameters.AddWithValue("@DocumentoJson", jsonRespuesta);
                        updateCommand.Parameters.AddWithValue("@Cufe", cufe);
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
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar la respuesta de consulta en la base de datos: {ex.Message}");
                return false;
            }
        }



        public void BorrarEnBD(string cadenaConexion, string factura, string recibo)
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

                    // Definir la consulta SQL para borrar el archivo en la tabla fac
                    string deleteQuery;
                    if (!string.IsNullOrEmpty(recibo) && recibo != "0")
                    {
                        // Si es una nota de crédito, buscar por el valor de recibo en lugar de factura
                        deleteQuery = "DELETE FROM fac WHERE recibo = @Recibo";
                    }
                    else
                    {
                        deleteQuery = "DELETE FROM fac WHERE factura = @Factura";
                    }

                    using (MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection))
                    {
                        // Asignar el valor del parámetro correspondiente
                        if (!string.IsNullOrEmpty(recibo) && recibo != "0")
                        {
                            deleteCommand.Parameters.AddWithValue("@Recibo", recibo);
                        }
                        else
                        {
                            deleteCommand.Parameters.AddWithValue("@Factura", factura);
                        }

                        // Ejecutar la consulta para borrar el archivo
                        int rowsAffected = deleteCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("El archivo se borró correctamente de la base de datos.");
                        }
                        else
                        {
                            Console.WriteLine("No se encontró ningún archivo con la factura/recibo especificada en la base de datos.");
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

                    string updateQuery = string.Empty;

                    // Verificar si la condición se cumple
                    if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
                    {
                        // Si la condición se cumple, actualizar la tabla "xxxxcmbt"
                        updateQuery = "UPDATE xxxxcmbt SET dato_qr = @EstadoMensaje WHERE factura = @Factura";
                    }
                    else
                    {
                        // Si la condición no se cumple, actualizar la tabla "xxxxccfc"
                        updateQuery = "UPDATE xxxxccfc SET dato_qr = @EstadoMensaje WHERE factura = @Factura";
                    }

                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        // Asignar los valores de los parámetros EstadoMensaje y Factura
                        updateCommand.Parameters.AddWithValue("@EstadoMensaje", estadoMensaje);
                        updateCommand.Parameters.AddWithValue("@Factura", factura.Facturas);

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



        public void ConsultarFacturaEnBD(string cadenaConexion, string factura)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    connection.Open();

                    // Definir la consulta SQL para consultar la tabla xxxxccfc
                    string selectQuery = "SELECT dato_cufe, fecha FROM xxxxccfc WHERE factura = @Factura";

                    using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection))
                    {
                        // Asignar el valor del parámetro Factura
                        selectCommand.Parameters.AddWithValue("@Factura", factura);

                        // Ejecutar la consulta y leer los resultados
                        using (MySqlDataReader reader = selectCommand.ExecuteReader())
                        {
                            // Verificar si se encontraron resultados
                            if (reader.Read())
                            {
                                // Obtener los valores de la columna dato_cufe y fecha
                                string datoCufe = reader["dato_cufe"].ToString();
                                DateTime fecha = Convert.ToDateTime(reader["fecha"]);

                            }
                            else
                            {
                                Console.WriteLine("No se encontró la factura especificada en la base de datos.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al consultar en la base de datos: {ex.Message}");
            }
        }


    }
}
