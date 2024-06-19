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
                string query = @"SELECT IFNULL(bancop, '00') AS bancop, IFNULL(vrpago, 0.00) AS vrpago 
                         FROM xxxxccpg  
                         WHERE factura = @factura AND id_empresa = @Empresa";

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

                // Si no se encontraron resultados, agregar valores predeterminados "00" y "0.00"
                if (listaFormaPago.Count == 0)
                {
                    listaFormaPago.Add(new FormaPago { Id_forma = "00", Valor_pago = 0.00m });
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
