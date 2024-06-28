using GeneradorCufe.Conexion;
using GeneradorCufe.Model;
using GeneradorCufe.ViewModel;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace GeneradorCufe.Consultas
{
    public class Factura_Consulta
    {
        private readonly Data _data;
        private readonly System.Timers.Timer _timer;
        private readonly object _lock = new object();

        public Factura_Consulta()
        {
            _data = new Data();
            _timer = new System.Timers.Timer();
            _timer.Interval = 15000; // Intervalo en milisegundos (15 segundos)
            _timer.Elapsed += TimerElapsed; // Método que se ejecutará cuando el temporizador expire
            _timer.Start(); // Iniciar el temporizador
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)  // Método que se ejecuta cada vez que el temporizador expire
        {
            ConsultarBaseDatos();
        }


        private void ConsultarBaseDatos()
        {
            lock (_lock)
            {
                try
                {
                    using (MySqlConnection connection = _data.CreateConnection())
                    {
                        connection.Open();
                        using (MySqlTransaction transaction = connection.BeginTransaction())
                        {
                            string query = @"
                    SELECT id_enc, empresa, tipo_mvt, factura, recibo, aplica, nombre3, notas, estado, terminal 
                    FROM fac 
                    WHERE estado IN (7, 8) 
                    AND (terminal IS NOT NULL AND terminal <> '') 
                    FOR UPDATE";

                            using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
                            {
                                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                                {
                                    DataTable dataTable = new DataTable();
                                    adapter.Fill(dataTable);

                                    List<int> idsActualizados = new List<int>();

                                    foreach (DataRow row in dataTable.Rows)
                                    {
                                        int idEncabezado = Convert.ToInt32(row["id_enc"]);
                                        idsActualizados.Add(idEncabezado);
                                    }

                                    if (idsActualizados.Count > 0)
                                    {
                                        // Marcar todas las facturas seleccionadas como en proceso (estado = 1) en la base de datos
                                        string updateQuery = "UPDATE fac SET estado = 1 WHERE id_enc IN (" + string.Join(",", idsActualizados) + ")";
                                        using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection, transaction))
                                        {
                                            updateCommand.ExecuteNonQuery();
                                        }
                                    }

                                    transaction.Commit();

                                    List<Factura> facturas = new List<Factura>();
                                    foreach (DataRow row in dataTable.Rows)
                                    {
                                        int idEncabezado = Convert.ToInt32(row["id_enc"]);
                                        Factura factura = ProcesarDatosFactura(row);
                                        facturas.Add(factura);
                                    }

                                    // Procesar todas las facturas en bloque
                                    ProcesarRegistros(facturas);
                                }
                            }
                        }
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    // Manejar el error adecuadamente
                }
            }
        }


        private Factura ProcesarDatosFactura(DataRow row)
        {
            Factura factura = new Factura
            {
                Id_encabezado = (row["id_enc"] == DBNull.Value) ? 0 : Convert.ToInt32(row["id_enc"]),
                Empresa = row["empresa"]?.ToString() ?? "",
                Tipo_movimiento = row["tipo_mvt"]?.ToString() ?? "",
                Facturas = row["factura"]?.ToString() ?? "",
                Recibo = row["recibo"]?.ToString() ?? "",
                Aplica = row["aplica"]?.ToString() ?? "",
                Nombre = row["nombre3"]?.ToString() ?? "",
                Notas = row["notas"]?.ToString() ?? "",
                Estado = (row["estado"] == DBNull.Value) ? 0 : Convert.ToInt32(row["estado"]),
                Terminal = row["terminal"]?.ToString() ?? "",
               // Ip_base = row["ip_base"]?.ToString() ?? ""
            };

            return factura;
        }

        private void ProcesarRegistros(List<Factura> facturas)
        {
            Emisor_Consulta emisorConsulta = new Emisor_Consulta();

            foreach (Factura factura in facturas)
            {
                try
                {
                  
                        emisorConsulta.EjecutarAccionParaIP(factura, "root", "**qwerty**");

                }
                catch (Exception ex)
                {
                  //  MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    MarcarComoConError(factura, ex);
                }
            }
        }



        public void MarcarComoConError(Factura factura, Exception ex)
        {
            try
            {
                using (MySqlConnection connection = _data.CreateConnection())
                {
                    connection.Open();

                    // Construir la consulta para actualizar el estado del registro con estado 5 y el mensaje de error
                    string query = "UPDATE fac SET estado = 5, msm_error = @mensajeError, dato_qr = @Respuesta WHERE id_enc = @idEncabezado";

                    string detalle = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string mensajeError = ex.Message; // Solo el mensaje de la excepción
                    string Respuesta = $"[{{\"estado\":{{\"codigo\":\"Error en el envio\"}},\"detalle\":\"{mensajeError}\"}}]";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Agregar parámetros a la consulta
                        command.Parameters.AddWithValue("@idEncabezado", factura.Id_encabezado);
                        command.Parameters.AddWithValue("@Respuesta", Respuesta);
                        command.Parameters.AddWithValue("@mensajeError", mensajeError); // Agregar el mensaje de error

                        // Ejecutar la consulta
                        int rowsAffected = command.ExecuteNonQuery();

                        // Verificar si se actualizó algún registro
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Registro con Id_encabezado {factura.Id_encabezado} marcado como error. Mensaje de error: {mensajeError}");
                        }
                        else
                        {
                            Console.WriteLine($"No se encontró ningún registro con Id_encabezado {factura.Id_encabezado} y estado 0.");
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al marcar como error el registro con Id_encabezado {factura.Id_encabezado}: {e.Message}");
            }
        }


        public void MarcarComoConErrorPDF(Factura factura, Exception ex)
        {
            try
            {
                using (MySqlConnection connection = _data.CreateConnection())
                {
                    connection.Open();

                    // Construir la consulta para actualizar el estado del registro con estado 5 y el mensaje de error
                    string query = "UPDATE fac SET estado = 5, msm_error = @mensajeError WHERE id_enc = @idEncabezado";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Agregar parámetros a la consulta
                        command.Parameters.AddWithValue("@idEncabezado", factura.Id_encabezado);
                        command.Parameters.AddWithValue("@mensajeError","PDF" + ex.Message); // Agregar el mensaje de error

                        // Ejecutar la consulta
                        int rowsAffected = command.ExecuteNonQuery();

                        // Verificar si se actualizó algún registro
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Registro con Id_encabezado {factura.Id_encabezado} marcado como error. Mensaje de error: {ex.Message}");
                        }
                        else
                        {
                            Console.WriteLine($"No se encontró ningún registro con Id_encabezado {factura.Id_encabezado} y estado 0.");
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al marcar como error el registro con Id_encabezado {factura.Id_encabezado}: {e.Message}");
            }
        }

        public void MarcarComoConErrorCorreo(Factura factura, Exception ex)
        {
            try
            {

                // Actualizar el estado en la base de datos
                using (MySqlConnection connection = _data.CreateConnection())
                {
                    connection.Open();

                    // Construir la consulta para actualizar el estado del registro con estado 5 y el mensaje de error
                    string query = "UPDATE fac SET estado = 3, msm_error = @mensajeError WHERE id_enc = @idEncabezado";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Agregar parámetros a la consulta
                        command.Parameters.AddWithValue("@idEncabezado", factura.Id_encabezado);
                        command.Parameters.AddWithValue("@mensajeError", ex.Message); // Agregar el mensaje de error

                        // Ejecutar la consulta
                        int rowsAffected = command.ExecuteNonQuery();

                        // Verificar si se actualizó algún registro
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Registro con Id_encabezado {factura.Id_encabezado} marcado como error. Mensaje de error: {ex.Message}");
                        }
                        else
                        {
                            Console.WriteLine($"No se encontró ningún registro con Id_encabezado {factura.Id_encabezado} y estado 0.");
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al marcar como error el registro con Id_encabezado {factura.Id_encabezado}: {e.Message}");
            }
        }

        public void MarcarComoConErrorATTAs(Factura factura, Exception ex)
        {
            try
            {

                using (MySqlConnection connection = _data.CreateConnection())
                {
                    connection.Open();

                    // Construir la consulta para actualizar el estado del registro con estado 5 y el mensaje de error
                    string query = "UPDATE fac SET estado = 2, msm_error = @mensajeError WHERE id_enc = @idEncabezado";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Agregar parámetros a la consulta
                        command.Parameters.AddWithValue("@idEncabezado", factura.Id_encabezado);
                        command.Parameters.AddWithValue("@mensajeError", "Error a consultar el Attasdocumnet" + ex); // Agregar el mensaje de error

                        // Ejecutar la consulta
                        int rowsAffected = command.ExecuteNonQuery();

                        // Verificar si se actualizó algún registro
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Registro con Id_encabezado {factura.Id_encabezado} marcado como error. Mensaje de error: {ex.Message}");
                        }
                        else
                        {
                            Console.WriteLine($"No se encontró ningún registro con Id_encabezado {factura.Id_encabezado} y estado 0.");
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al marcar como error el registro con Id_encabezado {factura.Id_encabezado}: {e.Message}");
            }
        }

        public void MarcarComoConErrorATTAS(Factura factura)
        {
            try
            {
                using (MySqlConnection connection = _data.CreateConnection())
                {
                    connection.Open();

                    // Construir la consulta para actualizar el estado del registro con estado 5 y el mensaje de error
                    string query = "UPDATE fac SET estado = 2, msm_error = @mensajeError WHERE id_enc = @idEncabezado";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Agregar parámetros a la consulta
                        command.Parameters.AddWithValue("@idEncabezado", factura.Id_encabezado);
                        command.Parameters.AddWithValue("@mensajeError", "Error a consultar el Attasdocumnet"); // Agregar el mensaje de error

                        // Ejecutar la consulta
                        int rowsAffected = command.ExecuteNonQuery();

                        // Verificar si se actualizó algún registro
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Registro con Id_encabezado {factura.Id_encabezado} marcado como error. Mensaje de error:");
                        }
                        else
                        {
                            Console.WriteLine($"No se encontró ningún registro con Id_encabezado {factura.Id_encabezado} y estado 0.");
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al marcar como error el registro con Id_encabezado {factura.Id_encabezado}: {e.Message}");
            }
        }


       
    }
}

