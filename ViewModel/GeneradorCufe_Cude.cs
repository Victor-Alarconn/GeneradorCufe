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
        public static string ConstruirCadenaCUFE(Movimiento movimiento, List<Productos> listaProductos, Factura factura, string hora, string nit, Emisor emisor)
        {

            decimal Valor = 0;

            if (emisor.Retiene_emisor == 2 && movimiento.Retiene != 0)
            {
                Valor = Math.Round(movimiento.Valor + movimiento.Retiene, 2);
            }
            else
            {
                Valor = movimiento.Valor;
            }

            decimal consumo = Math.Round(listaProductos.Sum(p => p.Consumo), 2);
            decimal Iva = Math.Round(listaProductos.Sum(p => p.IvaTotal), 2);
            DateTimeOffset now = DateTimeOffset.Now;
            // Asegúrate de convertir los valores a los formatos correctos y de manejar posibles valores nulos
            string numeroFactura = factura.Facturas;
            string fechaFactura = movimiento.Fecha_Factura.ToString("yyyy-MM-dd");
            string horaFactura = hora;
            decimal valorSubtotal = movimiento.Valor_neto;
            string codigo = "01";
            decimal iva = Iva;
            string codigo2 = "04";
            decimal impuesto2 = consumo;
            string codigo3 = "03";
            string impuesto3 = "0.00";
            decimal total = Valor;
            string nitFacturador = nit;
            string numeroIdentificacionCliente = movimiento.Nit;
            string clavetecnica = "fc8eac422eba16e22ffd8c6f94b3f40a6e38162c";
            int tipodeambiente = 2;

            // Construir la cadena CUFE
            string cadenaCUFE = $"{numeroFactura}{fechaFactura}{horaFactura}{valorSubtotal}{codigo}{iva}{codigo2}{impuesto2}{codigo3}{impuesto3}{total}{nitFacturador}{numeroIdentificacionCliente}{clavetecnica}{tipodeambiente}";

            // Reemplazar comas por puntos en la cadena CUFE
            cadenaCUFE = cadenaCUFE.Replace(',', '.');

            return cadenaCUFE;
        }

        public static string ConstruirCadenaCUDE(Movimiento movimiento, List<Productos> listaProductos, Factura factura, string hora, string nit)
        {
            decimal consumo = Math.Round(listaProductos.Sum(p => p.Consumo), 2);
            decimal Iva = Math.Round(listaProductos.Sum(p => p.IvaTotal), 2);
            DateTimeOffset now = DateTimeOffset.Now;
            // Asegúrate de convertir los valores a los formatos correctos y de manejar posibles valores nulos
            string numeroFactura = factura.Facturas;
            string fechaFactura = movimiento.Fecha_Factura.ToString("yyyy-MM-dd");
            string horaFactura = hora;
            decimal valorSubtotal = movimiento.Valor_neto;
            string codigo = "01";
            decimal iva = Iva;
            string codigo2 = "04";
            decimal impuesto2 = consumo;
            string codigo3 = "03";
            string impuesto3 = "0.00";
            decimal total = movimiento.Valor;
            string nitFacturador = nit;
            string numeroIdentificacionCliente = movimiento.Nit;
            string Software_Pin = "75315";
            int tipodeambiente = 2;

            string cadenaCUDE = $"{numeroFactura}{fechaFactura}{horaFactura}{valorSubtotal}{codigo}{iva}{codigo2}{impuesto2}{codigo3}{impuesto3}{total}{nitFacturador}{numeroIdentificacionCliente}{Software_Pin}{tipodeambiente}";

            // Reemplazar comas por puntos en la cadena CUFE
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
