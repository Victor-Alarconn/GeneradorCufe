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

        public List<FormaPago> ConsultarFormaPago(Factura factura, string cadenaConexion)
        {
            List<FormaPago> listaFormaPago = new List<FormaPago>();

            try
            {
                string query = @" SELECT bancop, vrpago FROM xxxxccpg  WHERE factura = @factura AND id_empresa = @Empresa"; // Añadir condición para id_empresa

                using (MySqlConnection connection = _data.CreateConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@factura", factura.Facturas); // Asignar valor a parámetro factura
                        command.Parameters.AddWithValue("@Empresa", factura.Empresa); // Asignar valor a parámetro empresa

                        connection.Open();
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                FormaPago formaPago = new FormaPago
                                {
                                    Id_forma = reader["bancop"].ToString(),
                                    Valor_pago = reader.GetDecimal("vrpago")
                                };
                                listaFormaPago.Add(formaPago); // Agregar objeto FormaPago a la lista
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

            return listaFormaPago;
        }





    }
}
