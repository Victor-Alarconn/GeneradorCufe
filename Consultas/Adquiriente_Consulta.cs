using GeneradorCufe.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Consultas
{
    public class Adquiriente_Consulta
    {
        private readonly Conexion.Data _data;

        public Adquiriente_Consulta()
        {
            _data = new Conexion.Data("MySqlConnectionString");
        }

        public Adquiriente ConsultarAdquiriente(string nit, string cadenaConexion)
        {
            Adquiriente adquiriente = new Adquiriente();

            string query = "SELECT tronombre, tronomb_2, troapel_1, troapel_2, trociudad, trodirec, troemail, troregimen, trodigito, trotp_3ro, trotelef, trocity, trotipo, tropagweb FROM xxxx3ros WHERE tronit = @Nit LIMIT 1";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion)) // Utilizar la cadena de conexión proporcionada
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nit", nit);

                        connection.Open();
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Concatenar los nombres y apellidos para formar el NombreCompleto
                                string nombreCompleto = $"{reader["tronombre"]} {reader["tronomb_2"]} {reader["troapel_1"]} {reader["troapel_2"]}".Trim();
                                adquiriente.Nombre_adqu = nombreCompleto;
                                adquiriente.Direccion_adqui = reader["trodirec"].ToString();
                                adquiriente.Correo_adqui = reader["troemail"].ToString();
                                adquiriente.Responsable = reader.GetDecimal("troregimen");
                                adquiriente.Dv_Adqui = reader["trodigito"].ToString();
                                adquiriente.Tipo_p = reader.GetDecimal("trotp_3ro");
                                adquiriente.Telefono_adqui = reader["trotelef"].ToString();
                                adquiriente.Nit_adqui = nit.ToString();
                                adquiriente.Tipo_doc = reader["trotipo"].ToString();
                                adquiriente.Correo2 = reader["tropagweb"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
              //  facturaConsulta.MarcarComoConError(factura, ex);
            }

            return adquiriente;
        }



    }
}
