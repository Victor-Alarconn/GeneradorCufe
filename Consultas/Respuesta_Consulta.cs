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
        public readonly Conexion.Data _data;

        public Respuesta_Consulta(Conexion.Data data)
        {
            _data = data;
        }

        public bool GuardarRespuestaEnBD(string cadenaConexion, string cufe, Factura factura1, Emisor emisor)
        {
            try
            {
                string idDocumento, codigoTipoDocumento, recibo, updateQuery;
                bool nota = !string.IsNullOrEmpty(factura1.Recibo) && factura1.Recibo != "0" && factura1.Tipo_movimiento == "NC";
                bool debito = !string.IsNullOrEmpty(factura1.Recibo) && factura1.Recibo != "0" && factura1.Tipo_movimiento == "ND";

                if (nota)
                {
                    recibo = factura1.Recibo;
                    idDocumento = recibo;
                    codigoTipoDocumento = "91";
                    cufe = emisor.cude;
                    updateQuery = "UPDATE xxxxcmbt SET estado_fe = 3, dato_qr = @DocumentoJson WHERE recibo = @Factura AND tipo = 'X'";
                }
                else if (debito)
                {
                    recibo = factura1.Recibo;
                    idDocumento = recibo;
                    codigoTipoDocumento = "92";
                    cufe = emisor.cude;
                    updateQuery = "UPDATE xxxxcmbt SET estado_fe = 3, dato_qr = @DocumentoJson WHERE recibo = @Factura AND tipo = 'W'";
                }
                else
                {
                    recibo = factura1.Facturas;
                    idDocumento = recibo;
                    codigoTipoDocumento = "01";
                    updateQuery = "UPDATE xxxxccfc SET estado_fe = 3, dato_qr = @DocumentoJson, dato_cufe = @Cufe WHERE factura = @Factura";
                }

                string detalle = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string jsonRespuesta = $"[{{\"factura\":\"{recibo}\",\"cufe/cude\":\"{cufe}\",\"estado\":{{\"codigo\":\"Enviado Adquiriente\"}},\"detalle\":\"{detalle}\"}}]";
                string cufe1 = "ENVIADO ADQUIRIENTE CUFE: " + cufe;

                using (MySqlConnection connection = _data.CreateConnection())
                {
                    connection.Open();

                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@DocumentoJson", jsonRespuesta);
                        updateCommand.Parameters.AddWithValue("@Cufe", cufe1);
                        updateCommand.Parameters.AddWithValue("@Factura", recibo);

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

                    // Añadir la actualización de la tabla "fac"
                    string updateFacQuery = "UPDATE fac SET estado = 4, dato_qr = @Cufe WHERE factura = @Factura AND empresa = @Empresa AND tipo_mvt = @Tipo";

                    using (MySqlCommand updateFacCommand = new MySqlCommand(updateFacQuery, connection))
                    {
                        updateFacCommand.Parameters.AddWithValue("@Cufe", cufe);
                        updateFacCommand.Parameters.AddWithValue("@Factura", factura1.Facturas);
                        updateFacCommand.Parameters.AddWithValue("@Empresa", factura1.Empresa);
                        updateFacCommand.Parameters.AddWithValue("@Tipo", factura1.Tipo_movimiento);

                        int rowsAffectedFac = updateFacCommand.ExecuteNonQuery();

                        if (rowsAffectedFac > 0)
                        {
                            Console.WriteLine("La tabla 'fac' se actualizó correctamente en la base de datos.");
                        }
                        else
                        {
                            Console.WriteLine("No se pudo actualizar la tabla 'fac' en la base de datos. No se encontró la factura especificada.");
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                new Factura_Consulta().MarcarComoConError(factura1, ex);
                return false;
            }
        }


        public void GuardarCufe(string cufe, Factura factura1)
        {
            try
            {
                using (MySqlConnection connection = _data.CreateConnection())
                {
                    connection.Open();

                    string updateFacQuery = "UPDATE fac SET estado = 1, dato_qr = @Cufe WHERE factura = @Factura AND empresa = @Empresa AND tipo_mvt = @Tipo";

                    using (MySqlCommand updateFacCommand = new MySqlCommand(updateFacQuery, connection))
                    {
                        updateFacCommand.Parameters.AddWithValue("@Cufe", cufe);
                        updateFacCommand.Parameters.AddWithValue("@Factura", factura1.Facturas);
                        updateFacCommand.Parameters.AddWithValue("@Empresa", factura1.Empresa);
                        updateFacCommand.Parameters.AddWithValue("@Tipo", factura1.Tipo_movimiento);

                        int rowsAffectedFac = updateFacCommand.ExecuteNonQuery();

                        if (rowsAffectedFac > 0)
                        {
                            Console.WriteLine("La tabla 'fac' se actualizó correctamente en la base de datos.");
                        }
                        else
                        {
                            Console.WriteLine("No se pudo actualizar la tabla 'fac' en la base de datos. No se encontró la factura especificada.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Factura_Consulta().MarcarComoConError(factura1, ex);
            }
        }



        public void BorrarEnBD(string cadenaConexion, string factura, string recibo, bool nota, Factura factura1)
        {
            try
            {
                // Abrir una nueva conexión utilizando la cadena de conexión definida en la clase
                using (MySqlConnection connection = _data.CreateConnection())
                {
                    connection.Open();

                    // Definir la consulta SQL para borrar el archivo en la tabla fac
                    string deleteQuery;
                    if (nota == true)
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
                        if (nota == true)
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
                            // Si se eliminó correctamente el registro de la base de datos, también lo eliminamos de la colección
                            Factura_Consulta facturaConsulta = new Factura_Consulta();
                            facturaConsulta.MarcarComoProcesado(factura1.Id_encabezado.Value);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConError(factura1, ex);
            }
        }
    



    public void GuardarErrorEnBD(string cadenaConexion, HttpStatusCode status, string mensaje, Factura factura)
        {
            try
            {
                // Concatenar el estado y el mensaje
                string estadoMensaje = $"{status}: {mensaje}";

                using (MySqlConnection connection = _data.CreateConnection())
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
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConError(factura, ex);
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
