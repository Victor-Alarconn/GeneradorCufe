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

        public Adquiriente ConsultarAdquiriente(int nit)
        {
            Adquiriente adquiriente = new Adquiriente();


            string query = "SELECT tronombre, tronomb_2, troapel_1, troapel_2, trociudad, trodirec, troemail, troregimen FROM xxxx3ros WHERE tronit = @Nit LIMIT 1";

            using (MySqlConnection connection = _data.CreateConnection())
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

                            // Separar el municipio y el departamento
                            string municipioDepartamento = reader["trociudad"].ToString();
                            string[] partes = municipioDepartamento.Split(',');
                            if (partes.Length == 2)
                            {
                                adquiriente.Nombre_municipio_adqui = partes[0].Trim();
                                adquiriente.Nombre_departamento_adqui = partes[1].Trim();

                                // Consultar los códigos correspondientes al municipio y departamento
                                Codigos_Consulta codigosConsulta = new Codigos_Consulta();
                                Codigos codigos = codigosConsulta.ConsultarCodigos(municipioDepartamento);

                                // Establecer los códigos en el objeto adquiriente
                                adquiriente.Codigo_municipio_adqui = codigos.Codigo_Municipio;
                                adquiriente.Codigo_departamento_adqui = codigos.Codigo_Departamento;
                            }

                            adquiriente.Direccion_adqui = reader["trodirec"].ToString();
                            adquiriente.Correo_adqui = reader["troemail"].ToString();
                            adquiriente.Responsable = reader["troregimen"].ToString();
                            adquiriente.Nit_adqui = nit.ToString();
                            // Puedes agregar más asignaciones si hay más columnas en la tabla
                        }
                    }
                }
            }

            return adquiriente;
        }

    }
}
