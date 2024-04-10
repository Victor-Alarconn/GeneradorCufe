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

        public Movimiento ConsultarValoresTotales(Factura factura, string cadenaConexion) // Consulta los valores totales de la factura
        {
            Movimiento movimiento = new Movimiento();

            try
            {
                string query = "SELECT nit, valor, vriva, desctos, gravada, exentas, fcruce, hdigita, rfuente, consumo FROM xxxxccfc WHERE factura = @factura";

                using (MySqlConnection connection = new MySqlConnection(cadenaConexion)) // Utilizar la cadena de conexión proporcionada
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@factura", factura.Facturas);

                        connection.Open();
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                movimiento.Nit = reader["nit"].ToString();
                                movimiento.Valor = reader.GetDecimal("valor");
                                movimiento.Valor_iva = reader.GetDecimal("vriva");
                                movimiento.Valor_dsto = reader.GetDecimal("desctos");
                                movimiento.Valor_neto = reader.GetDecimal("gravada");
                                movimiento.Exentas = reader.GetDecimal("exentas");
                                movimiento.Fecha_Factura = reader.GetDateTime("fcruce");
                                movimiento.Hora_dig = reader["hdigita"].ToString();
                                movimiento.Retiene = reader.GetDecimal("rfuente");
                                movimiento.Ipoconsumo = reader.GetDecimal("consumo");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción
                Console.WriteLine("Error en la consulta de valores totales: " + ex.Message);
            }

            return movimiento;
        }

    }
}
