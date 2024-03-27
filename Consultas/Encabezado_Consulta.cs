using GeneradorCufe.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Consultas
{
    public class Encabezado_Consulta
    {
        private readonly Conexion.Data _data;

        public Encabezado_Consulta()
        {
            _data = new Conexion.Data("MySqlConnectionString");
        }

        public Encabezado ConsultarEncabezado(Factura factura, string cadenaConexion)  // Consulta para obtener los datos del encabezado
        {
            Encabezado encabezado = new Encabezado();

            string query = "SELECT resol_fe, f_inicio, f_termina, r_inicio, r_termina, prefijo0 FROM xxxxterm WHERE terminal = @Terminal";

            using (MySqlConnection connection = new MySqlConnection(cadenaConexion)) // Utilizar la cadena de conexión proporcionada
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Terminal", factura.Terminal);

                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            encabezado.Autorizando = reader.GetString("resol_fe");
                            encabezado.Fecha_inicio = reader.GetDateTime("f_inicio");
                            encabezado.Fecha_termina = reader.GetDateTime("f_termina");
                            encabezado.R_inicio = reader.GetInt32("r_inicio");
                            encabezado.R_termina = reader.GetInt32("r_termina");
                            encabezado.Prefijo = reader.GetString("prefijo0");
                        }
                    }
                }
            }

            return encabezado;
        }

    }
}
