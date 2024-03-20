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

        public Encabezado ConsultarEncabezado(int idEncabezado)  // Consulta para obtener los datos del encabezado
        {
            Encabezado encabezado = new Encabezado();

            string query = "SELECT id_enc, factura, fecha, valor_net, vlr_iva, vlr_ipo, vlr_dsto, notas, cufe, cude " +
                           "FROM encabezado " +
                           "WHERE id_enc = @idEncabezado";

            using (MySqlConnection connection = _data.CreateConnection())
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idEncabezado", idEncabezado);

                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            encabezado.Id_enc = Convert.ToInt32(reader["id_enc"]);
                            encabezado.Factura_enc = reader["factura"].ToString();
                            encabezado.Fecha_enc = Convert.ToDateTime(reader["fecha"]);
                            encabezado.Valor_net = Convert.ToDecimal(reader["valor_net"]);
                            encabezado.Vlr_iva = Convert.ToDecimal(reader["vlr_iva"]);
                            encabezado.Vlr_ipo = Convert.ToDecimal(reader["vlr_ipo"]);
                            encabezado.Vlr_dsto = Convert.ToDecimal(reader["vlr_dsto"]);
                            encabezado.Notas = reader["notas"].ToString();
                            encabezado.Cufe = reader["cufe"].ToString();
                            encabezado.Cude = reader["cude"].ToString();
                        }
                    }
                }
            }

            return encabezado;
        }
    }
}
