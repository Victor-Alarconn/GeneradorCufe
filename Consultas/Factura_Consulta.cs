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
        private readonly Dictionary<int, EstadoProcesamiento> _registroProcesando;
        private readonly object _lock = new object();

        public Factura_Consulta()
        {
            _data = new Data();
            _timer = new System.Timers.Timer();
            _timer.Interval = 9000; // Intervalo en milisegundos (10 segundos)
            _timer.Elapsed += TimerElapsed; // Método que se ejecutará cuando el temporizador expire
            _timer.Start(); // Iniciar el temporizador
            _registroProcesando = new Dictionary<int, EstadoProcesamiento>();
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
                    Dictionary<int, EstadoProcesamiento> registroProcesando;

                    // Leer el archivo temporal si existe
                    if (File.Exists("registro_procesando.json"))
                    {
                        using (StreamReader reader = new StreamReader("registro_procesando.json"))
                        {
                            string json = reader.ReadToEnd();
                            registroProcesando = JsonConvert.DeserializeObject<Dictionary<int, EstadoProcesamiento>>(json);
                        }
                    }
                    else
                    {
                        // Si no existe el archivo, crear un nuevo diccionario
                        registroProcesando = new Dictionary<int, EstadoProcesamiento>();
                    }

                    using (MySqlConnection connection = _data.CreateConnection())
                    {
                        connection.Open();

                        string query = @"
                        SELECT id_enc, empresa, tipo_mvt, factura, recibo, aplica, nombre3, notas, estado, terminal 
                        FROM fac 
                        WHERE estado IN (0, 6) 
                        AND (terminal IS NOT NULL AND terminal <> '')";

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                            {
                                DataTable dataTable = new DataTable();
                                adapter.Fill(dataTable);

                                List<Factura> facturas = new List<Factura>(); // Lista para almacenar las facturas

                                // Llenar la lista de facturas con los registros de la base de datos
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    int idEncabezado = Convert.ToInt32(row["id_enc"]);

                                    // Verificar si el registro está en proceso
                                    if (!registroProcesando.ContainsKey(idEncabezado) || !registroProcesando[idEncabezado].Procesando)
                                    {
                                        // Marcar el registro como en proceso
                                        registroProcesando[idEncabezado] = new EstadoProcesamiento { Procesando = true, Intentos = 0, Envio = 0 };

                                        // Procesar los datos de la fila y agregar la factura a la lista
                                        Factura factura = ProcesarDatosFactura(row);
                                        facturas.Add(factura);
                                    }
                                }

                                // Serializar y guardar el diccionario actualizado
                                string jsonOutput = JsonConvert.SerializeObject(registroProcesando);
                                File.WriteAllText("registro_procesando.json", jsonOutput);

                                // Procesar todas las facturas en bloque
                                ProcesarRegistros(facturas);
                            }
                        }

                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                   // MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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



        public async Task ManejarIntentos(Emisor emisor, Factura factura, string cadenaConexion, string cufe, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado, HttpResponseMessage response)
        {
            try
            {
                // Cargar el diccionario desde el archivo temporal, si existe
                Dictionary<int, EstadoProcesamiento> registroProcesandoActualizado;

                if (File.Exists("registro_procesando.json"))
                {
                    using (StreamReader reader = new StreamReader("registro_procesando.json"))
                    {
                        string json = reader.ReadToEnd();
                        registroProcesandoActualizado = JsonConvert.DeserializeObject<Dictionary<int, EstadoProcesamiento>>(json);
                    }
                }
                else
                {
                    throw new FileNotFoundException("El archivo temporal 'registro_procesando.json' no se encontró. No se puede continuar sin este archivo.");
                }

                // Usar el diccionario cargado para acceder a los registros
                int idEncabezado = factura.Id_encabezado.Value;

                // Verificar si el registro está en proceso y actualizar los intentos
                if (registroProcesandoActualizado.ContainsKey(idEncabezado))
                {
                    registroProcesandoActualizado[idEncabezado].Intentos++;

                    int intentos = registroProcesandoActualizado[idEncabezado].Intentos;

                    // Definir el límite máximo de intentos
                    int maxIntentos = 2;

                    if (intentos <= maxIntentos)
                    {
                        // Implementar una política de reintento exponencial solo para esta acción específica
                        TimeSpan retardo = TimeSpan.FromSeconds(Math.Pow(2, intentos)); // Retardo exponencial

                        // Programar un nuevo intento después del retardo solo para esta acción específica
                        System.Timers.Timer timer = new System.Timers.Timer();
                        timer.Interval = 5000; // 5000 milisegundos = 5 segundos
                        timer.AutoReset = false; // No se reiniciará automáticamente
                        timer.Elapsed += async (sender, e) =>
                        {
                            try
                            {
                                // Aquí puedes llamar nuevamente a la acción específica que falló
                                await InvoiceViewModel.ConsultarXML(emisor, factura, cadenaConexion, cufe, listaProductos, adquiriente, movimiento, encabezado);
                            }
                            catch (Exception innerEx)
                            {
                                // Si hay un error al volver a intentar la acción, manejarlo
                               // await ManejarIntentos(emisor, factura, cadenaConexion, cufe, listaProductos, adquiriente, movimiento, encabezado, response);
                            }
                            finally
                            {
                                // Detener y liberar el temporizador después del intento
                                timer.Stop();
                                timer.Dispose();
                            }
                        };
                        timer.Start();
                    }
                    else
                    {
                        // Si se excede el límite de intentos, marcar el registro como con error
                        MarcarComoConError(factura, new SystemException());
                        registroProcesandoActualizado[idEncabezado].Procesando = false;
                    }
                }
                else
                {
                    throw new KeyNotFoundException($"No se encontró el registro con ID: {idEncabezado} en el archivo temporal 'registro_procesando.json'.");
                }

                // Guardar el diccionario actualizado en el archivo temporal
                string jsonOutput = JsonConvert.SerializeObject(registroProcesandoActualizado);
                File.WriteAllText("registro_procesando.json", jsonOutput);
            }
            catch (Exception ex)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConErrorATTAs(factura, ex);
            }
        }



        public void MarcarComoConError(Factura factura, Exception ex)
        {
            try
            {
                // Cargar el diccionario desde el archivo temporal, si existe
                Dictionary<int, EstadoProcesamiento> registroProcesandoActualizado = new Dictionary<int, EstadoProcesamiento>();

                if (File.Exists("registro_procesando.json"))
                {
                    using (StreamReader reader = new StreamReader("registro_procesando.json"))
                    {
                        string json = reader.ReadToEnd();
                        registroProcesandoActualizado = JsonConvert.DeserializeObject<Dictionary<int, EstadoProcesamiento>>(json);
                    }
                }
                else
                {
                    Console.WriteLine("El archivo temporal 'registro_procesando.json' no se encontró. No se puede continuar sin este archivo.");
                    return;
                }

                // Remover el registro del diccionario si existe
                if (registroProcesandoActualizado.ContainsKey(factura.Id_encabezado.Value))
                {
                    registroProcesandoActualizado.Remove(factura.Id_encabezado.Value);

                    // Guardar el diccionario actualizado en el archivo temporal
                    string jsonOutput = JsonConvert.SerializeObject(registroProcesandoActualizado);
                    File.WriteAllText("registro_procesando.json", jsonOutput);
                }
                else
                {
                    Console.WriteLine($"No se encontró el registro con Id_encabezado {factura.Id_encabezado} en el archivo temporal 'registro_procesando.json'.");
                }

                // Actualizar el estado en la base de datos
                using (MySqlConnection connection = _data.CreateConnection())
                {
                    connection.Open();

                    // Construir la consulta para actualizar el estado del registro con estado 5 y el mensaje de error
                    string query = "UPDATE fac SET estado = 5, msm_error = @mensajeError WHERE id_enc = @idEncabezado";

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

        public void MarcarComoConErrorCorreo(Factura factura, Exception ex)
        {
            try
            {
                // Cargar el diccionario desde el archivo temporal, si existe
                Dictionary<int, EstadoProcesamiento> registroProcesandoActualizado = new Dictionary<int, EstadoProcesamiento>();

                if (File.Exists("registro_procesando.json"))
                {
                    using (StreamReader reader = new StreamReader("registro_procesando.json"))
                    {
                        string json = reader.ReadToEnd();
                        registroProcesandoActualizado = JsonConvert.DeserializeObject<Dictionary<int, EstadoProcesamiento>>(json);
                    }
                }
                else
                {
                    Console.WriteLine("El archivo temporal 'registro_procesando.json' no se encontró. No se puede continuar sin este archivo.");
                    return;
                }

                // Remover el registro del diccionario si existe
                if (registroProcesandoActualizado.ContainsKey(factura.Id_encabezado.Value))
                {
                    registroProcesandoActualizado.Remove(factura.Id_encabezado.Value);

                    // Guardar el diccionario actualizado en el archivo temporal
                    string jsonOutput = JsonConvert.SerializeObject(registroProcesandoActualizado);
                    File.WriteAllText("registro_procesando.json", jsonOutput);
                }
                else
                {
                    Console.WriteLine($"No se encontró el registro con Id_encabezado {factura.Id_encabezado} en el archivo temporal 'registro_procesando.json'.");
                }

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
                // Cargar el diccionario desde el archivo temporal, si existe
                Dictionary<int, EstadoProcesamiento> registroProcesandoActualizado = new Dictionary<int, EstadoProcesamiento>();

                if (File.Exists("registro_procesando.json"))
                {
                    using (StreamReader reader = new StreamReader("registro_procesando.json"))
                    {
                        string json = reader.ReadToEnd();
                        registroProcesandoActualizado = JsonConvert.DeserializeObject<Dictionary<int, EstadoProcesamiento>>(json);
                    }
                }
                else
                {
                    Console.WriteLine("El archivo temporal 'registro_procesando.json' no se encontró. No se puede continuar sin este archivo.");
                    return;
                }

                // Remover el registro del diccionario si existe
                if (registroProcesandoActualizado.ContainsKey(factura.Id_encabezado.Value))
                {
                    registroProcesandoActualizado.Remove(factura.Id_encabezado.Value);

                    // Guardar el diccionario actualizado en el archivo temporal
                    string jsonOutput = JsonConvert.SerializeObject(registroProcesandoActualizado);
                    File.WriteAllText("registro_procesando.json", jsonOutput);
                }
                else
                {
                    Console.WriteLine($"No se encontró el registro con Id_encabezado {factura.Id_encabezado} en el archivo temporal 'registro_procesando.json'.");
                }

                // Actualizar el estado en la base de datos
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



        public void MarcarComoProcesado(int idEncabezado)
        {
            try
            {
                // Cargar el diccionario desde el archivo temporal, si existe
                Dictionary<int, EstadoProcesamiento> registroProcesandoActualizado;

                if (File.Exists("registro_procesando.json"))
                {
                    using (StreamReader reader = new StreamReader("registro_procesando.json"))
                    {
                        string json = reader.ReadToEnd();
                        registroProcesandoActualizado = JsonConvert.DeserializeObject<Dictionary<int, EstadoProcesamiento>>(json);
                    }
                }
                else
                {
                    throw new FileNotFoundException("El archivo temporal 'registro_procesando.json' no se encontró. No se puede continuar sin este archivo.");
                }

                // Eliminar el registro del diccionario
                if (registroProcesandoActualizado.ContainsKey(idEncabezado))
                {
                    registroProcesandoActualizado.Remove(idEncabezado);

                    // Guardar el diccionario actualizado en el archivo temporal
                    string jsonOutput = JsonConvert.SerializeObject(registroProcesandoActualizado);
                    File.WriteAllText("registro_procesando.json", jsonOutput);
                }
                else
                {
                    throw new KeyNotFoundException($"No se encontró el registro con ID: {idEncabezado} en el archivo temporal 'registro_procesando.json'.");
                }
            }
            catch (Exception ex)
            {
                // Manejar el error
                Console.WriteLine($"Error al marcar como procesado el registro: {ex.Message}");
            }
        }

       
    }
}

