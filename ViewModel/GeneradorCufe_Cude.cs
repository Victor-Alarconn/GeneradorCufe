using GeneradorCufe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.ViewModel
{
    public class GeneradorCufe_Cude
    {
        public static string ConstruirCadenaCUFE(Movimiento movimiento, List<Productos> listaProductos, Factura factura, string hora, string nit, Emisor emisor, Encabezado encabezado)
        {
            decimal Valor = emisor.Retiene_emisor == 2 && movimiento.Retiene != 0 ? Math.Round(movimiento.Valor + movimiento.Retiene, 2) : movimiento.Valor;

            int ambiente = emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase) ? 1 : 2;

            decimal consumo = Math.Round(listaProductos.Sum(p => p.Consumo), 2);
            decimal Iva = Math.Round(listaProductos.Sum(p => p.IvaTotal), 2);
            decimal Neto = Math.Round(listaProductos.Sum(p => p.Neto), 2);

            string fechaFactura = (DateTime.Today - movimiento.Fecha_Factura).TotalDays > 2 ? movimiento.Fecha_Factura.ToString("yyyy-MM-dd") : DateTime.Today.ToString("yyyy-MM-dd");

            string cadenaCUFE = $"{factura.Facturas}{fechaFactura}{hora}{Neto}01{Iva}04{consumo}030.00{Valor}{nit}{movimiento.Nit}{encabezado.Llave_tecnica}{ambiente}";
            cadenaCUFE = cadenaCUFE.Replace(',', '.');

            return cadenaCUFE;
        }


        public static string ConstruirCadenaCUDE(Movimiento movimiento, List<Productos> listaProductos, Factura factura, string horaf, string nit, Emisor emisor, string hora, string prefijo)
        {
            decimal consumo = Math.Round(listaProductos.Sum(p => p.Consumo), 2);
            decimal Iva = Math.Round(listaProductos.Sum(p => p.IvaTotal), 2);
            decimal VlrNeto = Math.Round(listaProductos.Sum(p => p.Neto), 2);
            DateTime fechaProducto = listaProductos.FirstOrDefault()?.Fecha ?? DateTime.Today;
            DateTime fechaHoy = DateTime.Today;

            fechaProducto = (fechaHoy - fechaProducto).TotalDays < 2 ? fechaHoy : fechaProducto;
            string fechaNC = fechaProducto.ToString("yyyy-MM-dd");

            decimal Valor = emisor.Retiene_emisor == 2 && movimiento.Retiene != 0 ? Math.Round(movimiento.Nota_credito + 0, 2) : movimiento.Nota_credito;

            int ambiente = emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase) ? 1 : 2;

            string cadenaCUDE = $"{prefijo}{fechaNC}{horaf}{VlrNeto}01{Iva}04{consumo}030.00{Valor}{nit}{movimiento.Nit}753151{ambiente}";
            cadenaCUDE = cadenaCUDE.Replace(',', '.');

            return cadenaCUDE;
        }


        public static string GenerarCUFE(string cadenaCUFE)
        {
            using (SHA384 sha384 = SHA384.Create())
            {
                // Convertir la cadena en un array de bytes
                byte[] bytesCadena = Encoding.UTF8.GetBytes(cadenaCUFE);

                // Aplicar SHA-384
                byte[] hashBytes = sha384.ComputeHash(bytesCadena);

                // Convertir el resultado del hash en una cadena hexadecimal en minúsculas
                string cufe = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return cufe;
            }
        }
    }
}
