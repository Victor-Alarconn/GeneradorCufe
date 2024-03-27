using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Model
{
    public class Productos
    {
        public string Codigo { get; set; }
        public string Recibo { get; set; }
        public int Nit { get; set; }
        public string Detalle { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Valor { get; set; }
        public decimal Neto { get; set; }
        public string Descuento { get; set; }
        public decimal Iva { get; set; }
        public string IvaTotal { get; set; }
        public string Total { get; set; }
    }
}
