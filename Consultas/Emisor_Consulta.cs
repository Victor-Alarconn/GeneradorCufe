using GeneradorCufe.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Consultas
{
    public class Emisor_Consulta
    {
        private readonly Conexion.Data _data;

        public Emisor_Consulta()
        {
            _data = new Conexion.Data("MySqlConnectionString");
        }



        public Emisor ConsultarEmisor() // Consulta para obtener los datos del emisor
        {
            Emisor emisor = new Emisor();

            string query = "SELECT emprnombre, emprnit, emprdirec, emprciuda FROM empresas LIMIT 1"; // Limitamos a un solo registro

            using (MySqlConnection connection = _data.CreateConnection())
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            emisor.Nombre_emisor = reader["emprnombre"].ToString();
                            emisor.Nit_emisor = reader["emprnit"].ToString();
                            emisor.Direccion_emisor = reader["emprdirec"].ToString();
                            emisor.Nombre_municipio_emisor = reader["emprciuda"].ToString();
                            
                        }
                    }
                }
            }

            return emisor;
        }
    }
}
