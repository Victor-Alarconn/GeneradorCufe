using GeneradorCufe.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Consultas
{
    public class Movimiento_Consulta
    {
        private readonly Conexion.Data _data;

        public Movimiento_Consulta()
        {
            _data = new Conexion.Data("MySqlConnectionString");
        }

        public Movimiento ConsultarMovimiento(int idMovimiento) // Consulta para obtener los datos del movimiento de la factura
        {
            Movimiento movimiento = new Movimiento();

            string query = "SELECT id_mvt, tipo_fac, 3ro, factura, recibo, cod_arti, cantidad, detalle, vlr_uni, iva, ipocons, vlr_iva, vlr_ipo, dsto, vlr_dsto, fech_fac, hora_fac " +
                           "FROM xxxxmvt " +
                           "WHERE id_mvt = @idMovimiento";

            using (MySqlConnection connection = _data.CreateConnection())
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idMovimiento", idMovimiento);

                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            movimiento.Id_movimiento = Convert.ToInt32(reader["id_mvt"]);
                            movimiento.Tipo_factura = reader["tipo_fac"].ToString();
                            movimiento.Tercero_mvt = reader["3ro"].ToString();
                            movimiento.Factura_mvt = reader["factura"].ToString();
                            movimiento.Recibo = reader["recibo"].ToString();
                            movimiento.Codigo_Articulo = reader["cod_arti"].ToString();
                            movimiento.Cantidad = reader["cantidad"].ToString();
                            movimiento.Detalle = reader["detalle"].ToString();
                            movimiento.Valor_unidad = Convert.ToDecimal(reader["vlr_uni"]);
                            movimiento.Iva = Convert.ToDecimal(reader["iva"]);
                            movimiento.Ipoconsumo = Convert.ToDecimal(reader["ipocons"]);
                            movimiento.Valor_iva = Convert.ToDecimal(reader["vlr_iva"]);
                            movimiento.Valor_ipo = Convert.ToDecimal(reader["vlr_ipo"]);
                            movimiento.Descuento = reader["dsto"].ToString();
                            movimiento.Valor_dsto = Convert.ToDecimal(reader["vlr_dsto"]);
                            movimiento.Fecha_factura = Convert.ToDateTime(reader["fech_fac"]);
                            movimiento.Hora_factura = reader["hora_fac"].ToString();
                        }
                    }
                }
            }

            return movimiento;
        }

    }
}
