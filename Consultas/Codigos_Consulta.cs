using GeneradorCufe.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Consultas
{
    public class Codigos_Consulta
    {
        private readonly Conexion.Data _data;

        public Codigos_Consulta()
        {
            _data = new Conexion.Data("ArticulosConnectionString");
        }

        public Codigos ConsultarCodigos(string ciudad)
        {
            Codigos codigo = new Codigos();

            string query = "SELECT citycodigo, citydepto FROM xxxxcity WHERE citynomb = @ciudad";

            using (MySqlConnection connection = _data.CreateConnection())
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ciudad", ciudad);

                    connection.Open();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            codigo.Codigo_Municipio = reader["citycodigo"].ToString();
                            codigo.Codigo_Departamento = reader["citydepto"].ToString();
                        }
                    }
                }
            }

            return codigo;
        }
    }
}
