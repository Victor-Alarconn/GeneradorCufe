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
                using (MySqlConnection connection = _data.CreateConnection())
                {
                    connection.Open();

                    // Consulta SQL principal, incluyendo la columna artiexclu
                    string query = @"
            SELECT 
                codigo, recibo, nit, detalle, cantidad, valor, neto, dsct4, iva, vriva, vrventa, consumo, artiexclu 
            FROM 
                xxxxmvin 
            WHERE 
                factura = @factura AND (recibo = '' OR recibo IS NULL)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@factura", factura.Facturas);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Crear un nuevo objeto Productos y asignar los valores de las columnas
                                Productos producto = new Productos()
                                {
                                    Codigo = reader.GetString("codigo"),
                                    Recibo = reader.IsDBNull(reader.GetOrdinal("recibo")) ? null : reader.GetString("recibo"),
                                    Nit = reader.GetString("nit"),
                                    Detalle = reader.GetString("detalle"),
                                    Cantidad = reader.GetDecimal("cantidad"),
                                    Valor = reader.GetDecimal("valor"),
                                    Neto = reader.GetDecimal("neto"),
                                    Descuento = reader.GetDecimal("dsct4"),
                                    Iva = reader.GetDecimal("iva"),
                                    IvaTotal = reader.GetDecimal("vriva"),
                                    Total = reader.GetDecimal("vrventa"),
                                    Consumo = reader.GetDecimal("consumo"),
                                    Excluido = reader.GetInt32("artiexclu") // Asignar el valor de artiexclu
                                };

                                // Guardar el producto en la lista
                                productos.Add(producto);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConError(factura, ex);
            }

            return productos;
        }





        public List<Productos> ConsultarProductosNota(Factura factura, string cadenaConexion)
        {
            List<Productos> productos = new List<Productos>();

            try
            {
                using (MySqlConnection connection = _data.CreateConnection()) // Utilizar la cadena de conexión proporcionada
                {
                    connection.Open();

                    // Define tu consulta SQL con las columnas específicas y el filtro por el valor de la factura
                    string query = "SELECT codigo, recibo, nit, detalle, cantidad, valor, neto, dsct4, iva, vriva, hdigita, vrventa, fecha, consumo, vrcmpant FROM xxxxmvin WHERE recibo = @factura";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Agregar el parámetro para el valor de la factura
                        command.Parameters.AddWithValue("@factura", factura.Recibo);

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
                                    Total = reader.GetDecimal("vrventa"),
                                    Consumo = reader.GetDecimal("consumo"),
                                    Hora_Digitada = reader["hdigita"].ToString(),
                                    Fecha = reader.GetDateTime("fecha"),
                                    Valor2 = reader.GetDecimal("vrcmpant")
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
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConError(factura, ex);
            }

            return productos;
        }

        public List<Productos> ConsultarProductosDebito(Factura factura, string cadenaConexion)
        {
            List<Productos> productos = new List<Productos>();

            try
            {
                using (MySqlConnection connection = _data.CreateConnection()) // Utilizar la cadena de conexión proporcionada
                {
                    connection.Open();

                    // Define tu consulta SQL con las columnas específicas y el filtro por el valor de la factura
                    string query = "SELECT codigo, recibo, nit, debitos, vriva, fdigitar FROM xxxxcmbt WHERE recibo = @factura AND tipo = 'W'";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Agregar el parámetro para el valor de la factura
                        command.Parameters.AddWithValue("@factura", factura.Recibo);

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
                                    Valor = reader.GetDecimal("debitos"),
                                    IvaTotal = reader.GetDecimal("vriva"),
                                    Hora_Digitada = DateTime.Now.ToString("HH:mm:ss"),
                                    Fecha = reader.GetDateTime("fdigitar"),
                                    Cantidad = 1.00M,
                                    Neto = reader.GetDecimal("debitos"),
                                    Consumo = 0.00M,
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
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConError(factura, ex);
            }

            return productos;
        }

    }
}
