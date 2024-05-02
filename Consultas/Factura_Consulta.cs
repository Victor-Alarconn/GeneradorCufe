using GeneradorCufe.Conexion;
using GeneradorCufe.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GeneradorCufe.Consultas
{
    public class Factura_Consulta
    {
        private readonly Data _data;
        private readonly System.Timers.Timer _timer;
        private readonly Dictionary<int, EstadoProcesamiento> _registroProcesando;

        public Factura_Consulta()
        {
            _data = new Data();
            _timer = new System.Timers.Timer();
            _timer.Interval = 30000; // Intervalo en milisegundos (15 segundos)
            _timer.Elapsed += TimerElapsed; // Método que se ejecutará cuando el temporizador expire
            _timer.Start(); // Iniciar el temporizador
            _registroProcesando = new Dictionary<int, EstadoProcesamiento>();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)  // Método que se ejecuta cada vez que el temporizador expire
        {
            ConsultarBaseDatos();
        }

        //private void ConsultarBaseDatos()
        //{
        //    try
        //    {
        //        using (MySqlConnection connection = _data.CreateConnection())
        //        {
        //            connection.Open();

        //            string query = "SELECT id_enc, empresa, tipo_mvt, factura, recibo, aplica, nombre3, notas, terminal, ip_base FROM fac";

        //            using (MySqlCommand command = new MySqlCommand(query, connection))
        //            {
        //                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
        //                {
        //                    DataTable dataTable = new DataTable();
        //                    adapter.Fill(dataTable);

        //                    Emisor_Consulta emisorConsulta = new Emisor_Consulta();

        //                    foreach (DataRow row in dataTable.Rows)
        //                    {
        //                        Factura factura = new Factura
        //                        {
        //                            Id_encabezado = Convert.ToInt32(row["id_enc"] ?? 0),
        //                            Empresa = row["empresa"]?.ToString() ?? "",
        //                            Tipo_movimiento = row["tipo_mvt"]?.ToString() ?? "",
        //                            Facturas = row["factura"]?.ToString() ?? "",
        //                            Recibo = row["recibo"]?.ToString() ?? "",
        //                            Aplica = row["aplica"]?.ToString() ?? "",
        //                            Nombre = row["nombre3"]?.ToString() ?? "",
        //                            Notas = row["notas"]?.ToString() ?? "",
        //                            Terminal = row["terminal"]?.ToString() ?? "",
        //                            Ip_base = row["ip_base"]?.ToString() ?? ""
        //                        };

        //                        // Ejecuta la acción correspondiente según la IP base
        //                        if (factura.Ip_base == "200.118.190.213" || factura.Ip_base == "200.118.190.167")
        //                        {
        //                            emisorConsulta.EjecutarAccionParaIP(factura, "RmSoft20X", "*LiLo89*");
        //                        }
        //                        else if (factura.Ip_base == "192.190.42.191")
        //                        {
        //                            emisorConsulta.EjecutarAccionParaIP(factura, "root", "**qwerty**");
        //                        }
        //                        else
        //                        {
        //                            // Maneja el caso de IP base no reconocida
        //                            Console.WriteLine($"IP base no reconocida: {factura.Ip_base}");
        //                        }
        //                    }

        //                    Console.WriteLine("Consulta a la base de datos completada exitosamente.");
        //                }
        //            }

        //            connection.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error al consultar la base de datos: " + ex.Message);
        //    }
        //}

        private void ConsultarBaseDatos()
        {
            try
            {
                using (MySqlConnection connection = _data.CreateConnection())
                {
                    connection.Open();

                    string query = "SELECT id_enc, empresa, tipo_mvt, factura, recibo, aplica, nombre3, notas, terminal, ip_base FROM fac";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            Emisor_Consulta emisorConsulta = new Emisor_Consulta();

                            foreach (DataRow row in dataTable.Rows)
                            {
                                int idEncabezado = Convert.ToInt32(row["id_enc"]);

                                // Verificar si el registro está en proceso
                                if (!_registroProcesando.ContainsKey(idEncabezado) || !_registroProcesando[idEncabezado].Procesando)
                                {
                                    // Iniciar el procesamiento del registro
                                    _registroProcesando[idEncabezado] = new EstadoProcesamiento { Procesando = true, Intentos = 0 };
                                    ProcesarRegistro(row, emisorConsulta); // Llamada a método para procesar el registro
                                }
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al consultar la base de datos: " + ex.Message);
            }
        }

        private void ProcesarRegistro(DataRow row, Emisor_Consulta emisorConsulta)
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
                Terminal = row["terminal"]?.ToString() ?? "",
                Ip_base = row["ip_base"]?.ToString() ?? ""
            };

            try
            {
                if (factura.Ip_base == "200.118.190.213" || factura.Ip_base == "200.118.190.167")
                {
                    emisorConsulta.EjecutarAccionParaIP(factura, "RmSoft20X", "*LiLo89*");
                }
                else if (factura.Ip_base == "192.190.42.191")
                {
                    emisorConsulta.EjecutarAccionParaIP(factura, "root", "**qwerty**");
                }
                else
                {
                    // Maneja el caso de IP base no reconocida
                    Console.WriteLine($"IP base no reconocida: {factura.Ip_base}");
                }

                // Marcar el registro como procesado
                _registroProcesando[factura.Id_encabezado.Value].Procesando = false;
                Console.WriteLine($"Registro {factura.Id_encabezado} procesado correctamente.");
            }
            catch (Exception ex)
            {
                // Manejar el error y reintentar si es necesario
                Console.WriteLine($"Error al procesar el registro {factura.Id_encabezado}: {ex.Message}");
                ManejarError(factura.Id_encabezado.Value);
            }
        }



        private void ManejarError(int idEncabezado)
        {
            // Incrementar el número de intentos
            _registroProcesando[idEncabezado].Intentos++;

            int intentos = _registroProcesando[idEncabezado].Intentos;

            // Definir el límite máximo de intentos
            int maxIntentos = 5;

            if (intentos <= maxIntentos)
            {
                // Implementar una política de reintento exponencial
                TimeSpan retardo = TimeSpan.FromSeconds(Math.Pow(2, intentos)); // Retardo exponencial

                // Programar un nuevo intento después del retardo
                _timer.Interval = retardo.TotalMilliseconds;
                _timer.Start();

                Console.WriteLine($"Reintentando procesar el registro {idEncabezado} en {retardo.TotalSeconds} segundos (Intento {intentos}).");
            }
            else
            {
                // Excedido el límite de intentos, se registra un mensaje de error y se marca como no procesando
                Console.WriteLine($"Se ha excedido el límite de intentos para el registro {idEncabezado}. Se marca como no procesando.");
                _registroProcesando[idEncabezado].Procesando = false;
            }
        }


    }
}
