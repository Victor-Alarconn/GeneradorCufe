using GeneradorCufe.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Conexion
{
    public class Data
    {
        public readonly string _connectionString;

        public Data(string nombreBaseDatos = "MySqlConnectionString")
        {
            // Aquí defines las cadenas de conexión directamente en la clase
            if (nombreBaseDatos == "MySqlConnectionString")
            {
                _connectionString = "Database=facturas; Data Source=192.190.42.191; User Id=root; Password=**qwerty**; ConvertZeroDateTime=True;";
            }
            else if (nombreBaseDatos == "ArticulosConnectionString")
            {
                _connectionString = "Database=empresas; Data Source=192.190.42.191; User Id=root; Password=**qwerty**; ConvertZeroDateTime=True;";
            }
            else
            {
                throw new ArgumentException("Nombre de base de datos desconocido");
            }
        }

        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        //public static string ConstruirCadenaConexion(Factura factura)
        //{
        //    if (factura.Ip_base == "200.118.190.213" || factura.Ip_base == "200.118.190.167")
        //    {
        //        return $"Database={factura.Empresa.ToLower()}; Data Source={factura.Ip_base}; User Id=RmSoft20X;Password=*LiLo89*; ConvertZeroDateTime=True;";
        //    }
        //    else if (factura.Ip_base == "192.190.42.191")
        //    {
        //        return $"Database={factura.Empresa.ToLower()}; Data Source={factura.Ip_base}; User Id=root;Password=**qwerty**; ConvertZeroDateTime=True;";
        //    }
        //    return "";
        //}
    }
}
