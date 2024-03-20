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

        public Adquiriente ConsultarAdquiriente() // Consulta para obtener los datos del adquiriente
        {
            Adquiriente adquiriente = new Adquiriente();

            string query = "SELECT tronombre, tronit, trociudad, trodirec FROM tabla_adquiriente LIMIT 1"; 

            using (MySqlConnection connection = _data.CreateConnection())
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            adquiriente.Nombre_adqu = reader["tronombre"].ToString();
                            adquiriente.Nit_adqui = reader["tronit"].ToString();
                            adquiriente.Nombre_municipio_adqui = reader["trociudad"].ToString();
                            adquiriente.Direccion_adqui = reader["trodirec"].ToString();
                            // Agrega aquí más asignaciones si hay más columnas en la tabla
                        }
                    }
                }
            }

            return adquiriente;
        }
    }
}
