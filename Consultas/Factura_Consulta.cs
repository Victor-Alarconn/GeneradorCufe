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
            try
            {
                using (MySqlConnection connection = _data.CreateConnection())
                {
                    // Abre la conexión
                    connection.Open();

                    // Define tu consulta SQL con las columnas específicas
                    string query = "SELECT id_enc, empresa, tipo_mvt, factura, recibo, aplica, nombre3, notas, terminal, ip_base FROM fac";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            foreach (DataRow row in dataTable.Rows)
                            {
                                // Mapea los datos a objetos de la clase Factura
                                Factura factura = new Factura
                                {
                                    id_encabezado = Convert.ToInt32(row["id_enc"] ?? 0),
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

                                // Verifica la IP base y ejecuta la acción correspondiente
                                if (factura.Ip_base == "200.118.190.213")
                                {
                                    // Realiza la acción específica para la primera IP
                                    Emisor_Consulta.EjecutarAccionParaIP(factura.Empresa, factura.Ip_base, "RmSoft20X", "**LiLo89**");
                                }
                                else if (factura.Ip_base == "200.118.190.167")
                                {
                                    // Realiza la acción específica para la segunda IP
                                    Emisor_Consulta.EjecutarAccionParaIP(factura.Empresa, factura.Ip_base, "RmSoft20X", "**LiLo89**");
                                }

                            }

                        };
                            
                            Console.WriteLine("Consulta a la base de datos completada exitosamente.");
                    }
                  // Cierra la conexión
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // Maneja cualquier excepción que pueda ocurrir durante la consulta
                Console.WriteLine("Error al consultar la base de datos: " + ex.Message);
            }
        }

    }
}
