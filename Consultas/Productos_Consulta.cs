using GeneradorCufe.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Consultas
{
    public class Productos_Consulta
    {
        private readonly Conexion.Data _data;


        public Productos_Consulta()
        {
            _data = new Conexion.Data("MySqlConnectionString");
        }

        public List<Productos> ConsultarProductosPorFactura(Factura factura, string cadenaConexion)
        {
            List<Productos> productos = new List<Productos>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion)) // Utilizar la cadena de conexión proporcionada
                {
                    // Abre la conexión
                    connection.Open();

                    // Define tu consulta SQL con las columnas específicas y el filtro por el valor de la factura
                    string query = "SELECT codigo, recibo, nit, detalle, cantidad, valor, neto, dsct4, iva, vriva, vrventa FROM xxxxmvin WHERE factura = @factura";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Agregar el parámetro para el valor de la factura
                        command.Parameters.AddWithValue("@factura", factura.Facturas);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Crear un nuevo objeto Productos y asignar los valores de las columnas
                                Productos producto = new Productos()
                                {
                                    Codigo = reader.GetString("codigo"),
                                    Recibo = reader.GetString("recibo"),
                                    Nit = reader.GetString("nit"),
                                    Detalle = reader.GetString("detalle"),
                                    Cantidad = reader.GetDecimal("cantidad"),
                                    Valor = reader.GetDecimal("valor"),
                                    Neto = reader.GetDecimal("neto"),
                                    Descuento = reader.GetDecimal("dsct4"),
                                    Iva = reader.GetDecimal("iva"),
                                    IvaTotal = reader.GetDecimal("vriva"),
                                    Total = reader.GetDecimal("vrventa")
                                };

                                // Agregar el producto a la lista
                                productos.Add(producto);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Manejar excepciones de MySQL
                Console.WriteLine("Error al consultar productos: " + ex.Message);
            }

            return productos;
        }



    }
}
