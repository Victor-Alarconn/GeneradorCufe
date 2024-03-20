using GeneradorCufe.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Consultas
{
    public class FormaPago_Consulta
    {
        private readonly Conexion.Data _data;

        public FormaPago_Consulta()
        {
            _data = new Conexion.Data("MySqlConnectionString");
        }

        public FormaPago ConsultarFormaPago(int idFormaPago)
        {
            FormaPago formaPago = new FormaPago();

            string query = "SELECT id_forma, codg_form, vlr_pag, fecha, factura, 3ros " +
                           "FROM forma_pago " +
                           "WHERE id_forma = @idFormaPago";

            using (MySqlConnection connection = _data.CreateConnection())
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idFormaPago", idFormaPago);

                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            formaPago.Id_forma = Convert.ToInt32(reader["id_forma"]);
                            formaPago.Codigo_forma = reader["codg_form"].ToString();
                            formaPago.Valor_pago = Convert.ToDecimal(reader["vlr_pag"]);
                            formaPago.Fecha_pago = Convert.ToDateTime(reader["fecha"]);
                            formaPago.Factura_pago = reader["factura"].ToString();
                            formaPago.Terceros_pago = Convert.ToInt32(reader["3ros"]);
                        }
                    }
                }
            }

            return formaPago;
        }
    }
}
