using GeneradorCufe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.ViewModel
{
    public class GeneradorCufe_Cude
    {
        public static string ConstruirCadenaCUFE(Movimiento movimiento, List<Productos> listaProductos, Factura factura, string hora)
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
            //  string codigo4 = "06";
            //  string impuesto4 = "0.00";
            decimal total = movimiento.Valor;
            string nitFacturador = "1004994836";
            string numeroIdentificacionCliente = movimiento.Nit;
            string clavetecnica = "fc8eac422eba16e22ffd8c6f94b3f40a6e38162c";
            int tipodeambiente = 2;

            // Construir la cadena CUFE
            string cadenaCUFE = $"{numeroFactura}{fechaFactura}{horaFactura}{valorSubtotal}{codigo}{iva}{codigo2}{impuesto2}{codigo3}{impuesto3}{total}{nitFacturador}{numeroIdentificacionCliente}{clavetecnica}{tipodeambiente}";

            // Reemplazar comas por puntos en la cadena CUFE
            cadenaCUFE = cadenaCUFE.Replace(',', '.');

            return cadenaCUFE;
        }

        public static string ConstruirCadenaCUDE(Movimiento movimiento, List<Productos> listaProductos, Factura factura, string hora)
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
            //  string codigo4 = "06";
            //  string impuesto4 = "0.00";
            decimal total = movimiento.Valor;
            string nitFacturador = "1004994836";
            string numeroIdentificacionCliente = movimiento.Nit;
            string clavetecnica = "fc8eac422eba16e22ffd8c6f94b3f40a6e38162c";
            int tipodeambiente = 2;

            // Construir la cadena CUFE
            string cadenaCUFE = $"{numeroFactura}{fechaFactura}{horaFactura}{valorSubtotal}{codigo}{iva}{codigo2}{impuesto2}{codigo3}{impuesto3}{total}{nitFacturador}{numeroIdentificacionCliente}{clavetecnica}{tipodeambiente}";

            // Reemplazar comas por puntos en la cadena CUFE
            cadenaCUFE = cadenaCUFE.Replace(',', '.');

            return cadenaCUFE;
        }
    }
}
