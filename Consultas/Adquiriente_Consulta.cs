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

        public Adquiriente ConsultarAdquiriente(string nit, string cadenaConexion, Factura factura)
        {
            Adquiriente adquiriente = new Adquiriente();

            string query = @"SELECT tronombre, tronomb_2, troapel_1, troapel_2, trociudad, trodirec, troemail, troregimen, 
                                trodigito, trotp_3ro, trotelef, trocity, trotipo, tropagweb 
                            FROM 
                                xxxx3ros 
                            WHERE 
                                tronit = @Nit AND id_empresa = @Empresa 
                            LIMIT 1";

                using (MySqlConnection connection = _data.CreateConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nit", nit);
                        command.Parameters.AddWithValue("@Empresa", factura.Empresa);

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

                                // Verificar el DV
                                if (nit != "222222222222")
                                {
                                    int dvCalculado = Calcular(nit);
                                    int dvConsultado = int.Parse(reader["trodigito"].ToString());

                                if (dvCalculado != dvConsultado)
                                {
                                    throw new Exception("Error: El dígito de verificación no coincide con el NIT ingresado. Verifique la informacion del Adquiriente");
                                }

                            }
                        }
                        }
                    }
                }

            return adquiriente;
        }

        public static int Calcular(string nit)
        {
            int[] pesos = { 71, 67, 59, 53, 47, 43, 41, 37, 29, 23, 19, 17, 13, 7, 3 };
            int suma = 0;
            nit = nit.PadLeft(15, '0');

            for (int i = 0; i < 15; i++)
            {
                suma += (int)char.GetNumericValue(nit[i]) * pesos[i];
            }

            int modulo = suma % 11;
            return (modulo < 2) ? modulo : 11 - modulo;
        }





    }
}
