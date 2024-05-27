﻿using GeneradorCufe.Model;
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
                string query = @"
        SELECT 
            nit, valor, vriva, desctos, gravada, exentas, fcruce, hdigita, rfuente, consumo, nbolsa, vbolsa, dato_cufe, ncre, numero, vendedor, dias, electron 
        FROM 
            xxxxccfc 
        WHERE 
            factura = @factura AND id_empresa = @Empresa"; // Añadir condición para id_empresa

                using (MySqlConnection connection = _data.CreateConnection()) // Utilizar la cadena de conexión proporcionada
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@factura", factura.Facturas); // Asignar valor a parámetro factura
                        command.Parameters.AddWithValue("@Empresa", factura.Empresa); // Asignar valor a parámetro empresa

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                movimiento.Nit = reader["nit"].ToString();
                                movimiento.Valor = reader.IsDBNull(reader.GetOrdinal("valor")) ? 0 : reader.GetDecimal(reader.GetOrdinal("valor"));
                                movimiento.Valor_iva = reader.IsDBNull(reader.GetOrdinal("vriva")) ? 0 : reader.GetDecimal(reader.GetOrdinal("vriva"));
                                movimiento.Valor_dsto = reader.IsDBNull(reader.GetOrdinal("desctos")) ? 0 : reader.GetDecimal(reader.GetOrdinal("desctos"));
                                movimiento.Valor_neto = reader.IsDBNull(reader.GetOrdinal("gravada")) ? 0 : reader.GetDecimal(reader.GetOrdinal("gravada"));
                                movimiento.Exentas = reader.IsDBNull(reader.GetOrdinal("exentas")) ? 0 : reader.GetDecimal(reader.GetOrdinal("exentas"));
                                movimiento.Fecha_Factura = reader.GetDateTime(reader.GetOrdinal("fcruce"));
                                movimiento.Hora_dig = reader["hdigita"].ToString();
                                movimiento.Retiene = reader.IsDBNull(reader.GetOrdinal("rfuente")) ? 0 : reader.GetDecimal(reader.GetOrdinal("rfuente"));
                                movimiento.Ipoconsumo = reader.IsDBNull(reader.GetOrdinal("consumo")) ? 0 : reader.GetDecimal(reader.GetOrdinal("consumo"));
                                movimiento.Numero_bolsa = reader.IsDBNull(reader.GetOrdinal("nbolsa")) ? 0 : reader.GetDecimal(reader.GetOrdinal("nbolsa"));
                                movimiento.Valor_bolsa = reader.IsDBNull(reader.GetOrdinal("vbolsa")) ? 0 : reader.GetDecimal(reader.GetOrdinal("vbolsa"));
                                movimiento.Dato_Cufe = reader["dato_cufe"].ToString();
                                movimiento.Nota_credito = reader.IsDBNull(reader.GetOrdinal("ncre")) ? 0 : reader.GetDecimal(reader.GetOrdinal("ncre"));
                                movimiento.Numero = reader["numero"].ToString();
                                movimiento.Vendedor = reader["electron"].ToString(); // Obtener el nombre del vendedor directamente
                                movimiento.Dias = reader.IsDBNull(reader.GetOrdinal("dias")) ? 0 : reader.GetDecimal(reader.GetOrdinal("dias"));
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

            return movimiento;
        }






    }
}
