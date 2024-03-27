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

        public Movimiento ConsultarValoresTotales(string factura) // Consulta los valores totales de la factura
        {
            Movimiento movimiento = new Movimiento();

            string query = "SELECT valor, vriva, desctos, gravada, exentas FROM xxxxccfc WHERE factura = @factura";

            using (MySqlConnection connection = _data.CreateConnection())
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@factura", factura);

                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            movimiento.Valor = reader.GetDecimal("valor");
                            movimiento.Valor_iva = reader.GetDecimal("vriva");
                            movimiento.Valor_dsto = reader.GetDecimal("desctos");
                            movimiento.Valor_neto = reader.GetDecimal("gravada");
                            movimiento.Exentas = reader.GetDecimal("exentas");
                        }
                    }
                }
            }

            return movimiento;
        }

    }
}
