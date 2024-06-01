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

        public Encabezado ConsultarEncabezado(Factura factura, string cadenaConexion)
        {
            Encabezado encabezado = new Encabezado();

            try
            {
                string query = @"
        SELECT resol_fe, f_inicio, f_termina, r_inicio, r_termina, prefijo0, resolucion, notas, NOTA_FIN, llave_tecn 
        FROM xxxxterm 
        WHERE id_empresa = @Empresa AND terminal = @Terminal";

                using (MySqlConnection connection = _data.CreateConnection()) // Utilizar la cadena de conexión proporcionada
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Empresa", factura.Empresa);
                        command.Parameters.AddWithValue("@Terminal", factura.Terminal);

                        connection.Open();
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                encabezado.Autorizando = reader["resol_fe"] as string ?? string.Empty;
                                encabezado.Fecha_inicio = reader["f_inicio"] != DBNull.Value ? reader.GetDateTime("f_inicio") : DateTime.MinValue;
                                encabezado.Fecha_termina = reader["f_termina"] != DBNull.Value ? reader.GetDateTime("f_termina") : DateTime.MinValue;
                                encabezado.R_inicio = reader["r_inicio"] != DBNull.Value ? reader.GetInt32("r_inicio") : 0;
                                encabezado.R_termina = reader["r_termina"] != DBNull.Value ? reader.GetInt32("r_termina") : 0;
                                encabezado.Prefijo = reader["prefijo0"] as string ?? string.Empty;
                                encabezado.Resolucion = reader["resolucion"] as string ?? string.Empty;
                                encabezado.Notas = reader["notas"] as string ?? string.Empty;
                                encabezado.Nota_fin = reader["NOTA_FIN"] as string ?? string.Empty;
                                encabezado.Llave_tecnica = reader["llave_tecn"] as string ?? string.Empty;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConError(factura, ex);
            }

            return encabezado;
        }



    }
}
